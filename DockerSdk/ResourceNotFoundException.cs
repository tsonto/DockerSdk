using System;

namespace DockerSdk
{
    /// <summary>
    /// Indicates that the Docker daemon could not find the indicated Docker resource.
    /// </summary>
    [Serializable]
    public class ResourceNotFoundException : DockerException
    {
        /// <summary>
        /// Creates an instance of the <see cref="ResourceNotFoundException"/> type.
        /// </summary>
        public ResourceNotFoundException()
        {
        }

        /// <summary>
        /// Creates an instance of the <see cref="ResourceNotFoundException"/> type.
        /// </summary>
        /// <param name="message"></param>
        public ResourceNotFoundException(string message) : base(message)
        {
        }

        /// <summary>
        /// Creates an instance of the <see cref="ResourceNotFoundException"/> type.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public ResourceNotFoundException(string message, Exception inner) : base(message, inner)
        {
        }

        /// <summary>
        /// Creates an instance of the <see cref="ResourceNotFoundException"/> type.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected ResourceNotFoundException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
