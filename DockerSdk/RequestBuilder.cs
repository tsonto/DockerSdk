using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DockerSdk.Core;

namespace DockerSdk
{
    public class RequestBuilder
    {
        public RequestBuilder(Comm comm, HttpMethod method, string path)
        {
            if (comm is null)
                throw new ArgumentNullException(nameof(comm));
            if (method is null)
                throw new ArgumentNullException(nameof(method));
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException($"'{nameof(path)}' cannot be null or empty.", nameof(path));

            this.comm = comm;
            this.method = method;
            this.path = path;
        }

        private readonly Comm comm;
        private readonly HttpMethod method;
        private readonly string path;
        private static readonly List<HttpStatusCode> defaultAllowedStatusCodes = new int[] { 200, 201, 202, 204 }.Select(code => (HttpStatusCode)code).ToList();
        private QueryParameters? parameters;
        private HttpContent? content;
        private List<Action<HttpStatusCode, string>> errorChecks = new();
        private List<HttpStatusCode> allowedStatusCodes = new();
        private IDictionary<string, string>? headers;
        private TimeSpan? timeout;

        public RequestBuilder RejectStatus(HttpStatusCode status, Func<string, Exception> makeException)
            => RejectStatus((s, e) => s == status, makeException);

        public RequestBuilder RejectStatus(HttpStatusCode status, string contains, Func<string, Exception> makeException)
            => RejectStatus((s, e) => s == status && e.Contains(contains), makeException);

        public RequestBuilder RejectStatus(HttpStatusCode status, Func<string, bool> errorMatches, Func<string, Exception> makeException)
            => RejectStatus((s, e) => s == status && errorMatches(e), makeException);

        public RequestBuilder RejectStatus(Func<HttpStatusCode, string, bool> matches, Func<string, Exception> makeException)
        {
            errorChecks.Add((status, error) =>
            {
                if (matches(status, error))
                    throw makeException(error);
            });
            return this;
        }

        public RequestBuilder AcceptStatus(HttpStatusCode status)
        {
            allowedStatusCodes.Add(status);
            return this;
        }

        public RequestBuilder WithParameters(QueryParameters? parameters)
        {
            if (parameters == null)
                return this;
            
            if (this.parameters == null)
                this.parameters = parameters;
            else
                this.parameters.AddRange(parameters);
            return this;
        }

        public RequestBuilder WithBody(Stream stream)
        {
            if (stream is null)
                throw new ArgumentNullException(nameof(stream));
            if (this.content != null)
                throw new InvalidOperationException("The request content has already been set.");
            this.content = new StreamContent(stream);
            return this;
        }

        public RequestBuilder WithBody(HttpContent content)
        {
            if (content is null)
                throw new ArgumentNullException(nameof(content));
            if (this.content != null)
                throw new InvalidOperationException("The request content has already been set.");
            this.content = content;
            return this;
        }

        public RequestBuilder WithHeaders(IReadOnlyDictionary<string, string>? headers)
        {
            if (headers == null)
                return this;

            if (this.headers == null)
            {
                this.headers = new Dictionary<string, string>(headers);
            }
            else
            {
                foreach (var pair in headers)
                    this.headers[pair.Key] = pair.Value;
            }

            return this;
        }

        public RequestBuilder WithTimeout(TimeSpan? timeout)
        {
            this.timeout = timeout;
            return this;
        }

        public Task<HttpResponseMessage> SendAsync(CancellationToken ct)
            => SendAsync(HttpCompletionOption.ResponseContentRead, ct);

        private  async Task<HttpResponseMessage> SendAsync(HttpCompletionOption completionOption, CancellationToken ct)
        {
            var response = await comm.SendAsync(method, path, parameters, content, headers, timeout, completionOption, ct).ConfigureAwait(false);

            if (!allowedStatusCodes.Any())
                allowedStatusCodes = defaultAllowedStatusCodes;

            if (!allowedStatusCodes.Contains(response.StatusCode))
                await ThrowAsync(response).ConfigureAwait(false);

            return response;
        }

        public async Task<T> SendAsync<T>(CancellationToken ct)
        {
            var response = await SendAsync(HttpCompletionOption.ResponseContentRead, ct).ConfigureAwait(false);
            return await response.DeserializeAsync<T>(ct).ConfigureAwait(false);
        }

        private async Task ThrowAsync(HttpResponseMessage response)
        {
            try
            {
                var error = await response.DeserializerErrorMessageAsync().ConfigureAwait(false);
                foreach (var check in errorChecks)
                    check(response.StatusCode, error);

                if (response.StatusCode == HttpStatusCode.InternalServerError)
                    throw new DockerDaemonException($"The Docker daemon reported an internal error: {error}");

                throw new DockerException($"The request received unexpected response code {(int)response.StatusCode}: {error}");
            }
            catch (Exception ex)
            {
                ex.Data["Http.Method"] = method;
                ex.Data["Http.Path"] = path;
                ex.Data["Http.Status"] = response.StatusCode;
                throw;
            }
        }

        public Task<IObservable<T>> SendAndStreamResults<T>(CancellationToken ct)
            => SendAndStreamResults<T>(new(), ct);

        public async Task<IObservable<T>> SendAndStreamResults<T>(JsonSerializerOptions serializerOptions, CancellationToken ct)
        {
            var response = await SendAsync(HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
            return CreateStream<T>(response, serializerOptions)
                .SubscribeOn(ThreadPoolScheduler.Instance)
                .ObserveOn(ThreadPoolScheduler.Instance);
        }

        private IObservable<T> CreateStream<T>(HttpResponseMessage response, JsonSerializerOptions serializerOptions)
        {
            return Observable.Create<T>(
                observer =>
                {
                    using CancellationTokenSource cts = new();
                    var task = Task.Run(async () =>
                    {
                        using var stream = await response.Content.ReadAsStreamAsync(cts.Token).ConfigureAwait(false);
                        using var reader = new StreamReader(stream, Encoding.UTF8);
                        try
                        {
                            while (!cts.IsCancellationRequested)
                            {
                                string? line = await reader.ReadLineAsync().ConfigureAwait(false);
                                if (line == null)
                                {
                                    observer.OnCompleted();
                                    return;
                                }

                                T? item = JsonSerializer.Deserialize<T>(line, serializerOptions);
                                if (item == null) // TODO: and T is not nullable
                                {
                                    observer.OnError(new InvalidOperationException("Unexpected null item in HTTP stream."));
                                    return;
                                }

                                observer.OnNext(item);
                            }
                        }
                        catch (ObjectDisposedException) { }
                        catch (OperationCanceledException) { }
                    });

                    return () =>
                    {
                        cts.Dispose();
                        task.Wait();
                    };
                });
        }

    }
}
