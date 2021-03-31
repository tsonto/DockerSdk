using System;
using System.Diagnostics.CodeAnalysis;
using DockerSdk.Images;
using Core = Docker.DotNet;

namespace DockerSdk
{
    /// <summary>
    /// Indicates that the Docker daemon does not have a local copy of the indicated image.
    /// </summary>
    [Serializable]
    public class ImageNotFoundException : ResourceNotFoundException
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

        /// <summary>
        /// Creates an <see cref="ImageNotFoundException"/> exception from a <see cref="Core.DockerImageNotFoundException"/> exception.
        /// </summary>
        /// <param name="image">The image reference that was not found.</param>
        /// <param name="ex">The exception to wrap.</param>
        /// <returns>A new <see cref="ImageNotFoundException"/> exception.</returns>
        internal static ImageNotFoundException Wrap(string image, Core.DockerImageNotFoundException ex)
            => new($"Image {image} does not exist.", ex);

        internal static bool TryWrap(Core.DockerApiException ex, ImageReference image, [NotNullWhen(returnValue: true)] out DockerException? wrapped)
        {
            if (ex is Core.DockerImageNotFoundException imageNotFoundEx)
            {
                // This happens when trying to fetch an image from a Docker daemon that doesn't have it.
                wrapped = Wrap(imageNotFoundEx);
                return true;
            }
            else if (ex.StatusCode == System.Net.HttpStatusCode.NotFound && ex.Message.Contains("manifest unknown"))
            {
                // This happens when trying to fetch a image from a Docker registry that doesn't have it.
                wrapped = new ImageNotFoundException($"Image {image} does not exist.", ex);
                return true;
            }
            else
            {
                wrapped = null;
                return false;
            }
        }
    }
}
