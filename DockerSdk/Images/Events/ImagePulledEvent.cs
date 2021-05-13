using DockerSdk.Registries;
using Message = Docker.DotNet.Models.Message;

namespace DockerSdk.Images.Events
{
    /// <summary>
    /// Represents a notification of the creation of a new image.
    /// </summary>
    public class ImagePulledEvent : ImageEvent
    {
        internal ImagePulledEvent(Message message) : base(message, ImageEventType.Pulled)
        {
        }

        /// <summary>
        /// Gets the image's full ID.
        /// </summary>
        public ImageName ImageName => (ImageName)ImageReference;

        /// <summary>
        /// Gets the Docker registry that the image was pulled from.
        /// </summary>
        public RegistryReference Registry => RegistryAccess.GetRegistryName(ImageName);
    }
}
