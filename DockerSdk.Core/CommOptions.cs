using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace DockerSdk.Core
{
    public record CommOptions : IDisposable
    {
        private bool disposedValue;

        /// <summary>
        /// Gets or sets the Docker daemon URL to connect to. The default is localhost using a platform-appropriate
        /// transport.
        /// </summary>
        public Uri DaemonUrl 
        { 
            get => daemonUrl; 
            set => daemonUrl = value ?? throw new ArgumentNullException(nameof(value));
        }
        private Uri daemonUrl = null!;

        /// <summary>
        /// Gets or sets the credentials to use for connecting to the Docker daemon.
        /// </summary>
        public Credentials Credentials { get; set; } = new AnonymousCredentials();

        /// <summary>
        /// Gets or sets how long the SDK should wait for responses to messages it sends to the Docker daemon.
        /// </summary>
        /// <remarks>Some SDK methods override this value.</remarks>
        public TimeSpan DefaultTimeout
        {
            get => defaultTimeout;
            set
            {
                if (value <= TimeSpan.Zero && value != Timeout.InfiniteTimeSpan)
                    throw new ArgumentOutOfRangeException(nameof(value), "Timeout must be greater than zero");
                defaultTimeout = value;
            }
        }
        private TimeSpan defaultTimeout = TimeSpan.FromSeconds(100);

        public CommOptions(Uri daemonUrl)
        {
            DaemonUrl = daemonUrl;
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
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
