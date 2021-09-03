using Microsoft.Net.Http.Client;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DockerSdk.Core
{
    public sealed class Comm : IDisposable
    {
        private const string UserAgent = "Docker.DotNet";
        private readonly Uri _endpointBaseUri;
        private readonly HttpClient _client;
        private readonly Version? _requestedApiVersion;
        private readonly TimeSpan _defaultTimeout;

        public Comm(DockerClientConfiguration configuration, Version? requestedApiVersion)
        {
            HttpMessageHandler handler = ManagedHandler.Create(configuration, out Uri uri);
            _endpointBaseUri = uri;
            _client = new HttpClient(handler, true)
            {
                Timeout = Timeout.InfiniteTimeSpan,
            };
            _requestedApiVersion = requestedApiVersion;
            _defaultTimeout = configuration.DefaultTimeout;
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

        private async Task<HttpResponseMessage> PrivateMakeRequestAsync(
            TimeSpan? timeout,
            HttpCompletionOption completionOption,
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            timeout ??= _defaultTimeout;

            if (timeout == Timeout.InfiniteTimeSpan)
            {
                var tcs = new TaskCompletionSource<HttpResponseMessage>();
                using (cancellationToken.Register(() => tcs.SetCanceled()))
                {
                    return await await Task.WhenAny(tcs.Task, _client.SendAsync(request, completionOption, cancellationToken)).ConfigureAwait(false);
                }
            }
            else
            {
                using (var timeoutTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
                {
                    timeoutTokenSource.CancelAfter(timeout.Value);
                    return await _client.SendAsync(request, completionOption, timeoutTokenSource.Token).ConfigureAwait(false);
                }
            }
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

        private Uri BuildUri(string path, string? query)
        {
            var builder = new UriBuilder(_endpointBaseUri);

            if (this._requestedApiVersion != null)
                builder.Path += $"v{this._requestedApiVersion}/";

            if (!string.IsNullOrEmpty(path))
                builder.Path += path;

            if (!string.IsNullOrEmpty(query))
                builder.Query = query;

            return builder.Uri;
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
