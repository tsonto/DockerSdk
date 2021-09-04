using System.Text.Json.Serialization;

namespace DockerSdk.Networks.Dto
{
    internal class Address
    {
        /// <summary>
        /// IP address.
        /// </summary>
        [JsonPropertyName("Addr")]
        public string Addr { get; set; } = null!;

        /// <summary>
        /// Mask length of the IP address.
        /// </summary>
        public long PrefixLen { get; set; }
    }
}
