namespace DockerSdk.Images.Events
{
    /// <summary>
    /// The types of event that images can raise in the Docker event system.
    /// </summary>
    public enum ImageEventType
    {
        /// <summary>
        /// <para>An image has been locally deleted. This is not related to deleting the image from its registry.</para>
        /// <para>
        /// Events of this type are represented by the <see cref="ImageDeletedEvent"/> class. Docker documentation calls
        /// this event type "delete".
        /// </para>
        /// </summary>
        Deleted,

        /// <summary>
        /// <para>An image has been loaded from a file or stream.</para>
        /// </summary>
        /// <para>
        /// Events of this type are represented by the <see cref="ImageLoadedEvent"/> class. Docker documentation calls
        /// this event type "load".
        /// </para>
        Loaded,

        /// <summary>
        /// <para>An image has been pulled from its registry.</para>
        /// <para>
        /// Events of this type are represented by the <see cref="ImagePulledEvent"/> class. Docker documentation calls
        /// this event type "pull".
        /// </para>
        /// </summary>
        Pulled,

        /// <summary>
        /// <para>A local image has been pushed to its registry.</para>
        /// <para>
        /// Events of this type are represented by the <see cref="ImagePushedEvent"/> class. Docker documentation calls
        /// this event type "pull".
        /// </para>
        /// </summary>
        Pushed,

        /// <summary>
        /// <para>A local image has been saved to a file or stream.</para>
        /// </summary>
        /// <para>
        /// Events of this type are represented by the <see cref="ImageSavedEvent"/> class. Docker documentation calls
        /// this event type "save".
        /// </para>
        Saved,

        /// <summary>
        /// <para>A tag has been created or updated.</para>
        /// <para>
        /// Events of this type are represented by the <see cref="ImageTaggedEvent"/> class. Docker documentation
        /// calls this event type "tag".
        /// </para>
        /// </summary>
        Tagged,

        /// <summary>
        /// <para>A tag has been removed from a local image.</para>
        /// <para>
        /// Events of this type are represented by the <see cref="ImageUntaggedEvent"/> class. Docker documentation
        /// calls this event type "untag".
        /// </para>
        /// </summary>
        /// <remarks>
        /// When Docker updates an existing tag to point to a different image, it emits this event but does not also emit an
        /// untag event for the old image.
        /// </remarks>
        Untagged,
    }
}
