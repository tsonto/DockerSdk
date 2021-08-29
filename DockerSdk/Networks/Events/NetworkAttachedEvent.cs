using DockerSdk.Core.Models;
using DockerSdk.Containers;

namespace DockerSdk.Networks.Events
{
    /// <summary>
    /// Indicates that the network has been attached to a container.
    /// </summary>
    public class NetworkAttachedEvent : NetworkEvent
    {
        internal NetworkAttachedEvent(Message message) : base(message, NetworkEventType.Attached)
        {
            string containerIdString = message.Actor.Attributes["container"];
            ContainerId = ContainerFullId.Parse(containerIdString);
        }

        /// <summary>
        /// Gets the ID of the container that the network attached to.
        /// </summary>
        public ContainerFullId ContainerId { get; }
    }
}
