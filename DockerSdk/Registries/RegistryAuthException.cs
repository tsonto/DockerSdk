using System;

namespace DockerSdk.Registries
{
    /// <summary>
    /// Represents a failure to authenticate with a Docker registry.
    /// </summary>
    [Serializable]
    public class RegistryAuthException : DockerException
    {
        /// <summary>
        /// Creates a new instance of the <see cref="RegistryAuthException"/> class.
        /// </summary>
        public RegistryAuthException()
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="RegistryAuthException"/> class.
        /// </summary>
        public RegistryAuthException(string message) : base(message)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="RegistryAuthException"/> class.
        /// </summary>
        public RegistryAuthException(string message, Exception inner) : base(message, inner)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="RegistryAuthException"/> class. This overload is used for
        /// deserialization.
        /// </summary>
        protected RegistryAuthException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
