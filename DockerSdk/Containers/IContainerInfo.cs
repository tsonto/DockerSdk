using System;
using System.Collections.Generic;
using DockerSdk.Images;
using DockerSdk.Networks;

namespace DockerSdk.Containers
{
    public interface IContainerInfo : IContainer
    {
        DateTimeOffset CreationTime { get; }
        string? ErrorMessage { get; }
        string Executable { get; }
        IReadOnlyList<string> ExecutableArgs { get; }
        long? ExitCode { get; }
        Image Image { get; }
        bool IsPaused { get; }
        bool IsRunning { get; }
        bool IsRunningOrPaused { get; }
        IReadOnlyDictionary<string, string> Labels { get; }
        long? MainProcessId { get; }
        ContainerName Name { get; }
        IReadOnlyList<INetworkEndpoint> NetworkEndpoints { get; }
        IReadOnlyDictionary<NetworkName, INetworkEndpoint> NetworkEndpointsByNetworkName { get; }
        IReadOnlyList<INetwork> Networks { get; }
        NetworkSandbox NetworkSandbox { get; }
        bool? RanOutOfMemory { get; }
        DateTimeOffset? StartTime { get; }
        ContainerStatus State { get; }
        DateTimeOffset? StopTime { get; }
    }
}
