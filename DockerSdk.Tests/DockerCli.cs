using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using DockerSdk.Containers;
using DockerSdk.Images;
using DockerSdk.Networks;
using Xunit.Abstractions;

namespace DockerSdk.Tests
{
    public sealed class DockerCli : IDisposable
    {
        public DockerCli(ITestOutputHelper toh) : this(s => toh.WriteLine(s))
        {
        }

        public DockerCli(Action<string> writer)
        {
            this.writer = writer;
            scriptsRoot = Path.Join(Environment.CurrentDirectory, "scripts");
        }

        private static readonly Lazy<string?> dockerPath = new(FindDockerCommand);
        private readonly List<string> containersToRemove = new();
        private readonly List<string> imagesToRemove = new();
        private readonly string scriptsRoot;

        private readonly Action<string> writer;

        public string Build(string name, string? dockerfile = null, string args = "")
        {
            dockerfile ??= name.Replace("ddnt:", "");
            if (!Path.IsPathRooted(dockerfile))
                dockerfile = Path.Join(scriptsRoot, dockerfile);

            var command = $"image build \"{dockerfile}\" --tag \"{name}\" {args} --quiet";
            var id = Invoke(command)[0];

            imagesToRemove.Add(id);
            return id;
        }

        public void CleanUpContainer(IContainer? container)
            => CleanUpContainer(container?.Id?.ToString());

        public void CleanUpContainer(string? id)
        {
            if (!string.IsNullOrEmpty(id))
                _ = Invoke("container rm --force " + id, ignoreErrors: true);
        }

        public void CleanUpNetwork(INetwork? network)
            => CleanUpNetwork(network?.Id?.ToString());

        public string Create(string name, string? image = null, string args = "")
        {
            image ??= name.Replace("ddnt-", "ddnt:");
            var command = $"container create {image} --name {name} {args}";
            var id = Invoke(command)[0];
            containersToRemove.Add(id);
            return id;
        }

        public IContainer CreateAndStartContainer(DockerClient client, string? image = null, string? containerName = null, string args = "")
        {
            var container = CreateContainer(client, image, containerName, args);
            _ = Invoke($"container start {container.Id}");
            return container;
        }

        public IContainer CreateContainer(DockerClient client, string? image = null, string? containerName = null, string args = "")
        {
            if (image is null)
                image = "ddnt:infinite-loop";

            if (containerName is null)
            {
                containerName = "ddnt-" + Guid.NewGuid();
            }
            else
            {
                if (!containerName.StartsWith("ddnt-"))
                    throw new ArgumentException("Container name must start with ddnt- so the cleanup script will find it if needed.");
                _ = Invoke($"container rm --force {containerName}", ignoreErrors: true);
            }

            var containerId = Invoke($"container create --name {containerName} {args} {image}")[0];
            return new Container(client, new ContainerFullId(containerId));
        }

        public INetwork CreateNetwork(DockerClient client, string? networkName = null, string args = "")
        {
            if (networkName is null)
            {
                networkName = "ddnt-" + Guid.NewGuid();
            }
            else
            {
                if (!networkName.StartsWith("ddnt-"))
                    throw new ArgumentException("Network name must start with ddnt- so the cleanup script will find it if needed.");
                _ = Invoke($"network rm {networkName}", ignoreErrors: true);
            }

            var networkId = Invoke($"network create {args} {networkName}")[0];
            return new Network(client, new NetworkFullId(networkId));
        }

        public void Dispose()
        {
            foreach (string id in containersToRemove)
            {
                var command = $"container rm --force {id} --volumes";
                Invoke(command);
            }

            foreach (string id in imagesToRemove)
            {
                var command = $"image rm --force {id}";
                Invoke(command);
            }
        }

        public string GetBridgeNetworkId() => Invoke("network inspect bridge --format \"{{.ID}}\"")[0];

        public string GetContainerId(string container)
        {
            var name = container.Replace("ddnt-", "");
            var path = $"./scripts/{name}.id";
            return File.ReadAllLines(path)[0];
        }

