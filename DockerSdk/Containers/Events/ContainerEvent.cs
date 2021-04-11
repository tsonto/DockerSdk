using DockerSdk.Events;
using DockerSdk.Images;
using Message = Docker.DotNet.Models.Message;

namespace DockerSdk.Containers.Events
{
    /// <summary>
    /// Represents an event emitted by the Docker daemon about a container.
    /// </summary>
    public abstract class ContainerEvent : Event
    {
        internal ContainerEvent(Message message, ContainerEventType eventType)
            : base(message, EventSubjectType.Container)
        {
            EventType = eventType;
            ContainerId = new ContainerFullId(message.Actor.ID);

            if (message.Actor.Attributes.TryGetValue("name", out var nameString))
                if (ContainerName.TryParse(nameString, out var name))
                    ContainerName = name;

            if (message.Actor.Attributes.TryGetValue("image", out var imageString))
                if (ImageReference.TryParse(imageString, out var image))
                    ImageReference = image;
        }

        /// <summary>
        /// Gets the container's ID.
        /// </summary>
        public ContainerFullId ContainerId { get; }

        /// <summary>
        /// Gets the container's name, if it was included in the event details.
        /// </summary>
        public ContainerName? ContainerName { get; }

        /// <summary>
        /// Gets the type of event that this is.
        /// </summary>
        public ContainerEventType EventType { get; }

        /// <summary>
        /// Gets a reference to the container's image, if it was included in the event details.
        /// </summary>
        public ImageReference? ImageReference { get; }

        internal static new Event? Wrap(Message message)
            => message.Action switch
            {
                "create" => new ContainerCreatedEvent(message),
                "die" => new ContainerExitedEvent(message),
                "destroy" => new ContainerDeletedEvent(message),
                "kill" => new ContainerSignalledEvent(message),
                "pause" => new ContainerPausedEvent(message),
                "restart" => new ContainerRestartCompletedEvent(message),
                "rm" => new ContainerDeletedEvent(message),
                "start" => new ContainerStartedEvent(message),
                "stop" => new ContainerStopCompletedEvent(message),
                "unpause" => new ContainerUnpausedEvent(message),
                _ => null
            };
    }
}
