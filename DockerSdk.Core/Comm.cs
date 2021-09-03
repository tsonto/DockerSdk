using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Net.Http.Client;

namespace DockerSdk.Core
{
    public sealed class Comm : IDisposable
    {
        public Comm(DockerClientConfiguration configuration, Version? apiVersion)
        {
            HttpMessageHandler handler = ManagedHandler.Create(configuration, out Uri uri);
            endpointBaseUri = uri;
            client = new HttpClient(handler, true)
            {
                Timeout = Timeout.InfiniteTimeSpan,
            };
            this.apiVersion = apiVersion;
            defaultTimeout = configuration.DefaultTimeout;
        }

        private const string UserAgent = "DockerSdk";
        private readonly HttpClient client;
        private readonly TimeSpan defaultTimeout;
        private readonly Uri endpointBaseUri;
        private readonly Version? apiVersion;

        public void Dispose()
        {
            client.Dispose();
        }

        public Task<HttpResponseMessage> SendAsync(
            HttpMethod method,
            string path,
            string? query = null,
            HttpContent? content = null,
            IDictionary<string, string>? headers = null,
            HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead,
            TimeSpan? timeout = null,
            CancellationToken ct = default)
        {
            var request = PrepareRequest(method, path, query, headers, content);
            return PrivateMakeRequestAsync(timeout, completionOption, request, ct);
        }

        private Uri BuildUri(string path, string? query)
        {
            var builder = new UriBuilder(endpointBaseUri);

            if (apiVersion != null)
                builder.Path += $"v{apiVersion}/";

            if (!string.IsNullOrEmpty(path))
                builder.Path += path;

            if (!string.IsNullOrEmpty(query))
                builder.Query = query;

            return builder.Uri;
        }

        private HttpRequestMessage PrepareRequest(HttpMethod method, string path, string? query, IDictionary<string, string>? headers, HttpContent? content)
        {
            var request = new HttpRequestMessage
            {
                Method = method,
                RequestUri = BuildUri(path, query),
                Content = content,
            };

            request.Headers.Add("User-Agent", UserAgent);

            if (headers != null)
            {
                foreach (var header in headers)
                    request.Headers.Add(header.Key, header.Value);
            }

            return request;
        }

        private async Task<HttpResponseMessage> PrivateMakeRequestAsync(
                            TimeSpan? timeout,
            HttpCompletionOption completionOption,
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            timeout ??= defaultTimeout;

            if (timeout == Timeout.InfiniteTimeSpan)
            {
                var tcs = new TaskCompletionSource<HttpResponseMessage>();
                using (cancellationToken.Register(() => tcs.SetCanceled()))
                {
                    return await await Task.WhenAny(tcs.Task, client.SendAsync(request, completionOption, cancellationToken)).ConfigureAwait(false);
                }
            }
            else
            {
                using (var timeoutTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
                {
                    timeoutTokenSource.CancelAfter(timeout.Value);
                    return await client.SendAsync(request, completionOption, timeoutTokenSource.Token).ConfigureAwait(false);
                }
            }
        }
    }
}
