using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace DockerSdk.Networks
{
    /// <summary>
    /// Represents a <a href="https://en.wikipedia.org/wiki/Subnetwork">subnetwork</a> of an IP network. This class
    /// supports IPv4 and IPv6 networks.
    /// </summary>
    public record IPSubnet
    {
        /// <summary>
        /// Creates a new instance of the <see cref="IPSubnet"/> class.
        /// </summary>
        /// <param name="address">The subnet's address, or any IP address within the subnet.</param>
        /// <param name="prefixLength">
        /// The length of the address prefix, in bits. (In CIDR notation, this is the number after the slash.)
        /// </param>
        public IPSubnet(IPAddress address, int prefixLength)
        {
            if (address is null)
                throw new ArgumentNullException(nameof(address));
            if (prefixLength < 0 || prefixLength > 8 * address.GetAddressBytes().Length)
                throw new ArgumentOutOfRangeException(nameof(prefixLength), "The prefix length must be non-negative and must not exceed the number of bits in the IP address.");

            Base = ToBaseAddress(address, prefixLength);
            PrefixLength = prefixLength;
        }

        private static IPAddress ToBaseAddress(IPAddress address, int prefixLength)
            => new IPAddress(KeepLeadingBits(address.GetAddressBytes(), prefixLength));

        /// <summary>
        /// Gets an <see cref="IPSubnet"/> object that represents all possible IPv4 IP addresses.
        /// </summary>
        public static IPSubnet EntireIPv4Space { get; } = new IPSubnet(new IPAddress(0), 0);

        /// <summary>
        /// Gets the IP address of the subnet.
        /// </summary>
        public IPAddress Base { get; }

        /// <summary>
        /// Gets the number of leading bits of the base address that are part of the subnet's prefix. That is, all
        /// addresses in the subnet will have the same value for this many of the leading bits.
        /// </summary>
        public int PrefixLength { get; }

        /// <summary>
        /// Tries to convert a given string in CIDR notation to a subnet object.
        /// </summary>
        /// <param name="cidr">The string to parse. This must be in valid CIDR format.</param>
        /// <param name="subnet">The resultant subnet object, or null if parsing failed.</param>
        /// <returns>True if parsing succeeded; false otherwise.</returns>
        public static bool TryParse(string? cidr, [NotNullWhen(returnValue: true)] out IPSubnet? subnet)
            => TryParse(cidr, out subnet, out _);

        /// <summary>
        /// Gets a string representation of the subnet, in CIDR notation.
        /// </summary>
        /// <returns>The subnet's range in CIDR notation.</returns>
        /// <remarks>IPv6 addresses will be given in shortened form.</remarks>
        public override string ToString()
            => $"{Base}/{PrefixLength}";

        /// <summary>
        /// Converts a given string in CIDR notation to a subnet object.
        /// </summary>
        /// <param name="cidr">The string to parse. This must be in valid CIDR format.</param>
        /// <returns>The resultant subnet object.</returns>
        /// <exception cref="ArgumentException">The input is null or is not in valid CIDR notation.</exception>
        public static IPSubnet Parse(string cidr)
            => TryParse(cidr, out var subnet)
            ? subnet
            : throw new ArgumentException($"\"{cidr}\" is not valid CIDR notation.");

        /// <summary>
        /// Tries to convert a given string in CIDR notation to a subnet object.
        /// </summary>
        /// <param name="cidr">The string to parse. This must be in valid CIDR format.</param>
        /// <param name="subnet">The resultant subnet object, or null if parsing failed.</param>
        /// <param name="givenAddress">The specific IP address given in CIDR input, or false if parsing failed..</param>
        /// <returns>True if parsing succeeded; false otherwise.</returns>
        public static bool TryParse(string? cidr, [NotNullWhen(returnValue: true)] out IPSubnet? subnet, [NotNullWhen(returnValue: true)] out IPAddress? givenAddress)
        {
            subnet = null;
            givenAddress = null;

            // Scrub the input.
            if (string.IsNullOrEmpty(cidr))
                return false;

            // Split the CIDR notation into its two parts.
            var parts = cidr.Split('/');
            if (parts.Length != 2)
                return false;

            // Parse the IP portion.
            if (!IPAddress.TryParse(parts[0], out givenAddress))
                return false;
            var maxBits = 8 * givenAddress.GetAddressBytes().Length;

            // Parse and validate.
            if (!int.TryParse(parts[1], out int prefixLength))
                return false;
            if (prefixLength < 0)
                return false;
            if (prefixLength > maxBits)
                return false;

            // Make the base address from the given address by zeroing the bits that aren't part of the prefix.
            var prefix = ToBaseAddress(givenAddress, prefixLength);

            // Make the result and return success.
            subnet = new IPSubnet(prefix, prefixLength);
            return true;
        }

        private static byte[] KeepLeadingBits(byte[] input, int numberOfBitsToKeep)
        {
            var output = new byte[input.Length];

            var fullBtyesToKeep = numberOfBitsToKeep / 8;
            var shift = numberOfBitsToKeep % 8;

            for (int b = 0; b < input.Length; ++b)
            {
                if (b < fullBtyesToKeep)
                    output[b] = input[b];
                else if (b > fullBtyesToKeep)
                    output[b] = 0;
                else
                    output[b] = (byte)(input[b] & (0xFF00 >> shift));
            }

            return output;
        }
    }
}
