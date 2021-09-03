using DockerSdk.Events.Dto;

namespace DockerSdk.Containers.Events
{
    /// <summary>
    /// Indicates that the container's main process has exited. This is a transition to the <see cref="ContainerStatus.Exited"/>
    /// state (such as from the <see cref="ContainerStatus.Running"/> of <see cref="ContainerStatus.Paused"/>
    /// state). If the process shut down due to a "stop" or "restart" command (as opposed to a "kill" command),
    /// Docker will also emit <see cref="ContainerStopCompletedEvent"/>.
    /// </summary>
    public class ContainerExitedEvent : ContainerEvent
    {
        internal ContainerExitedEvent(Message message) : base(message, ContainerEventType.Exited)
        {
            if (message.Actor!.Attributes.TryGetValue("exitCode", out string? exitCodeString))
                if (int.TryParse(exitCodeString, out int exitCodeNumber))
                    ExitCode = exitCodeNumber;
        }

        /// <summary>
        /// Gets the exit code returned by the container's main process.
        /// </summary>
        public int? ExitCode { get; }
    }
}
