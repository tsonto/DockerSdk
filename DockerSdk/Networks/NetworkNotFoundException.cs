using System;
using System.Diagnostics.CodeAnalysis;
using Core = Docker.DotNet;

namespace DockerSdk.Networks
{
    /// <summary>
    /// Indicates that no Docker network with the given name is known to the Docker daemon.
    /// </summary>
    [Serializable]
    public class NetworkNotFoundException : DockerException
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

        internal static bool TryWrap(Core.DockerApiException ex, NetworkReference network, [NotNullWhen(returnValue: true)] out DockerException? wrapper)
        {
            if (ex is Core.DockerNetworkNotFoundException)
            {
                wrapper = Wrap(ex, network);
                return true;
            }

            wrapper = null;
            return false;
        }

        internal static NetworkNotFoundException Wrap(Core.DockerApiException ex, NetworkReference network)
            => new($"No network with the name or ID \"{network}\" exists.", ex);
    }
}
