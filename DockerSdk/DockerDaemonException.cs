using System;
using System.Runtime.Serialization;

namespace DockerSdk
{
    [Serializable]
    internal class DockerDaemonException : Exception
    {
        public DockerDaemonException()
        {
        }

        public DockerDaemonException(string? message) : base(message)
        {
        }

        public DockerDaemonException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected DockerDaemonException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}