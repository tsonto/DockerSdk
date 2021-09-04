namespace DockerSdk.Networks.Dto
{
    internal class NetworkConnectParameters
    {
        public string? Container { get; set; }

        public EndpointSettings? EndpointConfig { get; set; }
    }
}
