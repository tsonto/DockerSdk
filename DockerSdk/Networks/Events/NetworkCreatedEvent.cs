using Docker.DotNet.Models;

namespace DockerSdk.Networks.Events
{
    /// <summary>
    /// Represents a notification of the creation of a new Docker network resource.
    /// </summary>
    public class NetworkCreatedEvent : NetworkEvent
    {
        internal NetworkCreatedEvent(Message message) : base(message, NetworkEventType.Created)
        {
            if (message.Actor.Attributes.TryGetValue("type", out var driver))
                Driver = driver;
        }

        /// <summary>
        /// Gets the name of the network drive that the network uses, such as "bridge" or "overlay".
        /// </summary>
        public string? Driver { get; }
    }
}
