using Message = Docker.DotNet.Models.Message;

namespace DockerSdk.Containers.Events
{
    /// <summary>
    /// Indicates that Docker has sent a signal to the container. Typically this is used to either ask the container's process to
    /// shut down or to force the container to end the process. It does not represent a state change, but may cause
    /// one.
    /// </summary>
    public class ContainerSignalledEvent : ContainerEvent
    {
        internal ContainerSignalledEvent(Message message) : base(message, ContainerEventType.Signalled)
        {
            if (message.Actor.Attributes.TryGetValue("signal", out string? signalString))
                if (int.TryParse(signalString, out int signalNumber))
                    SignalNumber = signalNumber;
        }

        /// <summary>
        /// Gets the numeric representation of the signal that was sent to the container.
        /// </summary>
        public int? SignalNumber { get; }
    }
}
