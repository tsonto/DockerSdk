using System.Collections.Generic;

namespace DockerSdk.Networks
{
    /// <summary>
    /// Specifies how to list images.
    /// </summary>
    /// <remarks>
    /// TODO: when the caller uses more than one kind of filter, does the API use "or" logic or "and" logic?
    /// </remarks>
    public class ListNetworksOptions
    {
        /// <summary>
        /// Gets or sets a filter for built-in networks. Built-in networks are ones provided by Docker, as opposed to
        /// custom networks that are created by Docker's consumers. True means that only built-in networks will be
        /// returned. False means that only custom networks will be returned. Null (the default) means not to filter by
        /// built-in vs. custom.
        /// </summary>
        public bool? BuiltInNetworksFilter { get; set; }

        /// <summary>
        /// Gets or sets a filter for dangling networks. Dangling networks are ones that are not currently in use by any
        /// containers. True means that only dangling networks will be returned; false means only non-dangling networks
        /// will be returned; and null (the default) means not to filter by whether the network is dangling.
        /// </summary>
        public bool? DanglingNetworksFilter { get; set; }

        /// <summary>
        /// Gets or sets a set of values naming the drivers to filter by. If this is not null, only networks that use
        /// one of the named drivers will be returned. The default is null, which means not to filter by driver. This
        /// field is case-sensitive.
        /// </summary>
        public IEnumerable<string>? DriverFilter { get; set; }

        /// <summary>
        /// Gets or sets a set of ID fragments to filter by. If this is not null, only networks whose full IDs contain
        /// one or more of these values will be returned. The default is null, which means not to filter by ID. This
        /// field is case-sensitive.
        /// </summary>
        /// <remarks>
        /// The match can be anywhere in the ID, including in parts of the ID that aren't visible in the short form of
        /// the ID.
        /// </remarks>
        public IEnumerable<string>? IdFilter { get; set; }

        /// <summary>
        /// Gets a list of labels to filter by. Only networks that have all of the given labels will be returned.
        /// </summary>
        /// <remarks>
        /// If both this setting and <see cref="LabelValueFilters"/> are set, they are combined with "or" logic.
        /// </remarks>
        public List<string> LabelExistsFilters { get; } = new();

        /// <summary>
        /// Gets a set of label-value pairs to filter by. Only networks that have all of the given labels set to the
        /// given values will be returned.
        /// </summary>
        /// <remarks>
        /// If both this setting and <see cref="LabelExistsFilters"/> are set, they are combined with "or" logic.
        /// </remarks>
        public Dictionary<string, string> LabelValueFilters { get; } = new();

        /// <summary>
        /// Gets or sets a set of name fragments to filter by. If this is not null, only networks whose names contain
        /// one or more of these values will be returned. The default is null, which means not to filter by name. This
        /// field is case-sensitive.
        /// </summary>
        /// <remarks>The match can be anywhere in the name--not just at the beginning.</remarks>
        public IEnumerable<string>? NameFilter { get; set; }

        /// <summary>
        /// Gets or sets a network scope to filter for. If this is not null, only networks with the indicated scope will
        /// be returned. The default is null, which means to not filter by scope.
        /// </summary>
        public NetworkScope? ScopeFilter { get; set; }
    }
}
