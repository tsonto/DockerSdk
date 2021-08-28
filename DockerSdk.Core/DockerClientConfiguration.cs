using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace DockerSdk.Core
{
    public class DockerClientConfiguration : IDisposable
    {
        private static readonly TimeSpan DefaultDefaultTimeout = TimeSpan.FromSeconds(100);
        private bool disposedValue;

        public Uri EndpointBaseUri { get; private set; }

        public Credentials Credentials { get; private set; }

        public TimeSpan DefaultTimeout { get; private set; }

        public TimeSpan NamedPipeConnectTimeout { get; set; } = DefaultDefaultTimeout;

        public static Uri LocalDockerUri()
        {
            var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            return isWindows ? new Uri("npipe://./pipe/docker_engine") : new Uri("unix:/var/run/docker.sock");
        }

        public DockerClientConfiguration(Credentials? credentials = null, TimeSpan? defaultTimeout = null)
            : this(LocalDockerUri(), credentials, defaultTimeout)
        {
        }

        public DockerClientConfiguration(Uri endpoint, Credentials? credentials = null, TimeSpan? defaultTimeout = null)
        {
            Credentials = credentials ?? new AnonymousCredentials();
            EndpointBaseUri = endpoint ?? throw new ArgumentNullException(nameof(endpoint));

            if (defaultTimeout == null)
                DefaultTimeout = DefaultDefaultTimeout;
            else if (defaultTimeout > TimeSpan.Zero || defaultTimeout == Timeout.InfiniteTimeSpan)
                DefaultTimeout = defaultTimeout.Value;
            else
                throw new ArgumentException("Timeout must be greater than zero", nameof(defaultTimeout));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Credentials.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
