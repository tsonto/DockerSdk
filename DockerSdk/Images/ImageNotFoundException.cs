using System;
using System.Diagnostics.CodeAnalysis;
using Core = Docker.DotNet;

namespace DockerSdk.Images
{
    /// <summary>
    /// Indicates that the Docker daemon could not find the indicated image where it was looking.
    /// </summary>
    [Serializable]
    public abstract class ImageNotFoundException : ResourceNotFoundException
    {
        /// <summary>
        /// Creates an instance of the <see cref="ImageNotFoundException"/> class.
        /// </summary>
        public ImageNotFoundException()
        {
        }

        /// <summary>
        /// Creates an instance of the <see cref="ImageNotFoundException"/> class.
        /// </summary>
        /// <param name="message"></param>
        public ImageNotFoundException(string message) : base(message)
        {
        }

        /// <summary>
        /// Creates an instance of the <see cref="ImageNotFoundException"/> class.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public ImageNotFoundException(string message, Exception inner) : base(message, inner)
        {
        }

        /// <summary>
        /// Creates an instance of the <see cref="ImageNotFoundException"/> class.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected ImageNotFoundException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        internal static bool TryWrap(Core.DockerApiException ex, ImageReference image, [NotNullWhen(returnValue: true)] out DockerException? wrapped)
        {
            if (ImageNotFoundRemotelyException.TryWrap(ex, image, out wrapped))
                return true;
            if (ImageNotFoundLocallyException.TryWrap(ex, image, out wrapped))
                return true;

            wrapped = null;
            return false;
        }
    }
}
