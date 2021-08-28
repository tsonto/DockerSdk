using System;
using System.Net.Http;

namespace DockerSdk.Core
{
    public abstract class Credentials : IDisposable
    {
        public abstract bool IsTlsCredentials();

        public abstract HttpMessageHandler GetHandler(HttpMessageHandler innerHandler);

        protected virtual void Dispose(bool disposing) { }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
