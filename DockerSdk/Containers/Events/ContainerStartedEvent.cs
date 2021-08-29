using Message = DockerSdk.Core.Models.Message;

namespace DockerSdk.Containers.Events
{
    /// <summary>
    /// Represents a notice that the container has begun its main process, transitioning to the <see cref="ContainerStatus.Running"/> state.
    /// The container might previously been in the <see cref="ContainerStatus.Exited"/> or <see
    /// cref="ContainerStatus.Created"/> state. Restart commands also issue this event in the second half of their
    /// action.
    /// </summary>
    public class ContainerStartedEvent : ContainerEvent
    {
        internal ContainerStartedEvent(Message message) : base(message, ContainerEventType.Started) { }
    }
}
