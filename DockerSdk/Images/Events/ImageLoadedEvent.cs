using Message = DockerSdk.Core.Models.Message;

namespace DockerSdk.Images.Events
{
    /// <summary>
    /// Represents a notification of an image being loaded from a file or stream.
    /// </summary>
    public class ImageLoadedEvent : ImageEvent
    {
        internal ImageLoadedEvent(Message message) : base(message, ImageEventType.Loaded)
        {
        }
    }
}
