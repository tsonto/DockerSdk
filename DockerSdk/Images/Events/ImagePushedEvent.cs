using DockerSdk.Registries;
using Message = Docker.DotNet.Models.Message;

namespace DockerSdk.Images.Events
{
    /// <summary>
    /// Represents a notification of a local image being pushed to a registry.
    /// </summary>
    public class ImagePushedEvent : ImageEvent
    {
        internal ImagePushedEvent(Message message) : base(message, ImageEventType.Pulled)
        {
        }

        public ImageName ImageName => (ImageName)ImageReference;
        public RegistryReference Registry => RegistryReference.Parse(ImageName);
            
    }
}
