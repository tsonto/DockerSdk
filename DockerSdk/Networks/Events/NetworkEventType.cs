namespace DockerSdk.Networks.Events
{
    /// <summary>
    /// The types of event that networks can raise in the Docker event system.
    /// </summary>
    public enum NetworkEventType
    {
        /// <summary>
        /// <para>The creation of a new Docker network resource.</para>
        /// <para>
        /// Events of this type are represented by the <see cref="NetworkCreatedEvent"/> class. Docker documentation
        /// calls this event type "create".
        /// </para>
        /// </summary>
        Created,

        /// <summary>
        /// <para>A network has been attached to a container.</para>
        /// <para>
        /// Events of this type are represented by the <see cref="NetworkAttachedEvent"/> class. Docker documentation
        /// calls this event type "connect".
        /// </para>
        /// </summary>
        Attached,

        /// <summary>
        /// <para>The deletion of an existing Docker network resource.</para>
        /// <para>
        /// Events of this type are represented by the <see cref="NetworkDeletedEvent"/> class. Docker documentation
        /// calls this event type "destroy".
        /// </para>
        /// </summary>
        Deleted,

        /// <summary>
        /// <para>A network has been detached from a container.</para>
        /// <para>
        /// Events of this type are represented by the <see cref="NetworkDetachedEvent"/> class. Docker documentation
        /// calls this event type "disconnect".
        /// </para>
        /// </summary>
        /// <remarks>Docker sometimes (always?) emits two of these events when detaching.</remarks>
        Detached,
    }
}
