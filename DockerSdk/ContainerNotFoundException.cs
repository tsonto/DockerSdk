using System;
using System.Diagnostics.CodeAnalysis;
using DockerSdk.Containers;
using Core = Docker.DotNet;

namespace DockerSdk
{
    /// <summary>
    /// Indicates that no Docker container with the given name is known to the Docker daemon.
    /// </summary>
    [Serializable]
    public class ContainerNotFoundException : DockerException
    {
        /// <summary>
        /// Creates an instance of the <see cref="ContainerNotFoundException"/> class.
        /// </summary>
        public ContainerNotFoundException()
        {
        }

        /// <summary>
        /// Creates an instance of the <see cref="ContainerNotFoundException"/> class.
        /// </summary>
        /// <param name="message">The message to give the exception.</param>
        public ContainerNotFoundException(string message) : base(message)
        {
        }

        /// <summary>
        /// Creates an instance of the <see cref="ContainerNotFoundException"/> class.
        /// </summary>
        /// <param name="message">The message to give the exception.</param>
        /// <param name="inner">The exception that led to this exception.</param>
        public ContainerNotFoundException(string message, Exception inner) : base(message, inner)
        {
        }

        /// <summary>
        /// Creates an instance of the <see cref="ContainerNotFoundException"/> class. This constructor is used for deserialization.
        /// </summary>
        protected ContainerNotFoundException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        internal static bool TryWrap(Core.DockerApiException ex, ContainerReference container, [NotNullWhen(returnValue:true)] out DockerException? wrapper)
        {
            if (ex is Core.DockerContainerNotFoundException)
            {
                wrapper = Wrap(ex, container);
                return true;
            }

            wrapper = null;
            return false;
        }

        internal static ContainerNotFoundException Wrap(Core.DockerApiException ex, ContainerReference container)
            => new($"No container named \"{container}\" exists.", ex);
    }
}
