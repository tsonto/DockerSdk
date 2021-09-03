using System;
using System.Net.Http;

namespace DockerSdk.Core
{
    public abstract class Credentials : IDisposable
    {
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public abstract HttpMessageHandler GetHandler(HttpMessageHandler innerHandler);

        public abstract bool IsTlsCredentials();

        protected virtual void Dispose(bool disposing)
        {
        }
    }
}
