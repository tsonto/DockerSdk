using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DockerSdk.Containers;
using DockerSdk.Core;
using DockerSdk.Events;
using DockerSdk.Images;
using DockerSdk.Networks;
using DockerSdk.Registries;
using Core = Docker.DotNet;
using CoreModels = Docker.DotNet.Models;

namespace DockerSdk
{
    /// <summary>
    /// Provides remote access to a Docker daemon.
    /// </summary>
    public class DockerClient : IDisposable, IObservable<Event>
    {
        private DockerClient(Comm core, ClientOptions options, Version negotiatedApiVersion)
        {
            // Start listening to events.
            EventListener = new EventListener(this);

            Core = core;
            Options = options;
            ApiVersion = negotiatedApiVersion;
            Containers = new ContainerAccess(this);
            Images = new ImageAccess(this);
            Networks = new NetworkAccess(this);
            Registries = new RegistryAccess(this);
        }

        /// <summary>
        /// Gets the version of the Docker API that will be used to communicate with the Docker daemon.
        /// </summary>
        /// <remarks>This will always be the highest version that both sides support.</remarks>
        public Version ApiVersion { get; }

        /// <summary>
        /// Provides access to functionality related to Docker containers.
        /// </summary>
        public ContainerAccess Containers { get; }

        /// <summary>
        /// Provides access to functionality related to Docker images.
        /// </summary>
        public ImageAccess Images { get; }

        /// <summary>
        /// Provides access to functionality related to Docker networks.
        /// </summary>
        public NetworkAccess Networks { get; }

        /// <summary>
        /// Provides access to functionality related to Docker registries.
        /// </summary>
        public RegistryAccess Registries { get; }

        /// <summary>
        /// Gets the core client, which is what does all the heavy lifting for communicating with the Docker daemon.
        /// </summary>
        internal Comm Core { get; }

        internal ClientOptions Options { get; }
        internal readonly EventListener EventListener;

        /// <summary>
        /// The minimum Docker API version that the SDK supports.
        /// </summary>
        private static readonly Version _libraryMaxApiVersion = new("1.41");

        /// <summary>
        /// The maximum Docker API version that the SDK supports.
        /// </summary>
        private static readonly Version _libraryMinApiVersion = new("1.41");

        private bool _isDisposed;

        /// <summary>
        /// Creates a new Docker client and connects it to the local Docker daemon.
        /// </summary>
        /// <param name="ct">A token used to cancel the operation.</param>
        /// <returns>A <see cref="Task"/> that completes when the connection has been established.</returns>
        /// <exception cref="DockerVersionException">
        /// The API versions that the SDK supports don't overlap with the API versions that the daemon supports.
        /// </exception>
        /// <exception cref="Core.DockerApiException">An internal error occurred within the daemon.</exception>
        /// <exception cref="System.Net.Http.HttpRequestException">
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate
        /// validation, or timeout.
        /// </exception>
        public static Task<DockerClient> StartAsync(CancellationToken ct = default)
            => StartAsync(new ClientOptions(), ct);

        /// <summary>
        /// Creates a new Docker client and connects it to a Docker daemon.
        /// </summary>
        /// <param name="daemonUrl">The URL of the Docker daemon to connect to.</param>
        /// <param name="ct">A token used to cancel the operation.</param>
        /// <returns>A <see cref="Task"/> that completes when the connection has been established.</returns>
        /// <exception cref="DockerVersionException">
        /// The API versions that the SDK supports don't overlap with the API versions that the daemon supports.
        /// </exception>
        /// <exception cref="Core.DockerApiException">An internal error occurred within the daemon.</exception>
        /// <exception cref="System.Net.Http.HttpRequestException">
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate
        /// validation, or timeout.
        /// </exception>
        /// <exception cref="ArgumentException">The URL is <see langword="null"/>.</exception>
        public static Task<DockerClient> StartAsync(Uri daemonUrl, CancellationToken ct = default)
            => StartAsync(new ClientOptions { DaemonUri = daemonUrl }, ct);

