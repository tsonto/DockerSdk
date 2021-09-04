using System;
using System.Net;
using DockerSdk.Core;

namespace DockerSdk
{
    /// <summary>
    /// Base class for exceptions that are specific to the Docker client's functionality.
    /// </summary>
    [Serializable]
    public class DockerException : Exception
    {
        /// <summary>
        /// Creates of instance of the DockerException type.
        /// </summary>
        public DockerException()
        {
        }

        /// <summary>
        /// Creates of instance of the DockerException type.
        /// </summary>
        public DockerException(string message) : base(message)
        {
        }

        /// <summary>
        /// Creates of instance of the DockerException type.
        /// </summary>
        public DockerException(string message, Exception inner)
            : base(message + " See inner exception for details.", inner)
        {
        }

        /// <summary>
        /// Creates of instance of the DockerException type.
        /// </summary>
        protected DockerException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
