using DockerSdk.Events.Dto;

namespace DockerSdk.Containers.Events
{
    /// <summary>
    /// Represents a notification of the creation of a new container. It will be at the <see
    /// cref="ContainerStatus.Created"/> state.
    /// </summary>
    public class ContainerCreatedEvent : ContainerEvent
    {
        internal ContainerCreatedEvent(Message message) : base(message, ContainerEventType.Created)
        {
        }
    }
}
