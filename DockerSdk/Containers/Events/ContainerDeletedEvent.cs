using Message = Docker.DotNet.Models.Message;

namespace DockerSdk.Containers.Events
{
    /// <summary>
    /// Represents a notification of the container has been deleted.
    /// </summary>
    public class ContainerDeletedEvent : ContainerEvent
    {
        internal ContainerDeletedEvent(Message message) : base(message, ContainerEventType.Deleted)
        {
        }
    }
}
