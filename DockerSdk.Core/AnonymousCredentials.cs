using System.Net.Http;

namespace DockerSdk.Core
{
    public class AnonymousCredentials : Credentials
    {
        public override HttpMessageHandler GetHandler(HttpMessageHandler innerHandler)
        {
            return innerHandler;
        }

        public override bool IsTlsCredentials()
        {
            return false;
        }
    }
}
