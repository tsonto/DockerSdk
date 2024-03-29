﻿using DockerSdk.Events.Dto;

namespace DockerSdk.Containers.Events
{
    /// <summary>
    /// Represents a notification that the container has been deleted.
    /// </summary>
    public class ContainerDeletedEvent : ContainerEvent
    {
        internal ContainerDeletedEvent(Message message) : base(message, ContainerEventType.Deleted)
        {
        }
    }
}
