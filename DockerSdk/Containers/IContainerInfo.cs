using System;
using System.Collections.Generic;
using DockerSdk.Images;
using DockerSdk.Networks;

namespace DockerSdk.Containers
{
    /// <summary>
    /// Holds detailed information about a Docker container.
    /// </summary>
    /// <remarks>This class holds a snapshot in time. Its information is immutable once created.</remarks>
    public interface IContainerInfo : IContainer
    {
        /// <summary>
        /// Gets the timestamp for when the container was created.
        /// </summary>
        DateTimeOffset CreationTime { get; }

        /// <summary>
        /// Gets an error message for why the container exited. Not all exited containers will have this set.
        /// </summary>
        string? ErrorMessage { get; }

        /// <summary>
        /// Gets the main executable that the container runs. This is what <see cref="MainProcessId"/> points to.
        /// </summary>
        /// <remarks>
        /// This gives the actual executable run by the main process. Note that ENTRYPOINT/CMD directives in
        /// <em>shell</em> format are shorthands for running a shell, so the actual executable in that case will be
        /// whatever you specified for the SHELL directive (which defaults to <c>/bin/sh</c> for Linux or <c>cmd</c> for
        /// Windows).
        /// </remarks>
        /// <seealso cref="ExecutableArgs"/>
        string Executable { get; }

        /// <summary>
        /// Gets the arguments to the main executable that the container runs.
        /// </summary>
        /// <remarks>
        /// This gives the arguments to the actual executable run by the main process. Note that ENTRYPOINT/CMD
        /// directives in <em>shell</em> format are shorthands for running a shell, so the text provided to those
        /// directives will really be part of the arguments.
        /// </remarks>
        /// <seealso cref="Executable"/>
        IReadOnlyList<string> ExecutableArgs { get; }

        /// <summary>
        /// Gets the exit code of the main process. This property is non-null for containers in the Exited state, and
        /// null in all other states.
        /// </summary>
        long? ExitCode { get; }

        /// <summary>
        /// Gets the Docker image that the container was created from.
        /// </summary>
        IImage Image { get; }

        /// <summary>
        /// Gets a value indicating whether the container was in the "paused" state when the query was performed.
        /// </summary>
        /// <remarks>
        /// Caution: The container's status might change between when the daemon reads it and when this object is
        /// available for reading. Beware of race conditions.
        /// </remarks>
        bool IsPaused { get; }

        /// <summary>
        /// Gets a value indicating whether the container was in the "running" state when the query was performed.
        /// </summary>
        /// <remarks>
        /// Cautions:
        /// * Unlike the `docker container inspect` command, this property does not consider paused containers to be
        /// "running". Instead, this is equivalent to `State == ContainerStatus.Running`.
        /// * The container's status might change between when the daemon reads it and when this object is available for
        /// reading. Beware of race conditions.
        /// </remarks>
        /// <seealso cref="IsRunningOrPaused"/>
        /// <seealso cref="State"/>
        /// <seealso cref="ContainerStatus"/>
        bool IsRunning { get; }

        /// <summary>
        /// Gets a value indicating whether the container was in either the "running" or "paused" state when the query
        /// was performed.
        /// </summary>
        /// <remarks>
        /// Caution: The container's status might change between when the daemon reads it and when this object is
        /// available for reading. Beware of race conditions.
        /// </remarks>
        bool IsRunningOrPaused { get; }

        /// <summary>
        /// Gets the labels and their values that have been applied to the container.
        /// </summary>
        IReadOnlyDictionary<string, string> Labels { get; }

        /// <summary>
        /// Gets the process ID of the container's main process. This property is non-null for containers that are
        /// running or paused. and null in all other states.
        /// </summary>
        /// <remarks>The container will automatically exit when this process exits.</remarks>
        long? MainProcessId { get; }

        /// <summary>
        /// Gets the containe's name.
        /// </summary>
        ContainerName Name { get; }

        /// <summary>
        /// Gets a list of network endpoints on the container. A network end-point is the link between a container and a
        /// network.
        /// </summary>
        IReadOnlyList<INetworkEndpoint> NetworkEndpoints { get; }

        /// <summary>
        /// Gets a mapping from network names to network endpoints on the container. A network end-point is the link
        /// between a container and a network.
        /// </summary>
        IReadOnlyDictionary<NetworkName, INetworkEndpoint> NetworkEndpointsByNetworkName { get; }

        /// <summary>
        /// Gets a list of networks attached to the container.
        /// </summary>
        IReadOnlyList<INetwork> Networks { get; }

        /// <summary>
        /// Gets information about the network sandbox. The sandbox is in charge of the network endpoints.
        /// </summary>
        NetworkSandbox? NetworkSandbox { get; }

        /// <summary>
        /// Gets a value indicating whether the container was forcibly shut down by the Docker daemon due to an
        /// out-of-memory condition. This property is non-null for containers in the Killed state, and null in all other
        /// states.
        /// </summary>
        bool? RanOutOfMemory { get; }

        /// <summary>
        /// Gets the time when the container was most recently started, or null if the container has not been started
        /// yet.
        /// </summary>
        /// <remarks>
        /// Caution: It's possible for the <see cref="StopTime"/> to be earlier than this, if the container previously
        /// exited, was restarted, and has not yet exited since the most recent restart.
        /// </remarks>
        DateTimeOffset? StartTime { get; }

        /// <summary>
        /// Gets a value that summarizes the container's overall state at the time of the query.
        /// </summary>
        /// <remarks>
        /// The container's status might change between when the daemon reads it and when this object is available for
        /// reading. Beware of race conditions.
        /// </remarks>
        ContainerStatus State { get; }

        /// <summary>
        /// Gets the time when the container most recently exited, or null if the container has never exited.
        /// </summary>
        /// <remarks>
        /// Caution: It's possible for this to be non-null for a running/paused container and earlier than <see
        ///          cref="StartTime"/>. This is true if the container previously exited, was restarted, and has not yet
        /// exited since the most recent restart. I've also observed it being slightly before the start time even in
        /// a container that was never restarted--perhaps due to some timing bug in Docker.
        /// </remarks>
        DateTimeOffset? StopTime { get; }
    }
}
