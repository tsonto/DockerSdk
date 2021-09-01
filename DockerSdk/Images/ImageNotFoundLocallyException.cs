using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using DockerSdk.Core;

namespace DockerSdk.Images
{
    /// <summary>
    /// Indicates that the Docker daemon does not have a local copy of the indicated image.
    /// </summary>
    [Serializable]
    public class ImageNotFoundLocallyException : ImageNotFoundException
    {
        /// <summary>
        /// Creates an instance of the <see cref="ImageNotFoundLocallyException"/> class.
        /// </summary>
        public ImageNotFoundLocallyException()
        {
        }

        /// <summary>
        /// Creates an instance of the <see cref="ImageNotFoundLocallyException"/> class.
        /// </summary>
        public ImageNotFoundLocallyException(string message) : base(message)
        {
        }

        /// <summary>
        /// Creates an instance of the <see cref="ImageNotFoundLocallyException"/> class.
        /// </summary>
        public ImageNotFoundLocallyException(string message, Exception inner) : base(message, inner)
        {
        }

        /// <summary>
        /// Creates an instance of the <see cref="ImageNotFoundLocallyException"/> class. This overload is intended for
        /// deserialization only.
        /// </summary>
        protected ImageNotFoundLocallyException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        //internal static new bool TryWrap(Core.DockerApiException ex, ImageReference image, [NotNullWhen(returnValue: true)] out DockerException? wrapped)
        //{
        //    if (ex is Core.DockerImageNotFoundException imageNotFoundEx)
        //    {
        //        // This happens when trying to fetch an image from a Docker daemon that doesn't have it.
        //        wrapped = Wrap(image, imageNotFoundEx);
        //        return true;
        //    }
        //    else if (ex.StatusCode == HttpStatusCode.NotFound && ex.Message.Contains("No such image:", StringComparison.OrdinalIgnoreCase))
        //    {
        //        // This happens when creating a container with auto-pull disabled if the image doesn't exist. (Note: Due
        //        // to a bug in the Docker SDK documentation, Docker.DotNet throws DockerContainerNotFoundException for
        //        // this.) (Also note: Sometimes the N in "no such image" is uppercase, and sometimes it's lowercase.
        //        // Thus the case-insensitive check.)
        //        wrapped = Wrap(image, ex);
        //        return true;
        //    }
        //    else
        //    {
        //        wrapped = null;
        //        return false;
        //    }
        //}

        ///// <summary>
        ///// Creates an <see cref="ImageNotFoundLocallyException"/> exception from a <see
        ///// cref="Core.DockerImageNotFoundException"/> exception.
        ///// </summary>
        ///// <param name="image">The image reference that was not found.</param>
        ///// <param name="ex">The exception to wrap.</param>
        ///// <returns>A new <see cref="ImageNotFoundException"/> exception.</returns>
        //internal static ImageNotFoundLocallyException Wrap(string image, Core.DockerApiException ex)
        //    => new($"Image {image} does not exist locally.", ex);
    }
}
