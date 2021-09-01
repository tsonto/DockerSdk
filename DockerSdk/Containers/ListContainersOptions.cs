using System.Collections.Generic;
using System.Linq;
using DockerSdk.Core;

namespace DockerSdk.Containers
{
    /// <summary>
    /// Filters for container listings.
    /// </summary>
    /// <seealso cref="ContainerAccess.ListAsync(ListContainersOptions, System.Threading.CancellationToken)"/>
    public class ListContainersOptions
    {
        /// <summary>
        /// Gets options that are equivalent to the defaults for the `docker container ls` command.
        /// </summary>
        public static ListContainersOptions CommandLineDefaults => new()
        {
            OnlyRunningContainers = true,
        };

        /// <summary>
        /// Gets or sets an image reference for filtering. Only containers that have that image as an ancestor will be
        /// returned.
        /// </summary>
        /// <remarks>This may be an image name or ID.</remarks>
        public string? AncestorFilter { get; set; }

        /// <summary>
        /// Gets or sets an exit code for filtering. If this is not null, only containers that have exited with the
        /// given exit code will be returned. Null means no filtering based on exit code.
        /// </summary>
        public int? ExitCodeFilter { get; set; }

        /// <summary>
        /// Gets a list of labels to filter by. Only containers that have all of the given labels will be returned.
        /// </summary>
        /// <remarks>
        /// If both this setting and <see cref="LabelValueFilters"/> are set, they are combined with "or" logic.
        /// </remarks>
        public List<string> LabelExistsFilters { get; } = new();

        /// <summary>
        /// Gets a set of label-value pairs to filter by. Only containers that have all of the given labels set to the
        /// given values will be returned.
        /// </summary>
        /// <remarks>
        /// If both this setting and <see cref="LabelExistsFilters"/> are set, they are combined with "or" logic.
        /// </remarks>
        public Dictionary<string, string> LabelValueFilters { get; } = new();

        /// <summary>
        /// Gets or sets the maximum number of containers to return.
        /// </summary>
        /// <remarks>This returns the N most recently-created containers that match the filters.</remarks>
        public long? MaxResults { get; set; }

        /// <summary>
        /// Gets or sets a filter for the container name. If this is not null, only containers with this name will be
        /// returned.
        /// </summary>
        /// <remarks>
        /// Caution: Container names are not guaranteed to be unique, so this has a small chance of returning multiple
        /// containers.
        /// </remarks>
        public string? NameFilter { get; set; }

        /// <summary>
        /// Gets or sets a valid indicating whether to limit results to containers that are currently running.
        /// </summary>
        /// <remarks>
        /// Caution: It's possible for a status to change between the time the filter was applied and when the results
        /// are returned to the caller.
        /// </remarks>
        public bool OnlyRunningContainers { get; set; }

        /// <summary>
        /// Gets or sets a filter for the container status. If this is not null, only containers with the given status
        /// will be returned.
        /// </summary>
        /// <remarks>
        /// Caution: It's possible for a status to change between the time the filter was applied and when the results
        /// are returned to the caller.
        /// </remarks>
        public ContainerStatus? StatusFilter { get; set; }

        internal string ToQueryParameters()
        {
            var labels = LabelValueFilters.Select(kvp => $"{kvp.Key}={kvp.Value}").Concat(LabelExistsFilters);

            var filters = new QueryStringBuilder.StringStringBool();
            filters.Set("ancestor", AncestorFilter);
            filters.Set("exited", ExitCodeFilter);
            filters.Set("label", labels);
            filters.Set("name", NameFilter);
            filters.Set("status", StatusFilter?.ToString().ToLowerInvariant());
            // Note: This is not all available filters. As of 4/2021, these other filters exist but are not implemented
            // here: before, expose, health, id, isolation, is-task, network, publish, since, volume.

            var builder = new QueryStringBuilder();
            builder.Set("all", !OnlyRunningContainers, false);
            builder.Set("filters", filters);
            builder.Set("limit", MaxResults);
            return builder.Build();
        }
    }
}
