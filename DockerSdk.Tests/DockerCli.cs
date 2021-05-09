using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Xunit.Abstractions;

namespace DockerSdk.Tests
{
    public sealed class DockerCli : IDisposable
    {
        public DockerCli(ITestOutputHelper toh) : this(s => toh.WriteLine(s)) { }

        public DockerCli(Action<string> writer)
        {
            this.writer = writer;
            scriptsRoot = Path.Join(Environment.CurrentDirectory, "scripts");
        }

        private static readonly Lazy<string?> dockerPath = new(FindDockerCommand);
        private readonly List<string> containersToRemove = new();
        private readonly List<string> imagesToRemove = new();
        private readonly string scriptsRoot;

        public string GetImageId(string image)
        {
            var name = image.Replace("ddnt:", "");
            var path = $"./scripts/{name}/image.id";
            return File.ReadAllLines(path)[0];
        }

        public string GetNetworkId(string network)
        {
            var name = network.Replace("ddnt-", "");
            var path = $"./scripts/{name}.network.id";
            return File.ReadAllLines(path)[0];
        }

        public string GetHostNetworkId() => Invoke("network ls --filter=driver=host --no-trunc --quiet")[0];

        public string GetNoneNetworkId() => Invoke("network ls --filter=driver=null --no-trunc --quiet")[0];

        public string GetContainerId(string container)
        {
            var name = container.Replace("ddnt-", "");
            var path = $"./scripts/{name}.id";
            return File.ReadAllLines(path)[0];
        }

        public string Run(string image, string? containerName = null, string args = "")
        {
            if (!string.IsNullOrEmpty(containerName))
                args += "--name " + containerName;
            var command = $"container run --detach {args} {image}";
            var id = Invoke(command)[0];
            containersToRemove.Add(id);
            return id;
        }

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

        public string Create(string name, string? image = null, string args = "")
        {
            image ??= name.Replace("ddnt-", "ddnt:");
            var command = $"container create {image} --name {name} {args}";
            var id = Invoke(command)[0];
            containersToRemove.Add(id);
            return id;
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

        public void Dispose()
        {
            foreach(string id in containersToRemove)
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

        private static string? FindDockerCommand()
            => Cli.Run("(get-command docker -CommandType Application).Path | Select-Object -First 1").FirstOrDefault();

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

        private void Write(string s) => writer(s);
    }
}
