using DockerSdk.Events.Dto;

namespace DockerSdk.Containers.Events
{
    /// <summary>
    /// <para>
    /// Indicates that Docker has finished restarting the container, as though by a <c>docker container restart</c> CLI command,
    /// and has transitioned from the <see cref="ContainerStatus.Restarting"/> state to the <see
    /// cref="ContainerStatus.Running"/> state. The container might previously been running, stopped, paused, or
    /// even in the <see cref="ContainerStatus.Created"/> state.
    /// </para>
    /// <para>
    /// If you're only interested in state changes, and not which specific command caused them, it may be best to
    /// simply ignore this event type, since it's always preceded by <see cref="ContainerStartedEvent"/> anyway.
    /// </para>
    /// </summary>
    public class ContainerRestartCompletedEvent : ContainerEvent
    {
        internal ContainerRestartCompletedEvent(Message message) : base(message, ContainerEventType.RestartCompleted) { }
    }
}