        public string GetHostNetworkId() => Invoke("network ls --filter=driver=host --no-trunc --quiet")[0];

        public string GetImageId(string image)
        {
            var name = image.Replace("ddnt:", "");
            var path = $"./scripts/{name}/image.id";
            return File.ReadAllLines(path)[0];
        }

        public INetwork GetNetwork(DockerClient client, string network)
            => new Network(client, new NetworkFullId(GetNetworkId(network)));

        public IImage GetImage(DockerClient client, string? image = null)
            => new Image(client, new ImageFullId(GetImageId(image ?? "ddnt:inspect-me-1")));

        public string GetNetworkId(string network)
        {
            var name = network.Replace("ddnt-", "");
            var path = $"./scripts/{name}.network.id";
            return File.ReadAllLines(path)[0];
        }

        public string GetNoneNetworkId() => Invoke("network ls --filter=driver=null --no-trunc --quiet")[0];

        public string[] Invoke(string subcommand, bool ignoreErrors = false)
        {
            if (string.IsNullOrEmpty(dockerPath.Value))
                throw new InvalidOperationException($"Could not find the `docker` executable. Make sure that it's installed and in your PATH.");

            // Declare how to start the process.
            var pi = new ProcessStartInfo(dockerPath.Value, subcommand)
            {
                CreateNoWindow = true,
                UseShellExecute = false,  // needed for .Net Framework, which defaults it to true
                RedirectStandardInput = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
            };

            // Start the process.
            Process? process;
            try
            {
                process = Process.Start(pi);
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                throw new InvalidOperationException($"Failed to start the Docker CLI. Command: {pi.FileName} {pi.Arguments}", ex);
            }
            if (process is null)
            {
                throw new InvalidOperationException($"Failed to start the Docker CLI. Command: {pi.FileName} {pi.Arguments}");
            }

            List<string> output = new();
            bool hasErrors = false;

            using (process)
            {
                process.OutputDataReceived += Process_OutputDataReceived;
                process.BeginOutputReadLine();
                process.ErrorDataReceived += Process_ErrorDataReceived;
                process.BeginErrorReadLine();

                // Feed the command to PowerShell via stdin.
                Write(">> docker " + subcommand);

                // Wait for the process to end.
                process.WaitForExit();

                // Throw an exception if errors were detected. (Unless supressed.)
                if (!ignoreErrors)
                {
                    if (process.ExitCode != 0)
                        throw new InvalidOperationException($"The process exited with code {process.ExitCode}.");
                    if (hasErrors)
                        throw new InvalidOperationException($"The process emitted errors.");
                }

                return output.ToArray();
            }

            void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
            {
                if (e.Data is not null)
                {
                    Write(e.Data!);
                    output.AddRange(e.Data.Split(Environment.NewLine));
                }
            }

            void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
            {
                if (e.Data is not null)
                {
                    Write(e.Data!);
                    hasErrors = true;
                }
            }
        }

        public string Pull(string name)
        {
            _ = Invoke($"image pull --quiet \"{name}\"")[0];
            string id = Invoke($"image ls \"{name}\" --quiet --no-trunc")[0];
            imagesToRemove.Add(id);
            return id;
        }

        public void RemoveImageIfPresent(string name)
            => Invoke($"image rm --force \"{name}\"", ignoreErrors: true);

        public string Run(string image, string? containerName = null, string args = "")
        {
            if (!string.IsNullOrEmpty(containerName))
                args += "--name " + containerName;
            var command = $"container run --detach {args} {image}";
            var id = Invoke(command)[0];
            containersToRemove.Add(id);
            return id;
        }

        private static string? FindDockerCommand()
            => Cli.Run("(get-command docker -CommandType Application).Path | Select-Object -First 1").FirstOrDefault();

        private void CleanUpNetwork(string? id)
        {
            if (!string.IsNullOrEmpty(id))
                _ = Invoke("network rm " + id, ignoreErrors: true);
        }

        private void Write(string s) => writer(s);
    }
}
