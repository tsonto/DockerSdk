using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using DockerSdk.Containers.Dto;
using DockerSdk.Core;
using DockerSdk.Images;
using DockerSdk.Networks;
using DockerSdk.Networks.Dto;

namespace DockerSdk.Containers
{
    /// <summary>
    /// Settings for how to create a container and how it should behave.
    /// </summary>
    public class CreateContainerOptions
    {
        /// <summary>
        /// Gets options that are equivalent to the defaults for the `docker container create` command.
        /// </summary>
        public static CreateContainerOptions CommandLineDefaults => new()
        {
            PullCondition = PullImageCondition.IfMissing,
        };

        /// <summary>
        /// Gets or sets an override to the command that the container runs when it starts.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is approximately equivalent to the <c><em>command</em></c> argument when calling the Docker CLI as
        /// <c>docker run <em>image</em><em>command</em></c>, and is closely related to the CMD directive in
        /// Dockerfiles. The exact behavior of the command depends on the image. See the image's documentation for how
        /// it's meant to be used.
        /// </para>
        /// <para>This setting uses the <a href="https://stackoverflow.com/a/47940538/848976">"exec" form</a>.</para>
        /// </remarks>
        /// <seealso cref="Entrypoint"/>
        public string[]? Command { get; set; }

        /// <summary>
        /// Gets or sets the domain name to use for the container.
        /// </summary>
        public string? DomainName { get; set; }

        /// <summary>
        /// Gets or sets an override to the command that the container runs when it starts.
        /// </summary>
        /// <remarks>
        /// <para>
        /// For most purposes you should leave this null and use <see cref="Command"/> instead. Typically you would only
        /// use this for troubleshooting purposes.
        /// </para>
        /// <para>
        /// This is approximately equivalent to the <c>--entrypoint</c> parameter of the Docker CLI's <c>docker run</c>
        /// command, and is closely related to the ENTRYPOINT directive in Dockerfiles. The exact behavior of the
        /// command depends on the image. See the image's documentation for how it's meant to be used.
        /// </para>
        /// <para>This setting uses the <a href="https://stackoverflow.com/a/47940538/848976">"exec" form</a>.</para>
        /// </remarks>
        /// <seealso cref="Command"/>
        public string[]? Entrypoint { get; set; }

        /// <summary>
        /// Gets a <see cref="Dictionary{TKey, TValue}"/> for setting environment variables in the container. The key is
        /// the environment variable's name, and the value is the environment variable's value. If the value is null,
        /// the environment variable will be unset, which is different than the environment variable being blank.
        /// </summary>
        public Dictionary<string, string?> EnvironmentVariables { get; } = new();

        /// <summary>
        /// Gets or sets the action to perform when the container exits. The default is to do nothing.
        /// </summary>
        /// <remarks>
        /// Docker will wait 100ms before restarting the first time, then doubling each subsequent time.
        /// </remarks>
        /// <seealso cref="MaximumRetriesCount"/>
        public ContainerExitAction ExitAction { get; set; } = ContainerExitAction.None;

        /// <summary>
        /// Gets or sets the hostname to use for the container.
        /// </summary>
        public string? Hostname { get; set; }

        /// <summary>
        /// Gets or sets a value indicating which isolation technology to apply to the container. This property is only
        /// applicable on Windows hosts. Available values are: "default", "process", and "hyperv".
        /// </summary>
        public string? IsolationTech { get; set; }

        /// <summary>
        /// Gets or sets the upper limit on how many times Docker should attempt to restart. This property only applies
        /// if <see cref="ExitAction"/> is set to <see cref="ContainerExitAction.RestartOnFailure"/>.
        /// </summary>
        /// <seealso cref="ExitAction"/>
        public int? MaximumRetriesCount { get; set; }

        /// <summary>
        /// Gets or sets the name to give to the container. If this is null, Docker will assign a random name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets a list of port bindings to apply to the container.
        /// </summary>
        /// <remarks>A port binding connects a port within the container to a port on the host.</remarks>
        public List<PortBinding> PortBindings { get; } = new();

        /// <summary>
        /// Gets or sets a value indicating whether to pull the image prior to creating the container, and--if so--under
        /// what conditions. The default is to never pull the image, which means that the container creation operation
        /// will throw an <see cref="ImageNotFoundException"/> exception if the image is not already present.
        /// </summary>
        public PullImageCondition PullCondition { get; set; } = PullImageCondition.Never;

