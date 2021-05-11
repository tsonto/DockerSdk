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

        public ImageName ImageName => (ImageName)ImageReference;
        public RegistryReference Registry => RegistryReference.Parse(ImageName);
    }
}
