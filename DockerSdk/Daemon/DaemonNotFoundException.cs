using System;

namespace DockerSdk.Daemon
{
    /// <summary>
    /// Indicates that the Docker daemon is not present or not running.
    /// </summary>
    [Serializable]
    public class DaemonNotFoundException : DockerException
    {
        /// <summary>
        /// </summary>
        public DaemonNotFoundException() { }

        /// <summary>
        /// </summary>
        /// <param name="message"></param>
        public DaemonNotFoundException(string message) : base(message) { }

        /// <summary>
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public DaemonNotFoundException(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected DaemonNotFoundException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
