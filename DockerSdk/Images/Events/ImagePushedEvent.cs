﻿using DockerSdk.Registries;
using DockerSdk.Events.Dto;

namespace DockerSdk.Images.Events
{
    /// <summary>
    /// Represents a notification of a local image being pushed to a registry.
    /// </summary>
    public class ImagePushedEvent : ImageEvent
    {
        internal ImagePushedEvent(Message message) : base(message, ImageEventType.Pushed)
        {
        }

        /// <summary>
        /// Gets the image's full ID.
        /// </summary>
        public ImageName ImageName => (ImageName)ImageReference;

        /// <summary>
        /// Gets the Docker registry that the image was pushed to.
        /// </summary>
        public RegistryReference Registry => RegistryAccess.GetRegistryName(ImageName);
            
    }
}