        /// <summary>
        /// Creates a new Docker client and connects it to a Docker daemon.
        /// </summary>
        /// <param name="options">Details on how to connect and how the client should behave.</param>
        /// <param name="ct">A token used to cancel the operation.</param>
        /// <returns>A <see cref="Task"/> that completes when the connection has been established.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
        /// <exception cref="DockerVersionException">
        /// The API versions that the SDK supports don't overlap with the API versions that the daemon supports.
        /// </exception>
        /// <exception cref="Core.DockerApiException">An internal error occurred within the daemon.</exception>
        /// <exception cref="System.Net.Http.HttpRequestException">
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate
        /// validation, or timeout.
        /// </exception>
        /// <exception cref="DaemonNotFoundException">There is no running Docker daemon at the specified URL.</exception>
        public static async Task<DockerClient> StartAsync(ClientOptions options, CancellationToken ct = default)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));
            if (options.DaemonUri is null)
                throw new ArgumentException("The daemon URL is required.", nameof(options));

            // First, establish a connection with the daemon.
            Comm comm = options.ToCore().CreateClient();

            // Now figure out which API version to use. This will be the max API version that both sides support.
            CoreModels.VersionResponse versionInfo;
            try
            {
                var response = await comm.SendAsync(HttpMethod.Get, "version", null, token: ct).ConfigureAwait(false);
                versionInfo = response.
            }
            catch (TimeoutException ex)
            {
                throw new DaemonNotFoundException($"No Docker daemon responded at {options.DaemonUri}. This typically means that the daemon is not running.", ex);
            }
            var negotiatedApiVersion = DetermineVersionToUse(_libraryMinApiVersion, versionInfo, _libraryMaxApiVersion);

            // Replace the non-versioned core instance with a versioned instance.
            comm.Dispose();
            comm = options.ToCore().CreateClient(negotiatedApiVersion);

            // Now remove the reference to the credentials so they can drop out of memory as soon as possible.
            options.Credentials = null;

            return new DockerClient(comm, options, negotiatedApiVersion);
        }

        /// <summary>
        /// Subscribes to events from the Docker daemon.
        /// </summary>
        /// <param name="observer">An object to observe the events.</param>
        /// <returns>
        /// An <see cref="IDisposable"/> representing the subscription. Disposing this unsubscribes and releases
        /// resources.
        /// </returns>
        public IDisposable Subscribe(IObserver<Event> observer)
            => EventListener.Subscribe(observer);

        /// <summary>
        /// Determines which API version to use for communications between the SDK and the Docker daemon, or throws an
        /// exception if there's no acceptable answer.
        /// </summary>
        /// <param name="libraryMin">The lowest API version that the SDK supports.</param>
        /// <param name="daemonInfo">Information about what versions the daemon supports.</param>
        /// <param name="libraryMax">The highest API version that the SDK supports.</param>
        /// <returns>The API version to use.</returns>
        /// <exception cref="DockerVersionException">
        /// There's no overlap between what the two sides can accept.
        /// </exception>
        internal static Version DetermineVersionToUse(Version libraryMin, CoreModels.VersionResponse daemonInfo, Version libraryMax)
        {
            // Watch out for daemons that are so old that they don't even report APIVersion.
            if (string.IsNullOrEmpty(daemonInfo.APIVersion))
                throw new DockerVersionException($"The daemon did not report its supported API version, which likely means that it is extremely old. The SDK only supports API versions down to v{libraryMin.ToString(2)}.");

            // Check that we support the daemon's max API version.
            var daemonMaxVersion = new Version(daemonInfo.APIVersion);
            if (daemonMaxVersion < libraryMin)
                throw new DockerVersionException($"Version mismatch: The Docker daemon only supports API versions up to v{daemonMaxVersion.ToString(2)}, and the Docker SDK library only supports API versions down to v{libraryMin.ToString(2)}.");

            // Check the daemon's min API version.
            var daemonMinVersion = new Version(daemonInfo.MinAPIVersion);
            if (daemonMinVersion > libraryMax)
                throw new DockerVersionException($"Version mismatch: The Docker daemon supports API versions v{daemonMinVersion.ToString(2)} through v{daemonMaxVersion.ToString(2)}, and the Docker SDK library supports API versions v{libraryMin.ToString(2)} through v{libraryMax.ToString(2)}.");

            // Use the highest version that both sides support.
            return daemonMaxVersion < libraryMax ? daemonMaxVersion : libraryMax;
        }

        /// <summary>
        /// Throws an exception if the negotiated API version is not in the expected range.
        /// </summary>
        /// <param name="minVersion">The minimum allowed API version, in MAJOR.MINOR format.</param>
        /// <param name="maxVersion">
        /// The maximum allowed API version, in MAJOR.MINOR format, or <see langword="null"/> for no upper limit.
        /// </param>
        /// <exception cref="NotSupportedException">The negotiated API version is not in the expected range.</exception>
        /// <exception cref="ObjectDisposedException">This object has been disposed.</exception>
        /// <seealso cref="ApiVersion"/>
        internal void RequireApiVersion(string? minVersion = null, string? maxVersion = null)
        {
            if (_isDisposed)
                throw new ObjectDisposedException("This object has been disposed.");

            var version = ApiVersion;
            if (minVersion is not null && version < new Version(minVersion))
                throw new NotSupportedException($"This feature is not available until API version v{minVersion}. You are currently using API version v{version.ToString(2)}.");
            if (maxVersion is not null && version > new Version(maxVersion))
                throw new NotSupportedException($"This feature has not been available since API version v{maxVersion}. You are currently using API version v{version.ToString(2)}.");
        }

        #region IDisposable

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Overridable Dispose method
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    EventListener.Dispose();
                    Core?.Dispose();
                }

                _isDisposed = true;
            }
        }

        #endregion IDisposable
    }
}
