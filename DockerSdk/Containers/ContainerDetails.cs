using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DockerSdk.Images;
using DockerSdk.Networks;
using CodeModels = Docker.DotNet.Models;

namespace DockerSdk.Containers
{
    /// <summary>
    /// Holds detailed information about a Docker container.
    /// </summary>
    /// <remarks>This class holds a snapshot in time. Its information is immutable once created.</remarks>
    public class ContainerDetails
    {
        internal ContainerDetails(DockerClient docker, CodeModels.ContainerInspectResponse response)
        {
            Id = new ContainerFullId(response.ID);

            CreationTime = response.Created;
            ErrorMessage = string.IsNullOrEmpty(response.State.Error) ? null : response.State.Error;
            Executable = response.Path;
            ExecutableArgs = response.Args.ToImmutableArray();
            ExitCode = State == ContainerStatus.Exited ? response.State.ExitCode : null;
            Image = new Image(docker, new ImageFullId(response.Image));
            IsPaused = State == ContainerStatus.Paused;
            IsRunning = State == ContainerStatus.Running;
            IsRunningOrPaused = State == ContainerStatus.Running || State == ContainerStatus.Paused;
            Labels = response.Config.Labels.ToImmutableDictionary();
            MainProcessId = IsRunningOrPaused ? response.State.Pid : null;
            Name = new ContainerName(response.Name);
            RanOutOfMemory = State == ContainerStatus.Dead ? response.State.OOMKilled : null;
            State = Enum.Parse<ContainerStatus>(response.State.Status, ignoreCase: true);
            StartTime = ConvertDate(response.State.StartedAt);
            StopTime = ConvertDate(response.State.FinishedAt);
            NetworkSandbox = new NetworkSandbox(response.NetworkSettings.SandboxID, response.NetworkSettings.SandboxKey);

            NetworkEndpointsByNetworkName = response.NetworkSettings.Networks.ToImmutableDictionary(
                kvp => new NetworkName(kvp.Key),
                kvp => new NetworkEndpoint(docker, kvp.Value, Id));
            NetworkEndpoints = NetworkEndpointsByNetworkName.Values.ToImmutableArray();
            Networks = NetworkEndpoints.Select(ep => ep.Network).ToImmutableArray();
        }

        /// <summary>
        /// Gets the timestamp for when the container was created.
        /// </summary>
        public DateTimeOffset CreationTime { get; }

        /// <summary>
        /// Gets an error message for why the container exited. Not all exited containers will have this set.
        /// </summary>
        public string? ErrorMessage { get; }

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
        public string Executable { get; }

        /// <summary>
        /// Gets the arguments to the main executable that the container runs.
        /// </summary>
        /// <remarks>
        /// This gives the arguments to the actual executable run by the main process. Note that ENTRYPOINT/CMD directives in
        /// <em>shell</em> format are shorthands for running a shell, so the text provided to those directives will really be
        /// part of the arguments.
        /// </remarks>
        /// <seealso cref="Executable"/>
        public IReadOnlyList<string> ExecutableArgs { get; }

        /// <summary>
        /// Gets the exit code of the main process. This property is non-null for containers in the Exited state, and
        /// null in all other states.
        /// </summary>
        public long? ExitCode { get; }

        /// <summary>
        /// Gets the container's full ID.
        /// </summary>
        public ContainerFullId Id { get; }

        /// <summary>
        /// Gets the Docker image that the container was created from.
        /// </summary>
        public Image Image { get; }

        /// <summary>
        /// Gets a value indicating whether the container was in the "paused" state when the query was performed.
        /// </summary>
        /// <remarks>
        /// Caution: The container's status might change between when the daemon reads it and when this object is
        /// available for reading. Beware of race conditions.
        /// </remarks>
        public bool IsPaused { get; }

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
        public bool IsRunning { get; }

        /// <summary>
        /// Gets a value indicating whether the container was in either the "running" or "paused" state when the query
        /// was performed.
        /// </summary>
        /// <remarks>
        /// Caution: The container's status might change between when the daemon reads it and when this object is
        /// available for reading. Beware of race conditions.
        /// </remarks>
        public bool IsRunningOrPaused { get; }

        /// <summary>
        /// Gets the labels and their values that have been applied to the container.
        /// </summary>
        public IReadOnlyDictionary<string, string> Labels { get; }

        /// <summary>
        /// Gets the process ID of the container's main process. This property is non-null for containers that are
        /// running or paused. and null in all other states.
        /// </summary>
        /// <remarks>The container will automatically exit when this process exits.</remarks>
        public long? MainProcessId { get; }

        /// <summary>
        /// Gets the containe's name.
        /// </summary>
        public ContainerName Name { get; }

        /// <summary>
        /// Gets a value indicating whether the container was forcibly shut down by the Docker daemon due to an
        /// out-of-memory condition. This property is non-null for containers in the Killed state, and null in all other
        /// states.
        /// </summary>
        public bool? RanOutOfMemory { get; }

        /// <summary>
        /// Gets the time when the container was most recently started, or null if the container has not been started
        /// yet.
        /// </summary>
        /// <remarks>
        /// Caution: It's possible for the <see cref="StopTime"/> to be earlier than this, if the container previously
        /// exited, was restarted, and has not yet exited since the most recent restart.
        /// </remarks>
        public DateTimeOffset? StartTime { get; }

        /// <summary>
        /// Gets a value that summarizes the container's overall state at the time of the query.
        /// </summary>
        /// <remarks>
        /// The container's status might change between when the daemon reads it and when this object is available for
        /// reading. Beware of race conditions.
        /// </remarks>
        public ContainerStatus State { get; }

        /// <summary>
        /// Gets the time when the container most recently exited, or null if the container has never exited.
        /// </summary>
        /// <remarks>
        /// Caution: It's possible for this to be non-null for a running/paused container and earlier than <see
        ///          cref="StartTime"/>. This is true if the container previously exited, was restarted, and has not yet
        /// exited since the most recent restart.
        /// </remarks>
        public DateTimeOffset? StopTime { get; }

        public IReadOnlyDictionary<NetworkName, NetworkEndpoint> NetworkEndpointsByNetworkName { get; }
        public IReadOnlyList<NetworkEndpoint> NetworkEndpoints { get; }
        public IReadOnlyList<Network> Networks { get; }
        public NetworkSandbox NetworkSandbox { get; }

        private static DateTimeOffset? ConvertDate(string input)
        {
            if (string.IsNullOrEmpty(input))
                return null;
            var parsed = DateTimeOffset.Parse(input);
            if (parsed == default)
                return null;
            return parsed;
        }

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