        /// <summary>
        /// Gets or sets a value indicating which user the container will run commands as. If this is null, it defaults
        /// to the USER from the dockerfile, which in turn defaults to <c>root</c>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This may be either a user name or a user ID. Optionally, it may also include a group name or group ID,
        /// separated from the user with a colon. (For example, "root:root".) Note that when specifying a group for the
        /// user, the user will have <em>only</em> the specified group membership. Any other configured group
        /// memberships will be ignored.
        /// </para>
        /// <para>
        /// If this value doesn't specify a group, and the user doesn't have a primary group, the default is the
        /// <c>root</c> group.
        /// </para>
        /// </remarks>
        public string? User { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to disable all networking within the container. The default is false.
        /// </summary>
        public bool DisableNetworking { get; set; }

        /// <summary>
        /// Gets a <see cref="Dictionary{TKey, TValue}"/> for setting labels on the container.
        /// </summary>
        public Dictionary<string, string> Labels { get; } = new();

        /// <summary>
        /// Gets the set of networks to attach to the container. In additiona to the networks mentioned here, Docker may
        /// also attach the built-in network named "bridge".
        /// </summary>
        public ISet<NetworkReference> Networks { get; } = new HashSet<NetworkReference>();

        // NOTE: Not adding ExposedPorts because I don't see a use case for it. (Note that exposing ports is different
        // than publishing ports, which is done via PortBindings.)

        internal string ToQueryString()
        {
            var builder = new QueryStringBuilder();
            builder.Set("name", Name);
            return builder.Build();
        }

        internal CreateContainerParameters ToBodyObject(string image)
            => new CreateContainerParameters
            {
                Image = image,
                Hostname = Hostname,
                Domainname = DomainName,
                User = User,
                HostConfig = new HostConfig
                {
                    PortBindings = MakePortBindings(PortBindings),
                    Isolation = IsolationTech,
                    AutoRemove = ExitAction == ContainerExitAction.Remove,
                    RestartPolicy = ExitAction switch
                    {
                        ContainerExitAction.Restart => new RestartPolicy { Name = RestartPolicyKind.Always },
                        ContainerExitAction.RestartUnlessStopped => new RestartPolicy { Name = RestartPolicyKind.UnlessStopped },
                        ContainerExitAction.RestartOnFailure => new RestartPolicy { Name = RestartPolicyKind.OnFailure, MaximumRetryCount = MaximumRetriesCount ?? 3 },
                        _ => new RestartPolicy { Name = RestartPolicyKind.No }
                    },
                },
                Entrypoint = Entrypoint,
                Cmd = Command,
                Env = MakeEnvironmentVariables(EnvironmentVariables),
                NetworkDisabled = DisableNetworking,
                Labels = Labels,
                NetworkingConfig = MakeNetworkConfigs(Networks),
            };

        internal static IDictionary<string, IList<Networks.Dto.PortBinding>> MakePortBindings(IEnumerable<PortBinding> portBindings)
        {
            return (from binding in portBindings
                    let key = GetKey(binding)
                    group binding.HostEndpoint by key into g
                    let hostParts = FormatEndpoints(g)
                    select new KeyValuePair<string, IList<Networks.Dto.PortBinding>>(g.Key, hostParts))
                   .ToDictionary();

            string GetKey(PortBinding binding)
                => binding.ContainerPort.ToStringI()
                + binding.Transport switch
                {
                    TransportType.Udp => "/udp",
                    TransportType.Tcp => "/tcp",
                    _ => ""
                };

            Networks.Dto.PortBinding[] FormatEndpoints(IEnumerable<IPEndPoint> endpoints)
                => endpoints.Select(FormatEndpoint).ToArray();

            Networks.Dto.PortBinding FormatEndpoint(IPEndPoint endpoint)
                => new()
                {
                    HostIP = endpoint.Address.ToString(),
                    HostPort = endpoint.Port.ToStringI(),
                };
        }

        private IList<string> MakeEnvironmentVariables(Dictionary<string, string?> environmentVariables)
        {
            if (environmentVariables.Keys.Any(k => k.Contains('=')))
                throw new ArgumentException("Environment variable names must not contain equal signs.");

            return environmentVariables
                   .Select(Convert)
                   .ToArray();

            static string Convert(KeyValuePair<string, string?> kvp) => kvp.Value is null ? kvp.Key : $"{kvp.Key}={kvp.Value}";
        }

        private NetworkingConfig? MakeNetworkConfigs(IEnumerable<NetworkReference> networks)
        {
            if (!networks.Any())
                return null;
            var x = networks.ToDictionary(net => net.ToString(), net => new EndpointSettings());
            return new NetworkingConfig
            {
                EndpointsConfig = x,
            };

        }
    }
}
