using System;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;

namespace DockerSdk.Containers
{
    /// <summary>
    /// Specifies a port mapping from a Docker container's port to a port on the host.
    /// </summary>
    public record PortBinding
    {
        /// <summary>
        /// Creates an instance of the <see cref="PortBinding"/> class.
        /// </summary>
        /// <param name="containerPort">The port number within the container.</param>
        /// <param name="hostPort">The port number of the host that it maps to.</param>
        /// <remarks>This maps to the loopback address, and allows both UDP and TCP traffic.</remarks>
        public PortBinding(ushort containerPort, ushort hostPort)
            : this(containerPort, TransportType.All, IPAddress.Loopback, hostPort)
        { }

        /// <summary>
        /// Creates an instance of the <see cref="PortBinding"/> class.
        /// </summary>
        /// <param name="portsAndTransport">
        /// A string representation of the port mapping. This is typically in <c>container:host</c> or
        /// <c>container:host/transport</c> format, but see the Remarks for details.
        /// </param>
        /// <remarks>
        /// <para>
        /// The basic format is <c>container:host/transport</c>, where the first part is the port number within the
        /// container, the second is the port number on the host, and the third is either <c>tcp</c> or <c>udp</c>. If
        /// the <c>:host</c> part is omitted, it defaults to the container port. If the <c>/transport</c> part is
        /// omitted, both TCP and UDP communications are allowed.
        /// </para>
        /// <para>
        /// This maps to the lookback address. If you need to map to a specific adaptor, use one of the other overloads.
        /// </para>
        /// </remarks>
        public PortBinding(string portsAndTransport)
        {
            ushort hostPort;
            (ContainerPort, Transport, hostPort) = ParsePortsAndTransport(portsAndTransport);
            HostEndpoint = new IPEndPoint(IPAddress.Loopback, hostPort);
        }

        /// <summary>
        /// Creates an instance of the <see cref="PortBinding"/> class.
        /// </summary>
        /// <param name="containerPortAndTransport">
        /// The port number within the container, optionally followed by either <c>:udp</c> or <c>:tcp</c> to limit
        /// which transport to allow.
        /// </param>
        /// <param name="hostIPAndPort">
        /// The IP address of the host adaptor to map to and/or the port on the host to map to. If both are given, they
        /// must be separated by a colon. The default for the IP address is the loopback address. The default for the
        /// port is the container port.
        /// </param>
        public PortBinding(string containerPortAndTransport, string hostIPAndPort)
        {
            (ContainerPort, Transport) = ParsePortAndTransport(containerPortAndTransport);
            HostEndpoint = ParseHostEndpoint(hostIPAndPort, ContainerPort);
        }

        /// <summary>
        /// Creates an instance of the <see cref="PortBinding"/> class, with a limitation on transport type.
        /// </summary>
        /// <param name="containerPort">The port number within the container.</param>
        /// <param name="transport">Which transports type to allow through the mapping.</param>
        /// <param name="hostPort">The port number of the host.</param>
        /// <remarks>This maps to the loopback address.</remarks>
        public PortBinding(ushort containerPort, TransportType transport, ushort hostPort)
            : this(containerPort, transport, IPAddress.Loopback, hostPort)
        { }

        /// <summary>
        /// Creates an instance of the <see cref="PortBinding"/> class, specifying the network adaptor to map to.
        /// </summary>
        /// <param name="containerPort">The port number within the container.</param>
        /// <param name="hostAddress">The IP address of the network adaptor to map to.</param>
        /// <param name="hostPort">The port number of the host.</param>
        /// <remarks>This allows both TCP and UDP traffic.</remarks>
        public PortBinding(ushort containerPort, IPAddress hostAddress, ushort hostPort)
            : this(containerPort, TransportType.All, new IPEndPoint(hostAddress, hostPort))
        { }

        /// <summary>
        /// Creates an instance of the <see cref="PortBinding"/> class, specifying the network adaptor to map to and
        /// with a limitation on transport type.
        /// </summary>
        /// <param name="containerPort">The port number within the container.</param>
        /// <param name="transport">Which transports type to allow through the mapping.</param>
        /// <param name="hostAddress">The IP address of the network adaptor to map to.</param>
        /// <param name="hostPort">The port number of the host.</param>
        public PortBinding(ushort containerPort, TransportType transport, IPAddress hostAddress, ushort hostPort)
            : this(containerPort, transport, new IPEndPoint(hostAddress, hostPort))
        { }

        /// <summary>
        /// Creates an instance of the <see cref="PortBinding"/> class, specifying the network adaptor to map to.
        /// </summary>
        /// <param name="containerPort">The port number within the container.</param>
        /// <param name="hostEndpoint">
        /// Specifies which of the host's network adaptors to map to (specified as an IP address) and which port on that
        /// adaptor to use.
        /// </param>
        /// <remarks>This allows both TCP and UDP traffic.</remarks>
        public PortBinding(ushort containerPort, IPEndPoint hostEndpoint)
            : this(containerPort, TransportType.All, hostEndpoint)
        { }

        /// <summary>
        /// Creates an instance of the <see cref="PortBinding"/> class, specifying the network adaptor to map to and
        /// with a limitation on transport type.
        /// </summary>
        /// <param name="containerPort">The port number within the container.</param>
        /// <param name="transport">Which transports type to allow through the mapping.</param>
        /// <param name="hostEndpoint">
        /// Specifies which of the host's network adaptors to map to (specified as an IP address) and which port on that
        /// adaptor to use.
        /// </param>
        /// <remarks>This allows both TCP and UDP traffic.</remarks>
        public PortBinding(ushort containerPort, TransportType transport, IPEndPoint hostEndpoint)
        {
            ContainerPort = containerPort;
            Transport = transport;
            HostEndpoint = hostEndpoint ?? throw new ArgumentNullException(nameof(hostEndpoint));
        }

