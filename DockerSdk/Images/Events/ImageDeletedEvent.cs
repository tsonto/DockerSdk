using DockerSdk.Events.Dto;

namespace DockerSdk.Images.Events
{
    /// <summary>
    /// Represents a notification of the deletion of an image.
    /// </summary>
    /// <remarks>
    /// Docker CLI command <c>docker image rm</c> does not always delete the image. If you supply an image name to it
    /// rather than an image ID, it will usually (always?) untag the image instead.
    /// </remarks>
    /// <seealso cref="ImageUntaggedEvent"/>
    public class ImageDeletedEvent : ImageEvent
    {
        internal ImageDeletedEvent(Message message) : base(message, ImageEventType.Deleted)
        {
        }

        /// <summary>
        /// Gets the image's full ID.
        /// </summary>
        public ImageFullId? ImageId => ImageReference as ImageFullId;
    }
}
