using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace DockerSdk
{
    public static class ExtensionsForHttpResponseMessage
    {
        public static Task<T> DeserializeAsync<T>(this HttpResponseMessage message, CancellationToken ct)
            => DeserializeAsync<T>(message, null, ct);

        public static async Task<T> DeserializeAsync<T>(this HttpResponseMessage message, JsonSerializerOptions? options, CancellationToken ct)
        {
            var stream = await message.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
            T? body = await JsonSerializer.DeserializeAsync<T>(stream, options, ct);
            if (body == null)
                throw new InvalidOperationException("Response body evaluated to null.");
            return body;
        }

        public static async Task ThrowIfNotStatusAsync(this HttpResponseMessage message, params HttpStatusCode[] allowedCodes)
        {
            var status = message.StatusCode;
            if (allowedCodes.Contains(status))
                return;
            await ThrowAsync(message).ConfigureAwait(false);
        }

        public static async Task ThrowIfNotSuccessfulAsync(this HttpResponseMessage message)
        {
            var status = message.StatusCode;
            if ((int)status is >= 200 and < 300)
                return;
            await ThrowAsync(message).ConfigureAwait(false);
        }

        private class ErrorMessage
        {
            [JsonPropertyName("message")]
            public string? Message { get; set; }
        }

        private static async Task ThrowAsync(HttpResponseMessage message)
        {
            string? errorFromBody;
            try
            {
                var errorStructure = await DeserializeAsync<ErrorMessage>(message, default);
                errorFromBody = errorStructure.Message;
            }
            catch
            {
                errorFromBody = null;
            }

            string MakeText(string s)
                => string.IsNullOrEmpty(errorFromBody)
                ? $"{s}."
                : $"{s}: {errorFromBody}";

            var status = (int)message.StatusCode;
            Exception ex = message.StatusCode switch
            {
                HttpStatusCode.InternalServerError => new DockerDaemonException(MakeText("The daemon reported an internal error")),
                _ => new DockerException(MakeText($"Unexpected response {status}")),
            };

            ex.Data["StatusCode"] = message.StatusCode;
            ex.Data["URL"] = message.RequestMessage?.RequestUri?.ToString();

            throw ex;
        }
    }
}
