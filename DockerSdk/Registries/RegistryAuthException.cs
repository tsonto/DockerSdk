using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using Docker.DotNet;

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

        internal static bool TryWrap(DockerApiException ex, string registry, [NotNullWhen(returnValue: true)] out DockerException? wrapped)
        {
            if (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                wrapped = new RegistryAuthException($"Authorization to registry {registry} failed: unauthorized.", ex);
                return true;
            }
            if (ex.StatusCode == HttpStatusCode.InternalServerError && ex.Message.Contains("401 Unauthorized"))
            {
                wrapped = new RegistryAuthException($"Authorization to registry {registry} failed: unauthorized.", ex);
                return true;
            }
            if (ex.StatusCode == HttpStatusCode.InternalServerError && ex.Message.Contains("no basic auth credentials"))
            {
                // This happens when we attempt to access an image on a private registry that expects basic auth, but we
                // gave it either no credentials or an identity token.
                wrapped = new RegistryAuthException($"Authorization to registry {registry} failed: expected basic auth credentials.", ex);
                return true;
            }
            if (ex.StatusCode == HttpStatusCode.NotFound && ex.Message.Contains("access to the resource is denied"))
            {
                // This happens when we attempt to access an image on a private registry without the credentials.
                wrapped = new RegistryAuthException($"Authorization to registry {registry} failed: denied.", ex);
                return true;
            }

            wrapped = null;
            return false;
        }
    }
}
