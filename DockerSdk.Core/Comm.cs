using Microsoft.Net.Http.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DockerSdk.Core
{
    public sealed class Comm : IDisposable
    {
        private const string UserAgent = "Docker.DotNet";
        private readonly Uri _endpointBaseUri;
        private readonly HttpClient _client;
        private readonly Version? _requestedApiVersion;
        private readonly TimeSpan _defaultTimeout;
        private readonly JsonSerializerOptions _defaultSerializerOptions;

        public Comm(DockerClientConfiguration configuration, Version? requestedApiVersion, JsonSerializerOptions? defaultSerializerOptions = null)
        {
            HttpMessageHandler handler = ManagedHandler.Create(configuration, out Uri uri);
            _endpointBaseUri = uri;
            _client = new HttpClient(handler, true)
            {
                Timeout = Timeout.InfiniteTimeSpan,
            };
            _requestedApiVersion = requestedApiVersion;
            _defaultTimeout = configuration.DefaultTimeout;
            _defaultSerializerOptions = defaultSerializerOptions ?? new();
        }

        //internal async Task<DockerApiResponse> MakeRequestAsync(
        //	IEnumerable<ApiResponseErrorHandlingDelegate> errorHandlers,
        //	HttpMethod method,
        //	string path,
        //	IQueryString queryString,
        //	IRequestContent body,
        //	IDictionary<string, string> headers,
        //	TimeSpan? timeout = null,
        //	CancellationToken token = default)
        //{
        //	timeout = timeout ?? this.DefaultTimeout;
        //	var response = await PrivateMakeRequestAsync(timeout, HttpCompletionOption.ResponseContentRead, method, path, queryString, headers, body, token).ConfigureAwait(false);
        //	using (response)
        //	{
        //		await HandleIfErrorResponseAsync(response.StatusCode, response, errorHandlers);

        //		var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        //		return new DockerApiResponse(response.StatusCode, responseBody);
        //	}
        //}


        internal async Task<WriteClosableStream> MakeRequestForHijackedStreamAsync(
            IEnumerable<ApiResponseErrorHandlingDelegate> errorHandlers,
            HttpMethod method,
            string path,
            string? query = null,
            HttpContent? body = null,
            IDictionary<string, string>? headers = null,
            CancellationToken cancellationToken = default)
        {
            var response = await PrivateMakeRequestAsync(Timeout.InfiniteTimeSpan, HttpCompletionOption.ResponseHeadersRead, method, path, query, headers, body, cancellationToken).ConfigureAwait(false);

            await HandleIfErrorResponseAsync(response.StatusCode, response, errorHandlers);

            if (response.Content is not HttpConnectionResponseContent content)
                throw new NotSupportedException("message handler does not support hijacked streams");

            return content.HijackStream();
        }

        internal async Task<DockerApiStreamedResponse> MakeRequestForStreamedResponseAsync(
            IEnumerable<ApiResponseErrorHandlingDelegate> errorHandlers,
            HttpMethod method,
            string path,
            string? query,
            CancellationToken token)
        {
            var response = await PrivateMakeRequestAsync(Timeout.InfiniteTimeSpan, HttpCompletionOption.ResponseHeadersRead, method, path, query, null, null, token);

            await HandleIfErrorResponseAsync(response.StatusCode, response, errorHandlers);

            var body = await response.Content.ReadAsStreamAsync(token);

            return new DockerApiStreamedResponse(response.StatusCode, body, response.Headers);
        }

        internal Task<HttpResponseMessage> MakeRequestForRawResponseAsync(
            HttpMethod method,
            string path,
            string? query,
            HttpContent body,
            IDictionary<string, string> headers,
            CancellationToken token)
            => PrivateMakeRequestAsync(Timeout.InfiniteTimeSpan, HttpCompletionOption.ResponseHeadersRead, method, path, query, headers, body, token);

        public Task<HttpResponseMessage> SendAsync(
            HttpMethod method,
            string path,
            string? query = null,
            HttpContent? content = null,
            IDictionary<string, string>? headers = null,
            TimeSpan? timeout = null,
            CancellationToken ct = default)
        {
            var request = PrepareRequest(method, path, query, headers, content);
            return PrivateMakeRequestAsync(timeout, HttpCompletionOption.ResponseHeadersRead, request, ct);
        }

        internal Task<HttpResponseMessage> SendAsync(
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

        public async Task<T> SendAndDeserializeAsync<T>(
            HttpMethod method,
            string path,
            Func<HttpResponseMessage, Task>? checkResponseAsync,
            string? query = null,
            HttpContent? content = null,
            IDictionary<string, string>? headers = null,
            TimeSpan? timeout = null,
            JsonSerializerOptions? serializerOptions = null,
            CancellationToken ct = default)
        {
            var request = PrepareRequest(method, path, query, headers, content);
            var response = await PrivateMakeRequestAsync(timeout, HttpCompletionOption.ResponseHeadersRead, request, ct).ConfigureAwait(false);
            if (checkResponseAsync != null)
                await checkResponseAsync(response).ConfigureAwait(false);

            var stream = await response.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
            serializerOptions ??= _defaultSerializerOptions;
            var result = await System.Text.Json.JsonSerializer.DeserializeAsync<T>(stream, serializerOptions, ct).ConfigureAwait(false);
            if (result == null) // TODO: and T is not nullable
                throw new InvalidOperationException();

            return result;
        }

        

        private Task<HttpResponseMessage> PrivateMakeRequestAsync(
            TimeSpan? timeout,
            HttpCompletionOption completionOption,
            HttpMethod method,
            string path,
            string? query,
            IDictionary<string, string>? headers,
            HttpContent? data,
            CancellationToken cancellationToken)
        {
            var request = PrepareRequest(method, path, query, headers, data);
            return PrivateMakeRequestAsync(timeout, completionOption, request, cancellationToken);
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

        private static async Task HandleIfErrorResponseAsync(HttpStatusCode statusCode, HttpResponseMessage response, IEnumerable<ApiResponseErrorHandlingDelegate> handlers)
        {
            bool isErrorResponse = statusCode < HttpStatusCode.OK || statusCode >= HttpStatusCode.BadRequest;

            string? responseBody = null;

            if (isErrorResponse)
            {
                // If it is not an error response, we do not read the response body because the caller may wish to consume it.
                // If it is an error response, we do because there is nothing else going to be done with it anyway and
                // we want to report the response body in the error message as it contains potentially useful info.
                responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }

            // If no customer handlers just default the response.
            if (handlers != null)
            {
                foreach (var handler in handlers)
                {
                    handler(statusCode, responseBody);
                }
            }

            // No custom handler was fired. Default the response for generic success/failures.
            if (isErrorResponse)
            {
                throw new DockerApiException(statusCode, responseBody);
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

    internal delegate void ApiResponseErrorHandlingDelegate(HttpStatusCode statusCode, string? responseBody);
}
