using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using Core = Docker.DotNet;

namespace DockerSdk.Images
{
    /// <summary>
    /// Indicates that the indicated image does not exist at the Docker registry its name indicates.
    /// </summary>
    [Serializable]
    public class ImageNotFoundRemotelyException : ImageNotFoundException
    {
        /// <summary>
        /// Creates an instance of the <see cref="ImageNotFoundRemotelyException"/> class.
        /// </summary>
        public ImageNotFoundRemotelyException()
        {
        }

        /// <summary>
        /// Creates an instance of the <see cref="ImageNotFoundRemotelyException"/> class.
        /// </summary>
        public ImageNotFoundRemotelyException(string message) : base(message)
        {
        }

        /// <summary>
        /// Creates an instance of the <see cref="ImageNotFoundRemotelyException"/> class.
        /// </summary>
        public ImageNotFoundRemotelyException(string message, Exception inner) : base(message, inner)
        {
        }

        /// <summary>
        /// Creates an instance of the <see cref="ImageNotFoundRemotelyException"/> class. This overload is intended for
        /// deserialization only.
        /// </summary>
        protected ImageNotFoundRemotelyException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        internal static new bool TryWrap(Core.DockerApiException ex, ImageReference image, [NotNullWhen(returnValue: true)] out DockerException? wrapped)
        {
            if (ex.StatusCode == HttpStatusCode.NotFound && ex.Message.Contains("manifest unknown"))
            {
                // This happens when trying to fetch a image from a Docker registry that doesn't have it.
                wrapped = new ImageNotFoundRemotelyException($"Image {image} does not exist.", ex);
                return true;
            }
            else
            {
                wrapped = null;
                return false;
            }

            // I've seen pulls intermittently fail with these server errors. They do not necessarily mean that the image
            // doesn't exist. Based on other evidence, I think these were due to actual errors in the daemon process
            // rather than anything done by the client.
            // https://registry-1.docker.io/v2/emdot/dockersdk-private/manifests/inspect-me-1: EOF Get
            // https://registry-1.docker.io/v2/: EOF Head
            // https://registry-1.docker.io/v2/emdot/dockersdk/manifests/empty: Get
            // https://auth.docker.io/token?scope=repository%3Aemdot%2Fdockersdk%3Apull&service=registry.docker.io: EOF
        }

        /// <summary>
        /// Creates an <see cref="ImageNotFoundRemotelyException"/> exception from a <see
        /// cref="Core.DockerImageNotFoundException"/> exception.
        /// </summary>
        /// <param name="image">The image reference that was not found.</param>
        /// <param name="ex">The exception to wrap.</param>
        /// <returns>A new <see cref="ImageNotFoundException"/> exception.</returns>
        internal static ImageNotFoundRemotelyException Wrap(string image, Core.DockerImageNotFoundException ex)
            => new($"Image {image} does not exist at the expected Docker registry.", ex);
    }
}
