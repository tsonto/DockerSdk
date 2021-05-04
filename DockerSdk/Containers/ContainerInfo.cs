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
    internal class ContainerInfo : Container, IContainerInfo
    {
        internal ContainerInfo(DockerClient docker, ContainerFullId id)
            : base(docker, id)
        {
        }

        /// <summary>
        /// Gets the timestamp for when the container was created.
        /// </summary>
        public DateTimeOffset CreationTime { get; init; }

        /// <summary>
        /// Gets an error message for why the container exited. Not all exited containers will have this set.
        /// </summary>
        public string? ErrorMessage { get; init; }

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
        public string Executable { get; init; }

        /// <summary>
        /// Gets the arguments to the main executable that the container runs.
        /// </summary>
        /// <remarks>
        /// This gives the arguments to the actual executable run by the main process. Note that ENTRYPOINT/CMD
        /// directives in <em>shell</em> format are shorthands for running a shell, so the text provided to those
        /// directives will really be part of the arguments.
        /// </remarks>
        /// <seealso cref="Executable"/>
        public IReadOnlyList<string> ExecutableArgs { get; init; }

        /// <summary>
        /// Gets the exit code of the main process. This property is non-null for containers in the Exited state, and
        /// null in all other states.
        /// </summary>
        public long? ExitCode { get; init; }

        /// <summary>
        /// Gets the container's full ID.
        /// </summary>
        public ContainerFullId Id { get; init; }

        /// <summary>
        /// Gets the Docker image that the container was created from.
        /// </summary>
        public Image Image { get; init; }

        /// <summary>
        /// Gets a value indicating whether the container was in the "paused" state when the query was performed.
        /// </summary>
        /// <remarks>
        /// Caution: The container's status might change between when the daemon reads it and when this object is
        /// available for reading. Beware of race conditions.
        /// </remarks>
        public bool IsPaused { get; init; }

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
        public bool IsRunning { get; init; }

        /// <summary>
        /// Gets a value indicating whether the container was in either the "running" or "paused" state when the query
        /// was performed.
        /// </summary>
        /// <remarks>
        /// Caution: The container's status might change between when the daemon reads it and when this object is
        /// available for reading. Beware of race conditions.
        /// </remarks>
        public bool IsRunningOrPaused { get; init; }

        /// <summary>
        /// Gets the labels and their values that have been applied to the container.
        /// </summary>
        public IReadOnlyDictionary<string, string> Labels { get; init; }

        /// <summary>
        /// Gets the process ID of the container's main process. This property is non-null for containers that are
        /// running or paused. and null in all other states.
        /// </summary>
        /// <remarks>The container will automatically exit when this process exits.</remarks>
        public long? MainProcessId { get; init; }

        /// <summary>
        /// Gets the containe's name.
        /// </summary>
        public ContainerName Name { get; init; }

        public IReadOnlyList<INetworkEndpoint> NetworkEndpoints { get; internal set; }

        public IReadOnlyDictionary<NetworkName, INetworkEndpoint> NetworkEndpointsByNetworkName { get; internal set; }

        public IReadOnlyList<INetwork> Networks { get; internal set; }

        public NetworkSandbox NetworkSandbox { get; init; }

        /// <summary>
        /// Gets a value indicating whether the container was forcibly shut down by the Docker daemon due to an
        /// out-of-memory condition. This property is non-null for containers in the Killed state, and null in all other
        /// states.
        /// </summary>
        public bool? RanOutOfMemory { get; init; }

        /// <summary>
        /// Gets the time when the container was most recently started, or null if the container has not been started
        /// yet.
        /// </summary>
        /// <remarks>
        /// Caution: It's possible for the <see cref="StopTime"/> to be earlier than this, if the container previously
        /// exited, was restarted, and has not yet exited since the most recent restart.
        /// </remarks>
        public DateTimeOffset? StartTime { get; init; }

        /// <summary>
        /// Gets a value that summarizes the container's overall state at the time of the query.
        /// </summary>
        /// <remarks>
        /// The container's status might change between when the daemon reads it and when this object is available for
        /// reading. Beware of race conditions.
        /// </remarks>
        public ContainerStatus State { get; init; }

        /// <summary>
        /// Gets the time when the container most recently exited, or null if the container has never exited.
        /// </summary>
        /// <remarks>
        /// Caution: It's possible for this to be non-null for a running/paused container and earlier than <see
        ///          cref="StartTime"/>. This is true if the container previously exited, was restarted, and has not yet
        /// exited since the most recent restart.
        /// </remarks>
        public DateTimeOffset? StopTime { get; init; }

        /*
         * TODO:
         * Mounts
         * SizeRootFs   include image's base size? bytes?
         * SizeRw       bytes?
         * GraphDriver
         * HostConfig
         * ExecIDs
         * AppArmorProfile  what is it? linux only?
         * ProcessLabel
         * MountLabel
         * Platform
         * Driver       fs driver?
         * RestartCount
         * Node
         * LogPath
         * HostsPath
         * HostnamePath
         * ResolvConfPath
         * Config:
         *      StopSignal
         *      OnBuild
         *      Restarting
         *      Dead
         *      Health
         * NetworkSettings
         * */
    }
}
