using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net;

namespace DockerSdk.Networks
{
    /// <summary>
    /// Represents an IP subnet with information about how to allocate IP addresse from it.
    /// </summary>
    public record NetworkPool
    {
        /// <summary>
        /// Gets the IP subnet for this pool.
        /// </summary>
        public IPSubnet Subnet { get; init; } = IPSubnet.EntireIPv4Space;

        /// <summary>
        /// Gets the IP address if the subnet's gateway.
        /// </summary>
        public IPAddress? Gateway { get; init; }

        /// <summary>
        /// Gets the range of IP addresses that Docker is allowed to assign to container endpoints. If this is null, the
        /// entire subnet is fair game.
        /// </summary>
        /// <seealso cref="AuxilliaryAddresses"/>
        public IPSubnet? Range { get; init; }

        /// <summary>
        /// Gets IP addresses that Docker should not assign to container endpoints. This property's keys are the
        /// hostnames of the hosts that use the IP addresses.
        /// </summary>
        /// <seealso cref="Range"/>
        /// <remarks>
        /// This list holds only user-specified entries. Docker does not add entries when it creates endpoints (even if
        /// you re-fetch this information).
        /// </remarks>
        public IReadOnlyDictionary<string, IPAddress> AuxilliaryAddresses { get; init; } = ImmutableDictionary<string, IPAddress>.Empty;
    }
}
