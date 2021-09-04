using DockerSdk.Events.Dto;

namespace DockerSdk.Containers.Events
{
    /// <summary>
    /// <para>Indicates that the container has transitioned out of the <see cref="ContainerStatus.Paused"/> state.</para>
    /// <para>
    /// Note that an unpause event might be part of a sequence that ultimately stops or deletes the container.
    /// </para>
    /// </summary>
    public class ContainerUnpausedEvent : ContainerEvent
    {
        internal ContainerUnpausedEvent(Message message) : base(message, ContainerEventType.Unpaused)
        {
        }
    }
}
