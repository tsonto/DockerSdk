using Message = Docker.DotNet.Models.Message;

namespace DockerSdk.Images.Events
{
    /// <summary>
    /// Represents a notification that a tag has been removed from a local image.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When Docker updates an existing tag to point to a different image, it emits this event but does not also emit an
    /// untag event for the old image.
    /// </para>
    /// <para>
    /// This event can be raised from the Docker CLI with either <c>docker image untag</c> or <c>docker image rm</c>.
    /// </para>
    /// </remarks>
    public class ImageTaggedEvent : ImageEvent
    {
        internal ImageTaggedEvent(Message message) : base(message, ImageEventType.Tagged)
        {
            ImageName = ImageName.Parse( message.Actor.Attributes["name"]);
        }

        /// <summary>
        /// Gets the ID of the tagged image.
        /// </summary>
        public ImageFullId ImageId => (ImageFullId)ImageReference;

        /// <summary>
        /// Gets the tag.
        /// </summary>
        public ImageName ImageName { get; }
    }
}
