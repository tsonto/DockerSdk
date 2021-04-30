using System;

namespace DockerSdk.Networks
{
    /// <summary>
    /// Represents the name or ID of a Docker network.
    /// </summary>
    /// <remarks>
    /// Unlike other *Reference classes, <see cref="NetworkReference"/> lacks public Parse and TryParse methods. This is
    /// because Docker is so permissive with network names that we too often can't determine whether a string is a name
    /// vs. an ID. Docker solved the same problem for containers and images by adding a rule about which to prefer in
    /// case of ambiguity, which is why <see cref="DockerSdk.Images.ImageReference"/> and <see
    /// cref="DockerSdk.Containers.ContainerReference"/> have such methods, but Docker does not presently (as of
    /// 2021-04-27) seem to have such a rule for network references.
    /// </remarks>
    public record NetworkReference
    {
        /// <summary>
        /// The full text of the reference.
        /// </summary>
        protected readonly string value;

        internal NetworkReference(string value)
            => this.value = value;

        /// <summary>
        /// Lets the reference be implicitly cast to a string.
        /// </summary>
        /// <param name="reference"></param>
        public static implicit operator string(NetworkReference reference)
            => reference.ToString();

        /// <inheritdoc/>
        public override string ToString() => value;

        internal static NetworkReference Parse(string input)
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            if (NetworkName.TryParse(input, out _) || NetworkId.TryParse(input, out _))
                return new NetworkReference(input);
            throw new MalformedReferenceException($"\"{input}\" is not a valid Docker network name or ID.");
        }
    }
}
