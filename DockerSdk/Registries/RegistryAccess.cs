using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using DockerSdk.Images;
using DockerSdk.Core;
using CoreModels = DockerSdk.Core.Models;
using System.Net.Http;
using DockerSdk.Core.Models;

namespace DockerSdk.Registries
{
    /// <summary>
    /// Caches credentials for Docker registries and provides a means to check the credentials against the registry.
    /// </summary>
    public class RegistryAccess
    {
        internal RegistryAccess(DockerClient client)
        {
            _client = client;
            AddBuiltInRegistries();
        }

        /// <summary>
        /// Gets the Docker registries that have cache entries.
        /// </summary>
        public IEnumerable<RegistryReference> Registries => _entriesByServer.Keys.Select(r => new RegistryReference(r));

        private readonly DockerClient _client;

        private readonly Dictionary<string, RegistryEntry> _entriesByServer = new();

        /// <summary>
        /// Specifies that you want to use anonymous access to the indicated registry.
        /// </summary>
        /// <param name="registry">The name of the registry, as used in image names.</param>
        /// <exception cref="ArgumentException">The input is null or empty.</exception>
        /// <exception cref="MalformedReferenceException">
        /// The input is not a well-formed registry access reference.
        /// </exception>
        public void AddAnonymous(string registry)
            => AddAnonymous(RegistryReference.Parse(registry));

        /// <summary>
        /// Specifies that you want to use anonymous access to the indicated registry.
        /// </summary>
        /// <param name="registry">The name of the registry, as used in image names.</param>
        /// <exception cref="ArgumentException">The input is null or empty.</exception>
        public void AddAnonymous(RegistryReference registry)
        {
            if (registry is null)
                throw new ArgumentNullException(nameof(registry));

            // Get or add an entry for the registry.
            if (!_entriesByServer.TryGetValue(registry, out RegistryEntry? entry))
                entry = _entriesByServer[registry] = new RegistryEntry(registry);

            entry.AuthObject = new AuthConfig { ServerAddress = registry };
            entry.IsAnonymous = true;
        }

        /// <summary>
        /// Specifies that you want to use <a href="https://en.wikipedia.org/wiki/Basic_access_authentication">basic
        /// authentication</a> for access to the indicated registry.
        /// </summary>
        /// <param name="registry">The name of the registry, as used in image names.</param>
        /// <param name="username">The username to use for the registry.</param>
        /// <param name="password">The password to use for the registry.</param>
        /// <exception cref="ArgumentException">The input is null or empty.</exception>
        /// <exception cref="MalformedReferenceException">
        /// The input is not a well-formed registry access reference.
        /// </exception>
        public void AddBasicAuth(string registry, string username, string password)
            => AddBasicAuth(RegistryReference.Parse(registry), username, password);

        /// <summary>
        /// Specifies that you want to use <a href="https://en.wikipedia.org/wiki/Basic_access_authentication">basic
        /// authentication</a> for access to the indicated registry.
        /// </summary>
        /// <param name="registry">The name of the registry, as used in image names.</param>
        /// <param name="username">The username to use for the registry.</param>
        /// <param name="password">The password to use for the registry.</param>
        /// <exception cref="ArgumentException">One or more of the inputs are null or the empty string.</exception>
        public void AddBasicAuth(RegistryReference registry, string username, string password)
        {
            // Scrub the inputs.
            if (registry is null)
                throw new ArgumentNullException(nameof(registry));
            if (string.IsNullOrEmpty(username))
                throw new ArgumentException($"'{nameof(username)}' cannot be null or empty", nameof(username));
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException($"'{nameof(password)}' cannot be null or empty", nameof(password));

            // Get or add an entry for the registry.
            if (!_entriesByServer.TryGetValue(registry, out RegistryEntry? entry))
                entry = _entriesByServer[registry] = new RegistryEntry(registry);

            entry.AuthObject = new AuthConfig { ServerAddress = registry, Username = username, Password = password };
            entry.IsAnonymous = false;
        }

        /// <summary>
        /// Specifies that you want to use an identity token for authenticating with the indicated registry.
        /// </summary>
        /// <param name="registry">The name of the registry, as used in image names.</param>
        /// <param name="identityToken">An identity token granted by the registry.</param>
        /// <exception cref="ArgumentException">One or more of the inputs are null or the empty string.</exception>
        /// <exception cref="MalformedReferenceException">
        /// The input is not a well-formed registry access reference.
        /// </exception>
        public void AddIdentityToken(string registry, string identityToken)
            => AddIdentityToken(RegistryReference.Parse(registry), identityToken);

