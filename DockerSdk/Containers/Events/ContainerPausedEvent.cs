using DockerSdk.Events.Dto;

namespace DockerSdk.Containers.Events
{
    /// <summary>
    /// Indicates that the container has transitioned from the <see cref="ContainerStatus.Running"/> state to the <see
    /// cref="ContainerStatus.Paused"/> state. The container's process is simply not given any CPU time until it is
    /// unpaused.
    /// </summary>
    public class ContainerPausedEvent : ContainerEvent
    {
        internal ContainerPausedEvent(Message message) : base(message, ContainerEventType.Paused)
        {
        }
    }
}
