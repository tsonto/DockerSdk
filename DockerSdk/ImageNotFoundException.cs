using System;
using Core = Docker.DotNet;

namespace DockerSdk
{
    [Serializable]
    public class ImageNotFoundException : ResourceNotFoundException
    {
        public ImageNotFoundException()
        {
        }

        public ImageNotFoundException(string message) : base(message)
        {
        }

        public ImageNotFoundException(string message, Exception inner) : base(message, inner)
        {
        }

        protected ImageNotFoundException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        internal static ImageNotFoundException Wrap(string image, Core.DockerImageNotFoundException ex)
            => new($"Image {image} does not exist.", ex);
    }
}
