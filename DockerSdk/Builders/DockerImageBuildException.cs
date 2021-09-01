using System;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;

namespace DockerSdk.Builders
{
    /// <summary>
    /// Indicates that an image build operation failed.
    /// </summary>
    public class DockerImageBuildException : DockerException
    {
        /// <summary>
        /// Initializes an instances of the <see cref="DockerImageBuildException"/> class.
        /// </summary>
        public DockerImageBuildException()
        {
        }

        /// <summary>
        /// Initializes an instances of the <see cref="DockerImageBuildException"/> class.
        /// </summary>
        public DockerImageBuildException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes an instances of the <see cref="DockerImageBuildException"/> class.
        /// </summary>
        public DockerImageBuildException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes an instances of the <see cref="DockerImageBuildException"/> class.
        /// </summary>
        protected DockerImageBuildException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
