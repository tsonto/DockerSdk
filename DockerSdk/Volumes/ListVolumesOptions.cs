using System;
using System.Collections.Generic;
using System.Linq;

namespace DockerSdk.Volumes
{
    /// <summary>
    /// Specifies how to list volumes.
    /// </summary>
    public class ListVolumesOptions
    {
        /// <summary>
        /// Gets or sets a filter for dangling volumes. False means to return only volumes in use by one or more
        /// containers; true means to only return volumes that are not in use by containers; and null means not to
        /// filter by whether the volume is dangling.
        /// </summary>
        public bool? DanglingVolumesFilter { get; set; }

        /// <summary>
        /// If set, the query will only return volumes that use the specified driver.
        /// </summary>
        public string? DriverFilter { get; set; }

        /// <summary>
        /// Gets a list of labels to filter by. Only volumes that have all of the given labels will be returned.
        /// </summary>
        /// <remarks>
        /// If both this setting and <see cref="LabelValueFilters"/> are set, they are combined with "or" logic.
        /// </remarks>
        public List<string> LabelExistsFilters { get; } = new();

        /// <summary>
        /// Gets a set of label-value pairs to filter by. Only volumes that have all of the given labels set to the given
        /// values will be returned.
        /// </summary>
        /// <remarks>
        /// If both this setting and <see cref="LabelExistsFilters"/> are set, they are combined with "or" logic.
        /// </remarks>
        public Dictionary<string, string> LabelValueFilters { get; } = new();

        /// <summary>
        /// If set, the query will only return volumes whose names contain the given text.
        /// </summary>
        public string? NameContainsFilter { get; set; }

        internal string ToQueryString()
        {
            var dangling = DanglingVolumesFilter switch
            {
                true => "true",
                false => "false",
                null => null
            };

            var labels = LabelValueFilters.Select(kvp => $"{kvp.Key}={kvp.Value}").Concat(LabelExistsFilters);

            var filters = new QueryStringBuilder.StringStringBool();
            filters.Set("dangling", dangling);
            filters.Set("driver", DriverFilter);
            filters.Set("label", labels);
            filters.Set("name", NameContainsFilter);

            var builder = new QueryStringBuilder();
            builder.Set("filters", filters);
            return builder.Build();
        }
    }
}
