namespace DockerSdk.Registries.Dto
{
    internal class AuthResponse
    {
        public string? IdentityToken { get; set; }

        public string Status { get; set; } = null!;
    }
}
