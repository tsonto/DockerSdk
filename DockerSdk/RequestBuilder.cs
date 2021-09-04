using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DockerSdk.Core;
using DockerSdk.Daemon;
using DockerSdk.Networks;
using DockerSdk.Registries;
using DockerSdk.Registries.Dto;
using DockerSdk.Volumes;

namespace DockerSdk
{
    /// <summary>
    /// Builder for constructing a message handler for sending an HTTP request.
    /// </summary>
    public class RequestBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequestBuilder"/> type.
        /// </summary>
        /// <param name="comm">The object responsible for the actual sending and recieving.</param>
        /// <param name="method">The HTTP method (fka verb) to use for the request.</param>
        /// <param name="path">The URL to send the request to, relative to the Docker daemon's API root.</param>
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

        private readonly List<HttpStatusCode> allowedStatusCodes = new() { HttpStatusCode.OK };

        private readonly Comm comm;

        private readonly List<Action<HttpStatusCode, string>> errorChecks = new();

        private readonly JsonSerializerOptions jsonOptions = new()
        {
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        };

        private readonly HttpMethod method;
        private readonly string path;
        private HttpContent? content;
        private string? parameters;
        private IDictionary<string, string>? requestHeaders;
        private TimeSpan? timeout;

        /// <summary>
        /// Indicates that a given status code is acceptable, and should not throw an exception.
        /// </summary>
        /// <param name="status">The status code to allow.</param>
        /// <returns>The same builder, for method chaining.</returns>
        /// <remarks>
        /// By default, only 200-OK is allowed, and any other 2xx code throws an "unexpected status" exception.
        /// </remarks>
        public RequestBuilder AcceptStatus(HttpStatusCode status)
        {
            allowedStatusCodes.Add(status);
            return this;
        }

        /// <summary>
        /// Specifies what exception to throw if the response matches the given conditions.
        /// </summary>
        /// <param name="status">An HTTP status code that the response must match.</param>
        /// <param name="makeException">Method to create the exception given the message's error text.</param>
        /// <returns>The same builder, for method chaining.</returns>
        public RequestBuilder RejectStatus(HttpStatusCode status, Func<string, Exception> makeException)
            => RejectStatus((s, e) => s == status, makeException);

        /// <summary>
        /// Specifies what exception to throw if the response matches the given conditions.
        /// </summary>
        /// <param name="status">An HTTP status code that the response must match.</param>
        /// <param name="contains">
        /// Text that the error message must contain for the response to be a match. This is compared
        /// case-insensitively.
        /// </param>
        /// <param name="makeException">Method to create the exception given the message's error text.</param>
        /// <returns>The same builder, for method chaining.</returns>
        public RequestBuilder RejectStatus(HttpStatusCode status, string contains, Func<string, Exception> makeException)
            => RejectStatus((s, e) => s == status && e.Contains(contains), makeException);

        /// <summary>
        /// Specifies what exception to throw if the response matches the given conditions.
        /// </summary>
        /// <param name="status">An HTTP status code that the response must match.</param>
        /// <param name="errorMatches">
        /// A condtion on the error text that must be true for the response to be a match.
        /// </param>
        /// <param name="makeException">Method to create the exception given the message's error text.</param>
        /// <returns>The same builder, for method chaining.</returns>
        public RequestBuilder RejectStatus(HttpStatusCode status, Func<string, bool> errorMatches, Func<string, Exception> makeException)
            => RejectStatus((s, e) => s == status && errorMatches(e), makeException);

        /// <summary>
        /// Specifies what exception to throw if the response matches the given conditions.
        /// </summary>
        /// <param name="matches">
        /// A condition for whether the response is a match. The inputs are the response code and the error message.
        /// </param>
        /// <param name="makeException">Method to create the exception given the message's error text.</param>
        /// <returns>The same builder, for method chaining.</returns>
        public RequestBuilder RejectStatus(Func<HttpStatusCode, string, bool> matches, Func<string, Exception> makeException)
        {
            errorChecks.Add((status, error) =>
            {
                if (matches(status, error))
                    throw makeException(error);
            });
            return this;
        }

        /// <summary>
        /// Sends the message, processes the response for errors, and channels the streamed response through an
        /// observable.
        /// </summary>
        /// <param name="ct">Allows cancelling the operation.</param>
        /// <returns>An observable that emits the streamed objects as they arrive.</returns>
        public Task<IObservable<T>> SendAndStreamResults<T>(CancellationToken ct)
            => SendAndStreamResults<T>(null, ct);

