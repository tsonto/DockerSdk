using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using DockerSdk.Builders;
using DockerSdk.Daemon;

namespace DockerSdk
{
    /// <summary>
    /// Provides extension methods for the <see cref="HttpResponseMessage"/> class.
    /// </summary>
    public static class ExtensionsForHttpResponseMessage
    {
        /// <summary>
        /// Deserializes the response's body contents. The contents must be a JSON representation of the requested
        /// return type.
        /// </summary>
        /// <typeparam name="T">The desired return tyoe.</typeparam>
        /// <param name="message">The HTTP response message to read from.</param>
        /// <param name="ct">Can cancel the operation.</param>
        /// <returns>A <see cref="Task"/> that resolves to the requested type.</returns>
        /// <exception cref="JsonException">
        /// The contents were not valid JSON, or the JSON is not compatible with the requested type. Note that if the
        /// message has a non-success status code, the content body will usually have an error structure instead of the
        /// normal content.
        /// </exception>
        /// <exception cref="InvalidOperationException">The body is empty except for a JSON null.</exception>
        public static Task<T> DeserializeAsync<T>(this HttpResponseMessage message, CancellationToken ct)
            => DeserializeAsync<T>(message, null, ct);

        /// <summary>
        /// Deserializes the response's body contents. The contents must be a JSON representation of the requested
        /// return type.
        /// </summary>
        /// <typeparam name="T">The desired return tyoe.</typeparam>
        /// <param name="message">The HTTP response message to read from.</param>
        /// <param name="options">Options for how to deserialize the body content.</param>
        /// <param name="ct">Can cancel the operation.</param>
        /// <returns>A <see cref="Task"/> that resolves to the requested type.</returns>
        /// <exception cref="JsonException">
        /// The contents were not valid JSON, or the JSON is not compatible with the requested type. Note that if the
        /// message has a non-success status code, the content body will usually have an error structure instead of the
        /// normal content.
        /// </exception>
        /// <exception cref="InvalidOperationException">The body is empty except for a JSON null.</exception>
        public static async Task<T> DeserializeAsync<T>(this HttpResponseMessage message, JsonSerializerOptions? options, CancellationToken ct)
        {
            var stream = await message.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
            T? body = await JsonSerializer.DeserializeAsync<T>(stream, options, ct);
            if (body == null)
                throw new InvalidOperationException("Response body evaluated to null.");
            return body;
        }

        /// <summary>
        /// Special case of <see cref="DeserializeAsync{T}(HttpResponseMessage, CancellationToken)"/> for when the
        /// caller expects the body to contain an error structure. Unwraps the message from the rest of the structure.
        /// </summary>
        /// <param name="message">The HTTP response message to read from.</param>
        /// <returns>
        /// A <see cref="Task"/> that resolves to the text of the error message. If the structure was malformed or
        /// empty, this method returns an empty string.
        /// </returns>
        public static async Task<string> DeserializeErrorMessageAsync(this HttpResponseMessage message)
        {
            try
            {
                var errorStructure = await DeserializeAsync<ErrorMessage>(message, default);
                return errorStructure.Message ?? "";
            }
            catch
            {
                return "";
            }
        }

        private class ErrorMessage
        {
            [JsonPropertyName("message")]
            public string? Message { get; set; }
        }
    }
}
