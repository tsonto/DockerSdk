namespace DockerSdk.Containers.Events
{
    /// <summary>
    /// The types of event that containers can raise in the Docker event system.
    /// </summary>
    public enum ContainerEventType
    {
        /// <summary>
        /// <para>The creation of a new container. It will be at the <see cref="ContainerStatus.Created"/> state.</para>
        /// <para>
        /// Events of this type are represented by the <see cref="ContainerCreatedEvent"/> class. Docker documentation
        /// calls this event type "create".
        /// </para>
        /// </summary>
        Created,

        /// <summary>
        /// <para>
        /// The container's main process has exited. This is a transition to the <see cref="ContainerStatus.Exited"/>
        /// state (such as from the <see cref="ContainerStatus.Running"/> of <see cref="ContainerStatus.Paused"/>
        /// state). If the process shut down due to a "stop" or "restart" command (as opposed to a "kill" command),
        /// Docker will also emit <see cref="StopCompleted"/>.
        /// </para>
        /// <para>
        /// Events of this type are represented by the <see cref="ContainerCreatedEvent"/> class. Docker documentation
        /// calls this event type "die".
        /// </para>
        /// </summary>
        Exited,

        /// <summary>
        /// <para>
        /// The container has transitioned from the <see cref="ContainerStatus.Running"/> state to the <see
        /// cref="ContainerStatus.Paused"/> state. The container's process is simply not given any CPU time until it is
        /// unpaused.
        /// </para>
        /// <para>
        /// Events of this type are represented by the <see cref="ContainerPausedEvent"/> class. Docker documentation
        /// calls this event type "pause".
        /// </para>
        /// </summary>
        Paused,

        /// <summary>
        /// <para>The container has been deleted.</para>
        /// <para>
        /// Events of this type are represented by the <see cref="ContainerDeletedEvent"/> class. Docker documentation
        /// calls this event type "destroy".
        /// </para>
        /// </summary>
        Deleted,

        /// <summary>
        /// <para>
        /// Docker has finished restarting the container, as though by a <c>docker container restart</c> CLI command,
        /// and has transitioned from the <see cref="ContainerStatus.Restarting"/> state to the <see
        /// cref="ContainerStatus.Running"/> state. The container might previously been running, stopped, paused, or
        /// even in the <see cref="ContainerStatus.Created"/> state.
        /// </para>
        /// <para>
        /// If you're only interested in state changes, and not which specific command caused them, it may be best to
        /// simply ignore this event type, since it's always preceded by <see cref="Started"/> anyway.
        /// </para>
        /// <para>
        /// Events of this type are represented by the <see cref="ContainerRestartCompletedEvent"/> class. Docker
        /// documentation calls this event type "restart".
        /// </para>
        /// </summary>
        RestartCompleted,

        /// <summary>
        /// <para>
        /// Docker has sent a signal to the container. Typically this is used to either ask the container's process to
        /// shut down or to force the container to end the process. It does not represent a state change, but may cause
        /// one.
        /// </para>
        /// <para>
        /// Events of this type are represented by the <see cref="ContainerSignalledEvent"/> class. Docker documentation
        /// calls this event type "kill", regardless which signal is sent--including signals that would not normally
        /// cause a process to end.
        /// </para>
        /// </summary>
        Signalled,

        /// <summary>
        /// <para>
        /// The container has begun its main process, transitioning to the <see cref="ContainerStatus.Running"/> state.
        /// The container might previously been in the <see cref="ContainerStatus.Exited"/> or <see
        /// cref="ContainerStatus.Created"/> state. Restart commands also issue this event in the second half of their
        /// action.
        /// </para>
        /// <para>
        /// Events of this type are represented by the <see cref="ContainerStartedEvent"/> class. Docker documentation
        /// calls this event type "start".
        /// </para>
        /// </summary>
        Started,

        /// <summary>
        /// <para>
        /// Docker has finished performing a "stop" operation, either on its own or in the first half of a restart
        /// operation.
        /// </para>
        /// <para>
        /// If you're only interested in state changes, and not which specific command caused them, it may be best to
        /// simply ignore this event type, since it's always preceded by <see cref="Exited"/> anyway.
        /// </para>
        /// <para>
        /// Docker "kill" commands do not emit this event; however, the presence of this command does not necessarily
        /// mean that the process shut down gracefully. In particular, if someone performs a "stop" operation on a
        /// paused container, it appears that the container will be only briefly unpaused before it's forcibly shut
        /// down. (Sequence of events: signal 15, unpause, signal 9, exit stop.)
        /// </para>
        /// <para>
        /// Events of this type are represented by the <see cref="ContainerStopCompletedEvent"/> class. Docker
        /// documentation calls this event type "stop".
        /// </para>
        /// </summary>
        StopCompleted,

        /// <summary>
        /// <para>The container has transitioned out of the <see cref="ContainerStatus.Paused"/> state.</para>
        /// <para>
        /// Note that an unpause event might be part of a sequence that ultimately stops or deletes the container.
        /// </para>
        /// <para>
        /// Events of this type are represented by the <see cref="ContainerUnpausedEvent"/> class. Docker documentation
        /// calls this event type "unpause".
        /// </para>
        /// </summary>
        Unpaused,
    }
}
