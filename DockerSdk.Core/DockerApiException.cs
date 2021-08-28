
using System;
using System.Net;

namespace DockerSdk.Core
{
    public class DockerApiException : Exception
    {
        public HttpStatusCode StatusCode { get; private set; }

        public string? ResponseBody { get; private set; }

        public DockerApiException(HttpStatusCode statusCode, string? responseBody)
            : base($"Docker API responded with status code={statusCode}, response={responseBody}")
        {
            this.StatusCode = statusCode;
            this.ResponseBody = responseBody;
        }
    }
}
