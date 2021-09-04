using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DockerSdk.Networks.Dto
{
    internal class NetworkResponse
    {
        public string Name { get; set; } = null!;

        public string Id { get; set; } = null!;

        public DateTimeOffset Created { get; set; }

        public string Scope { get; set; } = null!;

        public string Driver { get; set; } = null!;

        public bool EnableIPv6 { get; set; }

        [JsonPropertyName("IPAM")]
        public Ipam Ipam { get; set; } = null!;

        public bool Internal { get; set; }

        public bool Attachable { get; set; }

        public bool Ingress { get; set; }

        public ConfigReference? ConfigFrom { get; set; }

        public bool? ConfigOnly { get; set; }

        public IDictionary<string, EndpointResource> Containers { get; set; } = null!;

        public IDictionary<string, string>? Options { get; set; }

        public IDictionary<string, string>? Labels { get; set; }

        public IList<PeerInfo>? Peers { get; set; }

        public IDictionary<string, ServiceInfo>? Services { get; set; }
    }
}