        /// <summary>
        /// Specifies that you want to use an identity token for authenticating with the indicated registry.
        /// </summary>
        /// <param name="registry">The name of the registry, as used in image names.</param>
        /// <param name="identityToken">An identity token granted by the registry.</param>
        /// <exception cref="ArgumentException">One or more of the inputs are null or the empty string.</exception>
        public void AddIdentityToken(RegistryReference registry, string identityToken)
        {
            // Scrub the inputs.
            if (registry is null)
                throw new ArgumentNullException(nameof(registry));
            if (string.IsNullOrEmpty(identityToken))
                throw new ArgumentException($"'{nameof(identityToken)}' cannot be null or empty", nameof(identityToken));

            // Get or add an entry for the registry.
            if (!_entriesByServer.TryGetValue(registry, out RegistryEntry? entry))
                entry = _entriesByServer[registry] = new RegistryEntry(registry);

            entry.AuthObject = new AuthConfig { ServerAddress = registry, IdentityToken = identityToken };
            entry.IsAnonymous = false;
        }

        /// <summary>
        /// Tests whether the client can authenticate with the indicated registry.
        /// </summary>
        /// <param name="registry"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        /// <remarks>
        /// Use the various <c>Add</c>* methods to supply authentication instructions. If no such instructions are
        /// provided, this method will try anonymous access.
        /// </remarks>
        /// <exception cref="ArgumentException">The input is null or empty.</exception>
        /// <exception cref="MalformedReferenceException">
        /// The input is not a well-formed registry access reference.
        /// </exception>
        /// <exception cref="System.Net.Http.HttpRequestException">
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate
        /// validation, or timeout.
        /// </exception>
        public Task<bool> CheckAuthenticationAsync(string registry, CancellationToken ct = default)
            => CheckAuthenticationAsync(RegistryReference.Parse(registry), ct);

        /// <summary>
        /// Tests whether the client can authenticate with the indicated registry.
        /// </summary>
        /// <param name="registry"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        /// <remarks>
        /// Use the various <c>Add</c>* methods to supply authentication instructions. If no such instructions are
        /// provided, this method will try anonymous access.
        /// </remarks>
        /// <exception cref="ArgumentException">The input is null or empty.</exception>
        /// <exception cref="System.Net.Http.HttpRequestException">
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate
        /// validation, or timeout.
        /// </exception>
        public async Task<bool> CheckAuthenticationAsync(RegistryReference registry, CancellationToken ct = default)
        {
            // Scrub the inputs.
            if (registry is null)
                throw new ArgumentNullException(nameof(registry));

            // If we have an entry for that registry, get the auth object. Otherwise create a new auth object, and set a
            // flag indicating that the registry isn't already known.
            bool isInCache = TryGetAuthObject(registry, out AuthConfig? authObject);
            if (!isInCache)
                authObject = new AuthConfig { ServerAddress = registry };

            var response = await _client.BuildRequest(HttpMethod.Post, "auth")
                .WithJsonBody(authObject!)
                .AcceptStatus(HttpStatusCode.NoContent) // 200 means that it generated an identity token; 204 means that it didn't
                .RejectStatus(HttpStatusCode.NotFound, "access to the resource is denied", _ => new RegistryAuthException($"Authorization to registry {registry} failed: denied.")) // This happens when we attempt to access an image on a private registry without the credentials.
                .RejectStatus(HttpStatusCode.Unauthorized, _ => new RegistryAuthException($"Authorization to registry {registry} failed: unauthorized."))
                .RejectStatus(HttpStatusCode.InternalServerError, "401 Unauthorized", _ => new RegistryAuthException($"Authorization to registry {registry} failed: unauthorized."))
                .RejectStatus(HttpStatusCode.InternalServerError, "no basic auth credentials", _ => new RegistryAuthException($"Authorization to registry {registry} failed: expected basic auth credentials.")) // This happens when we attempt to access an image on a private registry that expects basic auth, but we gave it either no credentials or an identity token.
                .SendAsync<AuthResponse>(ct)
                .ConfigureAwait(false);

            // If we're given an auth token, clear username/pass and start using that instead.
            if (!string.IsNullOrEmpty(response.IdentityToken))
                AddIdentityToken(registry, response.IdentityToken);
            // Otherwise, if it wasn't in cache but it accepted anonymous access, cache it now.
            else if (!isInCache)
                AddAnonymous(registry);

            return true;
        }

        /// <summary>
        /// Removes all custom registry entries.
        /// </summary>
        public void Clear()
        {
            _entriesByServer.Clear();
            AddBuiltInRegistries();
        }

