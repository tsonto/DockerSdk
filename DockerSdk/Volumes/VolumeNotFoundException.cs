using System;
using System.Diagnostics.CodeAnalysis;
using Docker.DotNet;
using Core = Docker.DotNet;

namespace DockerSdk.Volumes
{
    /// <summary>
    /// Indicates that the Docker daemon could not find the indicated volume where it was looking.
    /// </summary>
    [Serializable]
    public abstract class VolumeNotFoundException : ResourceNotFoundException
    {
        /// <summary>
        /// Creates an instance of the <see cref="VolumeNotFoundException"/> class.
        /// </summary>
        public VolumeNotFoundException()
        {
        }

        /// <summary>
        /// Creates an instance of the <see cref="VolumeNotFoundException"/> class.
        /// </summary>
        /// <param name="message"></param>
        public VolumeNotFoundException(string message) : base(message)
        {
        }

        /// <summary>
        /// Creates an instance of the <see cref="VolumeNotFoundException"/> class.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public VolumeNotFoundException(string message, Exception inner) : base(message, inner)
        {
        }

        internal static bool TryWrap(DockerApiException ex, VolumeName name, out VolumeNotFoundException wrapped)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates an instance of the <see cref="VolumeNotFoundException"/> class.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected VolumeNotFoundException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
