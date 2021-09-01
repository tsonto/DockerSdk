using System;
using System.Collections.Generic;
using System.Linq;
using DockerSdk.Core;

namespace DockerSdk.Images
{
    /// <summary>
    /// Specifies how to list images.
    /// </summary>
    /// <remarks>
    /// TODO: when the caller uses more than one kind of filter, does the API use "or" logic or "and" logic?
    /// </remarks>
    public class ListImagesOptions
    {
        /// <summary>
        /// Gets options that are equivalent to the defaults for the `docker image ls` command.
        /// </summary>
        public static ListImagesOptions CommandLineDefaults => new()
        {
            HideIntermediateImages = true,
        };

        // TODO: BeforeImageFilter, if there's sufficient need for it

        /// <summary>
        /// Gets or sets a filter for dangling images. True means that only dangling images will be returned; false
        /// means only non-dangling images will be returned; and null means not to filter by whether the image is
        /// dangling.
        /// </summary>
        public bool? DanglingImagesFilter { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to filter out intermediate images.
        /// </summary>
        public bool HideIntermediateImages { get; set; }

        /// <summary>
        /// Gets a list of labels to filter by. Only images that have all of the given labels will be returned.
        /// </summary>
        /// <remarks>
        /// If both this setting and <see cref="LabelValueFilters"/> are set, they are combined with "or" logic.
        /// </remarks>
        public List<string> LabelExistsFilters { get; } = new();

        /// <summary>
        /// Gets a set of label-value pairs to filter by. Only images that have all of the given labels set to the given
        /// values will be returned.
        /// </summary>
        /// <remarks>
        /// If both this setting and <see cref="LabelExistsFilters"/> are set, they are combined with "or" logic.
        /// </remarks>
        public Dictionary<string, string> LabelValueFilters { get; } = new();

        /// <summary>
        /// Gets a list of glob patterns to filter by. Only images with references matching one or more of the patterns
        /// will be returned. If this list is blank, pattern filters are not applied.
        /// </summary>
        /// <remarks>
        /// <para>The following matching patterns are respected:
        /// <list type="bullet">
        /// <item><c>*</c> matches zero or more characters</item>
        /// <item><c>?</c> matches exactly one character</item>
        /// <item><c>[</c>...<c>]</c> matches exactly one character in the set</item>
        /// </list>
        /// </para>
        /// <para>
        /// The patterns can match on the repository (in which case all tags for that repository are returned) or the
        /// repository:tag or the repository@digest. Patterns do not match against the ID.
        /// </para>
        /// <para>
        /// Caution: If you use <c>*</c> without <c>:</c> or <c>@</c>, you might accidentally match more than you expect.
        /// For example, glob "a*b" will match "axb:1.0", "a:xb", and
        /// "ax@sha256:bf756fb1ae65adf866bd8c456593cd24beb6a0a061dedf42b26a993176745f6b". Even with <c>:</c> it's
        /// possible to accidentally match a digest, such as "a*:*b" matching
        /// "ax@sha256:bf756fb1ae65adf866bd8c456593cd24beb6a0a061dedf42b26a993176745f6b".
        /// </para>
        /// </remarks>
        public List<string> ReferencePatternFilters { get; } = new();

        internal string ToQueryString()
        {
            var dangling = DanglingImagesFilter switch
            {
                true => "true",
                false => "false",
                null => null
            };

            var labels = LabelValueFilters.Select(kvp => $"{kvp.Key}={kvp.Value}").Concat(LabelExistsFilters);

            var filters = new QueryStringBuilder.StringStringBool();
            filters.Set("dangling", dangling);
            filters.Set("label", labels);
            filters.Set("references", ReferencePatternFilters);

            var builder = new QueryStringBuilder();
            builder.Set("all", !HideIntermediateImages, false);
            builder.Set("filters", filters);
            return builder.Build();
        }

        // TODO: SinceImageFilter, if there's sufficient need for it
    }
}
