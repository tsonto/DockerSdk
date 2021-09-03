using System.Text.Json.Serialization;

namespace DockerSdk.Registries.Dto
{
    // TODO: don't expose this directly to the public?
    public class AuthConfig
    {
        [JsonPropertyName("username")]
        public string? Username { get; set; }

        [JsonPropertyName("password")]
        public string? Password { get; set; }

        [JsonPropertyName("auth")]
        public string? Auth { get; set; }

        [JsonPropertyName("email")]
		public string? Email { get; set; }

        [JsonPropertyName("serveraddress")]
		public string? ServerAddress { get; set; }

        [JsonPropertyName("identitytoken")]
		public string? IdentityToken { get; set; }

        [JsonPropertyName("registrytoken")]
		public string? RegistryToken { get; set; }
    }
}
