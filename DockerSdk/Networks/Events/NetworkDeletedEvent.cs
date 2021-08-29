using DockerSdk.Core.Models;

namespace DockerSdk.Networks.Events
{
    /// <summary>
    /// Represents a notification that the network has been deleted.
    /// </summary>
    public class NetworkDeletedEvent : NetworkEvent
    {
        internal NetworkDeletedEvent(Message message) : base(message, NetworkEventType.Deleted)
        {
        }
    }
}
