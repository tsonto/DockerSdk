using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using DockerSdk.Core;

namespace DockerSdk
{
    /// <summary>
    /// Specifies where to find a Docker daemon and how the SDK should connect to it.
    /// </summary>
    /// <seealso cref="DockerClient"/>
    public sealed record ClientOptions : CommOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClientOptions"/> class using the standard daemon URL.
        /// </summary>
        public ClientOptions() : this(GetDefaultUri()) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientOptions"/> class using the provided daemon URL.
        /// </summary>
        /// <param name="daemonUrl"></param>
        public ClientOptions(Uri daemonUrl) : base(daemonUrl) { }

        /// <summary>
        /// Gets or sets the set of certificates to use when communicating with the daemon.
        /// </summary>
        public X509Certificate2Collection Certificates { get; set; } = new X509Certificate2Collection();

        /// <summary>
        /// Gets or sets a value indicating whether to use <a
        /// href="https://en.wikipedia.org/wiki/Transport_Layer_Security">TLS</a> for communications with the daemon.
        /// Defaults to <see langword="true"/>.
        /// </summary>
        public bool UseTls { get; set; } = true;    // TODO: how does this relate to Credentials.IsTlsCredentials() ?

        /// <summary>
        /// Generates a <see cref="ClientOptions"/> object based on the local machine's <a
        /// href="https://en.wikipedia.org/wiki/Environment_variable">environment variables</a>.
        /// </summary>
        /// <returns>The generated <see cref="ClientOptions"/>.</returns>
        /// <remarks>
        /// If any of the relevant environment variables are not set, the <see cref="ClientOptions"/> object will use
        /// the default value for its corresponding property. Note that the default for <see
        /// cref="CommOptions.DaemonUrl"/>/ <c>DOCKER_HOST</c> is not valid for connecting to a Docker daemon. <br/>
        /// This method uses the following environment variables:
        /// <list type="bullet">
        /// <item>
        /// <c>DOCKER_HOST</c>: The URL for the Docker daemon to connect to. Corresponds to the <see
        /// cref="CommOptions.DaemonUrl"/> property.
        /// </item>
        /// <item>
        /// <c>DOCKER_CERT_PATH</c>: A filesystem path to read certificates from. Corresponds to the <see
        /// cref="Certificates"/> property.
        /// </item>
        /// <item>
        /// <c>DOCKER_TLS_VERIFY</c>: If set, the connection will use TLS. Corresponds to the <see cref="UseTls"/>
        /// property.
        /// </item>
        /// <item>
        /// <c>COMPOSER_HTTP_TIMEOUT</c>: The communications timeout to use, in seconds. Corresponds to the <see
        /// cref="CommOptions.DefaultTimeout"/> property.
        /// </item>
        /// </list>
        /// </remarks>
        public static ClientOptions FromEnvironment()
        {
            ClientOptions output;

            var daemonUriString = Environment.GetEnvironmentVariable("DOCKER_HOST");
            if (!string.IsNullOrEmpty(daemonUriString) && Uri.TryCreate(daemonUriString, UriKind.Absolute, out Uri? daemonUri))
                output = new ClientOptions(daemonUri);
            else
                output = new ClientOptions(GetDefaultUri());

            var certPath = Environment.GetEnvironmentVariable("DOCKER_CERT_PATH");
            if (!string.IsNullOrEmpty(certPath) && Directory.Exists(certPath))
            {
                output.Certificates = CertificateLoader.Load(certPath);
            }

            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DOCKER_TLS_VERIFY")))
            {
                output.UseTls = true;
            }

            var timeoutStringInSeconds = Environment.GetEnvironmentVariable("COMPOSER_HTTP_TIMEOUT");
            if (!string.IsNullOrEmpty(timeoutStringInSeconds) && double.TryParse(timeoutStringInSeconds, out double timeoutInSeconds))
            {
                output.DefaultTimeout = TimeSpan.FromSeconds(timeoutInSeconds);
            }

            return output;
        }

        private static Uri GetDefaultUri()
        {
            var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            return isWindows ? new Uri("npipe://./pipe/docker_engine") : new Uri("unix:/var/run/docker.sock");
        }
    }
}
