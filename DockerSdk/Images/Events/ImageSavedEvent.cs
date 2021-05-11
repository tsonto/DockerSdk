using Message = Docker.DotNet.Models.Message;

namespace DockerSdk.Images.Events
{
    /// <summary>
    /// Represents a notification of a local image being saved to a file or stream.
    /// </summary>
    public class ImageSavedEvent : ImageEvent
    {
        internal ImageSavedEvent(Message message) : base(message, ImageEventType.Saved)
        {
        }
    }
}
