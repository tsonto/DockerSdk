using DockerSdk.Events.Dto;

namespace DockerSdk.Containers.Events
{
    /// <summary>
    /// <para>
    /// Indicates that Docker has finished performing a "stop" operation, either on its own or in the first half of a
    /// restart operation.
    /// </para>
    /// <para>
    /// If you're only interested in state changes, and not which specific command caused them, it may be best to
    /// simply ignore this event type, since it's always preceded by <see cref="ContainerExitedEvent"/> anyway.
    /// </para>
    /// <para>
    /// Docker "kill" commands do not emit this event; however, the presence of this command does not necessarily
    /// mean that the process shut down gracefully. In particular, if someone performs a "stop" operation on a
    /// paused container, it appears that the container will be only briefly unpaused before it's forcibly shut
    /// down. (Sequence of events: signal 15, unpause, signal 9, exit stop.)
    /// </para>
    /// </summary>
    public class ContainerStopCompletedEvent : ContainerEvent
    {
        internal ContainerStopCompletedEvent(Message message) : base(message, ContainerEventType.StopCompleted) { }
    }
}
