using DockerSdk.Events;
using DockerSdk.Images;
using Message = Docker.DotNet.Models.Message;

namespace DockerSdk.Images.Events
{
    /// <summary>
    /// Represents an event emitted by the Docker daemon about a container.
    /// </summary>
    public abstract class ImageEvent : Event
    {
        internal ImageEvent(Message message, ImageEventType eventType)
            : base(message, EventSubjectType.Container)
        {
            EventType = eventType;
            ImageReference = ImageReference.Parse(message.Actor.ID);
        }

        /// <summary>
        /// Gets the type of event that this is.
        /// </summary>
        public ImageEventType EventType { get; }

        /// <summary>
        /// Gets a reference to the image.
        /// </summary>
        public ImageReference ImageReference { get; }

        internal static new Event? Wrap(Message message)
            => message.Action switch
            {
                "delete" => new ImageDeletedEvent(message),
                "load" => new ImageLoadedEvent(message),
                "pull" => new ImagePulledEvent(message),
                "push" => new ImagePushedEvent(message),
                "save" => new ImageSavedEvent(message),
                "tag" => new ImageTaggedEvent(message),
                "untag" => new ImageUntaggedEvent(message),
                _ => null
            };
    }
}