        /// <summary>
        /// Parses an image's name to determine which registry the image is associated with.
        /// </summary>
        /// <param name="imageName">The name of image. (Not the ID.)</param>
        /// <returns>A reference to the registry.</returns>
        /// <exception cref="ArgumentException"><paramref name="imageName"/> is null or empty.</exception>
        /// <exception cref="MalformedReferenceException">The input could not be parsed as an image name.</exception>
        public RegistryReference GetRegistryName(string imageName) => GetRegistryName(ImageName.Parse(imageName));

        /// <summary>
        /// Removes an entry from the cache.
        /// </summary>
        /// <param name="registry">The host name of the registry to remove.</param>
        /// <returns>True if the entry was removed, or false if the entry was not present.</returns>
        /// <remarks>This method is equivalent to <c>docker logout</c>.</remarks>
        /// <exception cref="ArgumentException">The input is null or empty.</exception>
        public bool Remove(string registry) 
            => string.IsNullOrEmpty(registry)
            ? throw new ArgumentException("The input must not be null or empty.")
            : _entriesByServer.Remove(registry);

        /// <summary>
        /// Removes an entry from the cache.
        /// </summary>
        /// <param name="registry">The host name of the registry to remove.</param>
        /// <returns>True if the entry was removed, or false if the entry was not present.</returns>
        /// <remarks>This method is equivalent to <c>docker logout</c>.</remarks>
        /// <exception cref="ArgumentException">The input is null.</exception>
        public bool Remove(RegistryReference registry) => Remove(registry.ToString());

        /// <summary>
        /// Parses an image's name to determine which registry the image is associated with.
        /// </summary>
        /// <param name="imageName">The name of image.</param>
        /// <returns>The registry hostname, possibly including a port.</returns>
        internal static RegistryReference GetRegistryName(ImageName imageName)
        {
            if (string.IsNullOrEmpty(imageName))
                throw new ArgumentException("Must not be null or empty.", nameof(imageName));

            // Pull apart the image reference. This will always succeed, because ImageName objects are guaranteed to be well-formed.
            _ = ImageReferenceParser.TryParse(imageName, out DecomposedImageReference? parsed);
            
            // If the registry isn't given explicitly, it's implicitly Dockerhub.
            if (string.IsNullOrEmpty(parsed!.HostWithPort))
                return new("docker.io");

            // Create the reference. We use the Parse method so it's normalized properly.
            return RegistryReference.Parse(parsed.HostWithPort);
        }

        /// <summary>
        /// Gets the authorization information for the given registry. If the information is not already available, the
        /// method attempts anonymous access. If that fails, the method throws <see cref="RegistryAuthException"/>.
        /// </summary>
        /// <param name="registry">The Docker registry to get auth information for.</param>
        /// <param name="ct"></param>
        /// <returns>The auth object that Core methods need.</returns>
        /// <exception cref="ArgumentException">The <paramref name="registry"/> input was null or empty.</exception>
        /// <exception cref="MalformedReferenceException">
        /// The <paramref name="registry"/> is not in the expected format for a Docker registry name.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// One Task removed the auth object while another was getting it.
        /// </exception>
        /// <exception cref="RegistryAuthException">
        /// The registry requires credentials that the client hasn't been given.
        /// </exception>
        /// <exception cref="System.Net.Http.HttpRequestException">
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate
        /// validation, or timeout.
        /// </exception>
        internal async Task<AuthConfig> GetAuthObjectAsync(string registry, CancellationToken ct = default)
        {
            if (TryGetAuthObject(registry, out AuthConfig? auth))
                return auth;

            if (!await CheckAuthenticationAsync(registry, ct).ConfigureAwait(false))
                throw new RegistryAuthException($"Could not authenticate with registry {registry}. If this is a private registry, use one of the client.Registries.Add* methods to set credentials.");

            if (!TryGetAuthObject(registry, out auth))
                throw new InvalidOperationException($"Registry {registry} was removed from the client's auth information while another Task was trying to read it.");

            return auth;
        }

        /// <summary>
        /// Attempts to get the auth information for the given registry.
        /// </summary>
        /// <param name="registry">The name of the registry to retrieve information about.</param>
        /// <param name="authObject"></param>
        internal bool TryGetAuthObject(string registry, [NotNullWhen(returnValue: true)] out CoreModels.AuthConfig? authObject)
        {
            if (!_entriesByServer.TryGetValue(registry, out RegistryEntry? entry))
            {
                authObject = null;
                return false;
            }

            authObject = entry.AuthObject;
            return true;
        }

        private void AddBuiltInRegistries()
        {
            // These common registries allow anonymous access for public images.
            AddAnonymous("docker.io"); // Dockerhub
            AddAnonymous("ghcr.io"); // Github
            AddAnonymous("mcr.microsoft.com"); // Microsoft Azure
            AddAnonymous("public.ecr.aws"); // Amazon AWS
        }
    }
}
