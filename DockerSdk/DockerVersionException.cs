using System;

namespace DockerSdk
{
    /// <summary>
    /// Indicates that API version negotiation failed because there is no overlap between the versions that the SDK
    /// supports with the versions the Docker daemon supports.
    /// </summary>
    [Serializable]
    public class DockerVersionException : DockerException
    {
        /// <summary>
        /// Creates an instance of the DockerVersionException type.
        /// </summary>
        public DockerVersionException()
        {
        }

        /// <summary>
        /// Creates an instance of the DockerVersionException type.
        /// </summary>
        /// <param name="message"></param>
        public DockerVersionException(string message) : base(message)
        {
        }

        /// <summary>
        /// Creates an instance of the DockerVersionException type.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public DockerVersionException(string message, Exception inner) : base(message, inner)
        {
        }

        /// <summary>
        /// Creates an instance of the DockerVersionException type.
        /// </summary>
        protected DockerVersionException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
