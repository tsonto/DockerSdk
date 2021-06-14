using System;
using System.Runtime.Serialization;

namespace DockerSdk.Builders
{
    internal class DockerIgnoreException : Exception
    {
        public DockerIgnoreException()
        {
        }

        public DockerIgnoreException(string? message) : base(message)
        {
        }

        public DockerIgnoreException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected DockerIgnoreException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}