        private IPEndPoint ParseHostEndpoint(string hostIPAndPort, ushort defaultPort)
        {
            if (string.IsNullOrEmpty(hostIPAndPort))
                throw new ArgumentException($"'{nameof(hostIPAndPort)}' cannot be null or empty.", nameof(hostIPAndPort));

            var parts = hostIPAndPort.Split(':');
            if (parts.Length > 2)
                throw new ArgumentException($"Invalid host specification \"{hostIPAndPort}\".", nameof(hostIPAndPort));
            if (parts.Length == 2)
                return ParseHostEndpoint(parts[0], parts[1]);
            if (ushort.TryParse(parts[0], out var port))
                return new IPEndPoint(IPAddress.Loopback, port);
            if (IPAddress.TryParse(parts[0], out var address))
                return new IPEndPoint(address, defaultPort);
            throw new ArgumentException($"Invalid host specification \"{hostIPAndPort}\".", nameof(hostIPAndPort));
        }

        private static IPEndPoint ParseHostEndpoint(string hostIP, string hostPort)
        {
            if (string.IsNullOrEmpty(hostIP))
                throw new ArgumentException($"'{nameof(hostIP)}' cannot be null or empty.", nameof(hostIP));
            if (string.IsNullOrEmpty(hostPort))
                throw new ArgumentException($"'{nameof(hostPort)}' cannot be null or empty.", nameof(hostPort));

            if (!IPAddress.TryParse(hostIP, out var address))
                throw new ArgumentException($"Invalid IP address \"{hostIP}\".", nameof(hostIP));
            if (!ushort.TryParse(hostPort, NumberStyles.Integer, CultureInfo.InvariantCulture, out var port))
                throw new ArgumentException($"Invalid port number \"{hostPort}\".", nameof(hostPort));

            return new IPEndPoint(address, port);
        }

        private static (ushort port, TransportType transport) ParsePortAndTransport(string containerPortAndTransport)
        {
            if (string.IsNullOrEmpty(containerPortAndTransport))
                throw new ArgumentException($"'{nameof(containerPortAndTransport)}' cannot be null or empty.", nameof(containerPortAndTransport));

            var match = _portAndTransportRegex.Match(containerPortAndTransport);
            if (!match.Success)
                throw new ArgumentException($"Invalid port specification \"{containerPortAndTransport}\".", nameof(containerPortAndTransport));

            var portAsString = match.Groups["port"].Value;
            if (!ushort.TryParse(portAsString, NumberStyles.Integer, CultureInfo.InvariantCulture, out var port))
                throw new ArgumentException($"Invalid port specification \"{containerPortAndTransport}\": The numeric part is out of range for a port number.", nameof(containerPortAndTransport));

            var transport = match.Groups["transport"].Value switch
            {
                "tcp" => TransportType.Tcp,
                "udp" => TransportType.Udp,
                _ => TransportType.All
            };

            return (port, transport);
        }

        private (ushort ContainerPort, TransportType Transport, ushort hostPort) ParsePortsAndTransport(string portsAndTransport)
        {
            if (string.IsNullOrEmpty(portsAndTransport))
                throw new ArgumentException($"'{nameof(portsAndTransport)}' cannot be null or empty.", nameof(portsAndTransport));

            var match = _portsAndTransportRegex.Match(portsAndTransport);
            if (!match.Success)
                throw new ArgumentException($"Invalid mapping specification \"{portsAndTransport}\".", nameof(portsAndTransport));

            var containerPortAsString = match.Groups["containerport"].Value;
            if (!ushort.TryParse(containerPortAsString, NumberStyles.Integer, CultureInfo.InvariantCulture, out var containerPort))
                throw new ArgumentException($"Invalid port specification \"{portsAndTransport}\": The container port number is out of range.", nameof(portsAndTransport));

            var hostPortAsString = match.Groups["hostport"].Value;
            if (string.IsNullOrEmpty(hostPortAsString))
                hostPortAsString = containerPortAsString;
            if (!ushort.TryParse(hostPortAsString, NumberStyles.Integer, CultureInfo.InvariantCulture, out var hostPort))
                throw new ArgumentException($"Invalid port specification \"{portsAndTransport}\": The host port number is out of range.", nameof(portsAndTransport));

            var transport = match.Groups["transport"].Value switch
            {
                "tcp" => TransportType.Tcp,
                "udp" => TransportType.Udp,
                _ => TransportType.All
            };

            return (containerPort, transport, hostPort);
        }

        private static readonly Regex _portAndTransportRegex = new(
            @"^
                (?<port>[0-9]+)
                (
                    /
                    (?<transport>tcp|udp)
                )?
            $",
            RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace);

        private static readonly Regex _portsAndTransportRegex = new(
            @"^
                (?<containerport>[0-9]+)
                (
                    :
                    (?<hostport>[0-9]+)
                )?
                (
                    /
                    (?<transport>tcp|udp)
                )?
            $",
            RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace);

        /// <summary>
        /// Gets which transport types to map.
        /// </summary>
        public TransportType Transport { get; }

        /// <summary>
        /// Gets the port number within the container to map.
        /// </summary>
        public ushort ContainerPort { get; }

        /// <summary>
        /// Gets the host endpoint to map to. This encodes both the network adaptor (as represented by its IP address)
        /// and port number.
        /// </summary>
        public IPEndPoint HostEndpoint { get; }
    }
}
