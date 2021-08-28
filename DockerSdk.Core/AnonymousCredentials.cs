using System;
using System.Net.Http;

namespace DockerSdk.Core
{
    public class AnonymousCredentials : Credentials
    {
        public override bool IsTlsCredentials()
        {
            return false;
        }

        public override HttpMessageHandler GetHandler(HttpMessageHandler innerHandler)
        {
            return innerHandler;
        }
    }
}
