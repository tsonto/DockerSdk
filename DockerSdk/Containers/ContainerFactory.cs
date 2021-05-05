using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DockerSdk.Images;
using DockerSdk.Networks;
using Core = Docker.DotNet;
using CoreModels = Docker.DotNet.Models;

namespace DockerSdk.Containers
{
    internal static class ContainerFactory
    {
        internal static async Task<IContainer> LoadAsync(DockerClient docker, ContainerReference reference, CancellationToken ct)
        {
            CoreModels.ContainerInspectResponse raw = await LoadCoreAsync(docker, reference, ct).ConfigureAwait(false);

            return new Container(docker, new ContainerFullId(raw.ID));
        }

        internal static async Task<IContainerInfo> LoadInfoAsync(DockerClient docker, ContainerReference reference, CancellationToken ct)
        {
            CoreModels.ContainerInspectResponse raw = await LoadCoreAsync(docker, reference, ct).ConfigureAwait(false);

            // Get values for the constructor.
            var id = new ContainerFullId(raw.ID);
            var name = new ContainerName(raw.Name);
            var image = new Image(docker, new ImageFullId(raw.Image));

            // Synthesize values we'll need multiple times in this method.
            var container = new Container(docker, id);
            var state = Enum.Parse<ContainerStatus>(raw.State.Status, ignoreCase: true);
            var isRunningOrPaused = state == ContainerStatus.Running || state == ContainerStatus.Paused;

            // Build the endpoint objects.
            Dictionary<NetworkName, INetworkEndpoint> endpointsByNetworkName = new();
            foreach (var kvp in raw.NetworkSettings.Networks)
            {
                var netName = new NetworkName(kvp.Key);
                INetworkEndpoint ep = NetworkEndpointFactory.Create(docker, kvp.Value, container);
                endpointsByNetworkName[netName] = ep;
            }
            var endpoints = endpointsByNetworkName.Select(kvp => kvp.Value).ToImmutableArray();
            var networks = endpointsByNetworkName.Select(kvp => kvp.Value).Select(ep => ep.Network).ToImmutableArray();

            // Build the network sandbox.
            var sandbox = new NetworkSandbox(raw.NetworkSettings.SandboxID, raw.NetworkSettings.SandboxKey);

            // Create the container object
            var output = new ContainerInfo(docker, id, name, image)
            {
                CreationTime = raw.Created,
                ErrorMessage = string.IsNullOrEmpty(raw.State.Error) ? null : raw.State.Error,
                Executable = raw.Path,
                ExecutableArgs = raw.Args.ToImmutableArray(),
                ExitCode = state == ContainerStatus.Exited ? raw.State.ExitCode : null,
                IsPaused = state == ContainerStatus.Paused,
                IsRunning = state == ContainerStatus.Running,
                IsRunningOrPaused = isRunningOrPaused,
                Labels = raw.Config.Labels.ToImmutableDictionary(),
                MainProcessId = isRunningOrPaused ? raw.State.Pid : null,
                RanOutOfMemory = state == ContainerStatus.Dead ? raw.State.OOMKilled : null,
                State = state,
                StartTime = ConvertDate(raw.State.StartedAt),
                StopTime = ConvertDate(raw.State.FinishedAt),
                NetworkSandbox = sandbox,
                NetworkEndpointsByNetworkName = endpointsByNetworkName.ToImmutableDictionary(),
                NetworkEndpoints = endpoints,
                Networks = networks,
            };

            return output;
        }

        private static DateTimeOffset? ConvertDate(string input)
        {
            if (string.IsNullOrEmpty(input))
                return null;
            var parsed = DateTimeOffset.Parse(input);
            if (parsed == default)
                return null;
            return parsed;
        }

        private static async Task<CoreModels.ContainerInspectResponse> LoadCoreAsync(DockerClient docker, ContainerReference reference, CancellationToken ct)
        {
            // Call the Docker API to load the resource.
            CoreModels.ContainerInspectResponse response;
            try
            {
                response = await docker.Core.Containers.InspectContainerAsync(reference, ct).ConfigureAwait(false);
            }
            catch (Core.DockerApiException ex)
            {
                if (ContainerNotFoundException.TryWrap(ex, reference, out var cnfEx))
                    throw cnfEx;
                throw DockerException.Wrap(ex);
            }

            return response;
        }
    }
}
