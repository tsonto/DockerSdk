using DockerSdk.Events;
using DockerSdk.Networks;
using DockerSdk.Events.Dto;

namespace DockerSdk.Networks.Events
{
    /// <summary>
    /// Represents an event emitted by the Docker daemon about a network.
    /// </summary>
    public abstract class NetworkEvent : Event
    {
        internal NetworkEvent(Message message, NetworkEventType eventType)
            : base(message, EventSubjectType.Network)
        {
            EventType = eventType;
            NetworkId = new NetworkFullId(message.Actor!.Id);

            if (message.Actor.Attributes.TryGetValue("name", out var nameString))
                if (NetworkName.TryParse(nameString, out var name))
                    NetworkName = name;
        }

        /// <summary>
        /// Gets the network's ID.
        /// </summary>
        public NetworkFullId NetworkId { get; }

        /// <summary>
        /// Gets the network's name, if it was included in the event details.
        /// </summary>
        public NetworkName? NetworkName { get; }

        /// <summary>
        /// Gets the type of event that this is.
        /// </summary>
        public NetworkEventType EventType { get; }

        internal static new Event? Wrap(Message message)
            => message.Action switch
            {
                "create" => new NetworkCreatedEvent(message),
                "connect" => new NetworkAttachedEvent(message),
                "destroy" => new NetworkDeletedEvent(message),
                "disconnect" => new NetworkDetachedEvent(message),
                // remove
                _ => null
            };
    }
}
