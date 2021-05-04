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
    internal static class ContainerLoader
    {
        internal static async Task<IContainer> LoadAsync(DockerClient docker, ContainerReference reference, ContainerLoadOptions options, LoadContext context, CancellationToken ct)
        {
            // If we know that we already have it, don't load it again.
            if (context.Containers.TryGet(reference, options.IncludeDetails, out IContainer? loadedContainer))
                return loadedContainer;

            // If we have a full ID and we haven't been asked to load any details, return a minimal object.
            if (!options.IncludeDetails && reference is ContainerFullId cid)
            {
                var minimal = new Container(docker, cid);
                context.Containers.Cache(minimal);
                return minimal;
            }

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

            // Create the majority of the resource object.
            var id = new ContainerFullId(response.ID);
            var state = Enum.Parse<ContainerStatus>(response.State.Status, ignoreCase: true);
            var isRunningOrPaused = state == ContainerStatus.Running || state == ContainerStatus.Paused;
            var output = new ContainerInfo(docker, id)
            {
                CreationTime = response.Created,
                ErrorMessage = string.IsNullOrEmpty(response.State.Error) ? null : response.State.Error,
                Executable = response.Path,
                ExecutableArgs = response.Args.ToImmutableArray(),
                ExitCode = state == ContainerStatus.Exited ? response.State.ExitCode : null,
                Image = new Image(docker, new ImageFullId(response.Image)),
                IsPaused = state == ContainerStatus.Paused,
                IsRunning = state == ContainerStatus.Running,
                IsRunningOrPaused = isRunningOrPaused,
                Labels = response.Config.Labels.ToImmutableDictionary(),
                MainProcessId = isRunningOrPaused ? response.State.Pid : null,
                Name = new ContainerName(response.Name),
                RanOutOfMemory = state == ContainerStatus.Dead ? response.State.OOMKilled : null,
                State = state,
                StartTime = ConvertDate(response.State.StartedAt),
                StopTime = ConvertDate(response.State.FinishedAt),
                NetworkSandbox = new NetworkSandbox(response.NetworkSettings.SandboxID, response.NetworkSettings.SandboxKey),
            };

            // Cache the object.
            context.Containers.Cache(output, reference, output.Name);

            // Build the endpoint objects. This may involve loading more information.
            Dictionary<NetworkName, INetworkEndpoint> endpointsByNetworkName = new();
            foreach (var kvp in response.NetworkSettings.Networks)
            {
                var netName = new NetworkName(kvp.Key);
                INetworkEndpoint ep = await NetworkEndpoint.LoadAsync(docker, kvp.Value, options?.NetworkLoadOptions, context, output, ct).ConfigureAwait(false);
                endpointsByNetworkName[netName] = ep;
            }

            // Add the endpoint information.
            output.NetworkEndpointsByNetworkName = endpointsByNetworkName.ToImmutableDictionary();
            output.NetworkEndpoints = endpointsByNetworkName.Select(kvp => kvp.Value).ToImmutableArray();
            output.Networks = output.NetworkEndpoints.Select(ep => ep.Network).ToImmutableArray();

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
    }
}
