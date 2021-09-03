using DockerSdk.Events.Dto;
using DockerSdk.Containers;

namespace DockerSdk.Networks.Events
{
    /// <summary>
    /// Indicates that the network has been detached from a container.
    /// </summary>
    public class NetworkDetachedEvent : NetworkEvent
    {
        internal NetworkDetachedEvent(Message message) : base(message, NetworkEventType.Detached)
        {
            string containerIdString = message.Actor!.Attributes!["container"];
            ContainerId = ContainerFullId.Parse(containerIdString);
        }

        /// <summary>
        /// Gets the ID of the container that the network detached from.
        /// </summary>
        public ContainerFullId ContainerId { get; }
    }
}