        /// <summary>
        /// Sends the message, processes the response for errors, and channels the streamed response through an
        /// observable.
        /// </summary>
        /// <param name="ct">Allows cancelling the operation.</param>
        /// <param name="serializerOptions">Options for deserializing the streamed objects.</param>
        /// <returns>An observable that emits the streamed objects as they arrive.</returns>
        public async Task<IObservable<T>> SendAndStreamResults<T>(JsonSerializerOptions? serializerOptions, CancellationToken ct)
        {
            var response = await SendAsync(HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
            return CreateStream<T>(response, serializerOptions);
        }

        /// <summary>
        /// Sends the message and processes the response for errors.
        /// </summary>
        /// <param name="ct">Allows cancelling the operation.</param>
        /// <returns>The HTTP response object.</returns>
        public Task<HttpResponseMessage> SendAsync(CancellationToken ct)
            => SendAsync(HttpCompletionOption.ResponseContentRead, ct);

        /// <summary>
        /// Sends the message, processes the response for errors, and deserializes the response body.
        /// </summary>
        /// <param name="ct">Allows cancelling the operation.</param>
        /// <returns>The deserialized response body.</returns>
        public async Task<T> SendAsync<T>(CancellationToken ct)
        {
            var response = await SendAsync(HttpCompletionOption.ResponseContentRead, ct).ConfigureAwait(false);
            return await response.DeserializeAsync<T>(jsonOptions, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// </summary>
        /// <param name="auth"></param>
        /// <returns>The same builder, for method chaining.</returns>
        public RequestBuilder WithAuthHeader(AuthConfig auth)
        {
            var value = Convert.ToBase64String(JsonSerializer.SerializeToUtf8Bytes(auth)).Replace("/", "_").Replace("+", "-"); // base64-url
            var dict = new Dictionary<string, string>
            {
                ["X-Registry-Auth"] = value,
            };
            return WithRequestHeaders(dict);
        }

        /// <summary>
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="headers"></param>
        /// <returns>The same builder, for method chaining.</returns>
        public RequestBuilder WithBody(Stream stream, Dictionary<string, string>? headers = null)
        {
            if (stream is null)
                throw new ArgumentNullException(nameof(stream));
            if (this.content != null)
                throw new InvalidOperationException("The request content has already been set.");
            this.content = new StreamContent(stream);
            AddHeaders(this.content.Headers, headers);
            return this;
        }

        /// <summary>
        /// </summary>
        /// <param name="content"></param>
        /// <returns>The same builder, for method chaining.</returns>
        public RequestBuilder WithBody(HttpContent content)
        {
            if (content is null)
                throw new ArgumentNullException(nameof(content));
            if (this.content != null)
                throw new InvalidOperationException("The request content has already been set.");
            this.content = content;
            return this;
        }

        /// <summary>
        /// </summary>
        /// <param name="body"></param>
        /// <param name="serializerOptions"></param>
        /// <param name="headers"></param>
        /// <returns>The same builder, for method chaining.</returns>
        public RequestBuilder WithJsonBody(object body, JsonSerializerOptions? serializerOptions = null, Dictionary<string, string>? headers = null)
        {
            if (body is null)
                throw new ArgumentNullException(nameof(body));
            if (this.content != null)
                throw new InvalidOperationException("The request content has already been set.");
            this.content = JsonContent.Create(body, body.GetType(), options: serializerOptions ?? jsonOptions);
            AddHeaders(content.Headers, headers);
            return this;
        }

        /// <summary>
        /// Sets the query parameters string to use when sending the request.
        /// </summary>
        /// <param name="parameters">The query string, which must already be percent-encodeded.</param>
        /// <returns>The same builder, for method chaining.</returns>
        public RequestBuilder WithParameters(string parameters)
        {
            this.parameters = parameters;
            return this;
        }

        /// <summary>
        /// </summary>
        /// <param name="headers"></param>
        /// <returns>The same builder, for method chaining.</returns>
        public RequestBuilder WithRequestHeaders(IReadOnlyDictionary<string, string>? headers)
        {
            if (headers == null)
                return this;

            if (this.requestHeaders == null)
            {
                this.requestHeaders = new Dictionary<string, string>(headers);
            }
            else
            {
                foreach (var pair in headers)
                    this.requestHeaders[pair.Key] = pair.Value;
            }

            return this;
        }

        /// <summary>
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns>The same builder, for method chaining.</returns>
        public RequestBuilder WithTimeout(TimeSpan? timeout)
        {
            this.timeout = timeout;
            return this;
        }

        private void AddHeaders(HttpHeaders headers, Dictionary<string, string>? dict)
        {
            if (dict == null)
                return;
            foreach (var kvp in dict)
                headers.Add(kvp.Key, kvp.Value);
        }

        private ResourceNotFoundException CreateResourceNotFoundException(string error)
        {
            var match = Regex.Match(error, "\"message\":\"network (.*) not found\"");
            if (match.Success)
            {
                var network = match.Groups[0].Value;
                return new NetworkNotFoundException($"No network with the name or ID \"{network}\" exists.");
            }

            return new ResourceNotFoundException(error);
        }

        private IObservable<T> CreateStream<T>(HttpResponseMessage response, JsonSerializerOptions? serializerOptions)
        {
            return Observable.Create<T>(
                observer =>
                {
                    // The following objects are disposed when the observable is disposed.
                    CancellationTokenSource cts = new();
                    Stream? stream = null;
                    StreamReader? reader = null;

                    var task = Task.Run(async () =>
                    {
                        stream = await response.Content.ReadAsStreamAsync(cts.Token).ConfigureAwait(false);
                        reader = new StreamReader(stream, Encoding.UTF8);
                        while (!cts.IsCancellationRequested)
                        {
                            string? line = await reader.ReadLineAsync().ConfigureAwait(false);
                            if (line == null)
                            {
                                observer.OnCompleted();
                                return;
                            }

                            T? item = JsonSerializer.Deserialize<T>(line, serializerOptions ?? jsonOptions);
                            if (item == null) // TODO: and T is not nullable
                            {
                                observer.OnError(new InvalidOperationException("Unexpected null item in HTTP stream."));
                                return;
                            }

                            observer.OnNext(item);
                        }
                    });

                    // Set the actions to take when the subscriber unsubscribes.
                    return () =>
                    {
                        cts.Cancel();
                        reader?.Dispose();
                        stream?.Dispose();

                        // Wait for the task to complete, to ensure that we surface any relevant exceptions.
                        try
                        {
                            task.Wait();
                        }
                        catch (ObjectDisposedException) { }
                        catch (OperationCanceledException) { }
                        catch (AggregateException ex) when (ex.InnerExceptions.All(e => e is ObjectDisposedException or OperationCanceledException)) { }

                        cts.Dispose();
                        response.Dispose();
                    };
                });
        }

        private async Task<HttpResponseMessage> SendAsync(HttpCompletionOption completionOption, CancellationToken ct)
        {
            var response = await comm.SendAsync(method, path, parameters, content, requestHeaders, completionOption, timeout, ct).ConfigureAwait(false);

            if (!allowedStatusCodes.Contains(response.StatusCode))
                await ThrowAsync(response).ConfigureAwait(false);

            return response;
        }

        private async Task ThrowAsync(HttpResponseMessage response)
        {
            try
            {
                var error = await response.DeserializeErrorMessageAsync().ConfigureAwait(false);
                var status = response.StatusCode;

                // Under some conditions the daemon will report a misleading HTTP status code. For the purposes of error
                // checking, correct for that.
                if (status == HttpStatusCode.NotFound && error.Contains("access to the resource is denied")) // attempt to access an image on a private registry without the credentials
                    status = HttpStatusCode.Unauthorized;
                else if (status == HttpStatusCode.InternalServerError && error.Contains("401 Unauthorized"))
                    status = HttpStatusCode.Unauthorized;
                else if (status == HttpStatusCode.InternalServerError && error.Contains("no basic auth credentials")) // attempt to access an image on a private registry that expects basic auth, but we gave it either no credentials or an identity token
                    status = HttpStatusCode.Unauthorized;

                // Run the checks provided by the caller.
                foreach (var check in errorChecks)
                    check(status, error);

                // Use RegistryAuthException for unhandled 401s.
                if (status == HttpStatusCode.Unauthorized)
                    throw new RegistryAuthException($"Authorization to the registry failed: {error}");

                // Use ResourceNotFoundException (or a subclass) for unhandled 404s.
                if (status == HttpStatusCode.NotFound)
                {
                    var match = Regex.Match(error, "^network (.*) not found$");
                    if (match.Success)
                        throw new NetworkNotFoundException($"No network with name or ID \"{match.Groups[1].Value}\" exists.");

                    match = Regex.Match(error, "^get (.*): no such volume$");
                    if (match.Success)
                        throw new VolumeNotFoundException($"No volume with name \"{match.Groups[1].Value}\" exists.");

                    throw CreateResourceNotFoundException(error);
                }

                // Use DockerDaemonException for unhandled 500s.
                if (status == HttpStatusCode.InternalServerError)
                    throw new DockerDaemonException($"The Docker daemon reported an internal error: {error}");

                // Use a general exception for everything else.
                throw new DockerException($"The request received unexpected response code {(int)response.StatusCode}: {error}");
            }
            catch (Exception ex)
            {
                // Augment the exception.
                ex.Data["Http.Method"] = method;
                ex.Data["Http.Path"] = path;
                ex.Data["Http.Status"] = response.StatusCode;
                throw;
            }
        }
    }
}
