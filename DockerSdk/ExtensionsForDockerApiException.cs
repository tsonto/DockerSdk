using System.Text.Json;

using DockerSdk.Core;

namespace DockerSdk
{
    internal static class ExtensionsForDockerApiException
    {
        public static string ReadJsonMessage(this Core.DockerApiException ex)
        {
            try
            {
                var bodyElement = JsonSerializer.Deserialize<JsonElement>(ex.Message);
                var messageElement = bodyElement.GetProperty("message");
                var messageText = messageElement.GetString();
                if (string.IsNullOrEmpty(messageText))
                    return ex.Message;
                return messageText;
            }
            catch
            {
                // We couldn't get the message node for some reason, so just return the original text.
                return ex.Message;
            }
        }
    }
}
