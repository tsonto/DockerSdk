using System;

namespace DockerSdk
{
    /// <summary>
    /// Indicates that the given string is not in the expected format. This exception applies to references to Docker resources.
    /// </summary>
    [Serializable]
    public class MalformedReferenceException : DockerException
    {
        /// <summary>
        /// Creates an instance of the <see cref="MalformedReferenceException"/> type.
        /// </summary>
        public MalformedReferenceException()
        {
        }

        /// <summary>
        /// Creates an instance of the <see cref="MalformedReferenceException"/> type.
        /// </summary>
        /// <param name="message"></param>
        public MalformedReferenceException(string message) : base(message)
        {
        }

        /// <summary>
        /// Creates an instance of the <see cref="MalformedReferenceException"/> type.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public MalformedReferenceException(string message, Exception inner) : base(message, inner)
        {
        }

        /// <summary>
        /// Creates an instance of the <see cref="MalformedReferenceException"/> type.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context">s</param>
        protected MalformedReferenceException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
