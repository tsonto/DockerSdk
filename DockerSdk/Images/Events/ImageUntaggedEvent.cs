using Message = Docker.DotNet.Models.Message;

namespace DockerSdk.Images.Events
{
    /// <summary>
    /// Represents a notification that a tag has been removed from a local image.
    /// </summary>
    /// <remarks>
    /// When Docker updates an existing tag to point to a different image, it emits this event but does not also emit an
    /// untag event for the old image.
    /// </remarks>
    public class ImageUntaggedEvent : ImageEvent
    {
        internal ImageUntaggedEvent(Message message) : base(message, ImageEventType.Untagged)
        {
        }

        public ImageFullId? ImageId => ImageReference as ImageFullId;
    }
}
