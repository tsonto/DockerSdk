using System;
using System.Diagnostics.CodeAnalysis;
using DockerSdk.Core;

namespace DockerSdk.Networks
{
    /// <summary>
    /// Indicates that no Docker network with the given name is known to the Docker daemon.
    /// </summary>
    [Serializable]
    public class NetworkNotFoundException : ResourceNotFoundException
    {
        /// <summary>
        /// Creates an instance of the <see cref="NetworkNotFoundException"/> class.
        /// </summary>
        public NetworkNotFoundException()
        {
        }

        /// <summary>
        /// Creates an instance of the <see cref="NetworkNotFoundException"/> class.
        /// </summary>
        /// <param name="message">The message to give the exception.</param>
        public NetworkNotFoundException(string message) : base(message)
        {
        }

        /// <summary>
        /// Creates an instance of the <see cref="NetworkNotFoundException"/> class.
        /// </summary>
        /// <param name="message">The message to give the exception.</param>
        /// <param name="inner">The exception that led to this exception.</param>
        public NetworkNotFoundException(string message, Exception inner) : base(message, inner)
        {
        }

        /// <summary>
        /// Creates an instance of the <see cref="NetworkNotFoundException"/> class. This constructor is used for
        /// deserialization.
        /// </summary>
        protected NetworkNotFoundException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
