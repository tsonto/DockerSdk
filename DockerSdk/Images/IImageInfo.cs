﻿using System;
using System.Collections.Generic;

namespace DockerSdk.Images
{
    /// <remarks>This class holds a snapshot in time. Its information is immutable once created.</remarks>
    public interface IImageInfo : IImage
    {
        /// <summary>
        /// Gets the image's author, or a blank string if no author information is available.
        /// </summary>
        string Author { get; }

        /// <summary>
        /// Gets the comment saved with the image, or a blank string if no comment is available.
        /// </summary>
        string Comment { get; }

        /// <summary>
        /// Gets the time at which the image was created.
        /// </summary>
        DateTimeOffset CreationTime { get; }

        /// <summary>
        /// Gets the image's digest.
        /// </summary>
        /// <remarks>
        /// This is a hash of the image's manifest, which is effectively a list of the layers used to produce the image.
        /// It represents the image's build inputs--that is, if the build is changed, the digest will change too. Note
        /// that this value is not calculated until the image is pushed/pulled.
        /// </remarks>
        string? Digest { get; }

        /// <summary>
        /// Gets the labels that have been applied to the image.
        /// </summary>
        IReadOnlyDictionary<string, string> Labels { get; }

        /// <summary>
        /// Gets the kind of OS that containers made from this image run.
        /// </summary>
        /// <remarks>
        /// The only situation where this can be Windows is when running Docker for Windows (which only runs on Windows
        /// hosts) in Windows containers mode. In that situation Linux containers will not be visible.
        /// </remarks>
        GuestOsType OsType { get; }

        /// <summary>
        /// Gets the image that this image was built from, of any.
        /// </summary>
        /// <remarks>
        /// Due to the way that Docker works, this will usually be null:
        /// <list type="bullet">
        /// <item>
        /// Images using image manifest v2 (the default since Docker 1.3.0) that are pulled from a registry will never
        /// have a parent image set.
        /// </item>
        /// <item>Images built with Buildkit (the default since Docker 20.10) seem to never set the parent.</item>
        /// </list>
        /// When the parent image is null, the history will not have any image/layer identifiers either, except the
        /// current image's.
        /// </remarks>
        IImage? ParentImage { get; }

        /// <summary>
        /// Gets the size, in bytes, of the image's writable layer.
        /// </summary>
        /// <remarks>
        /// Mounting the first container for the image will consume memory equal to the virtual size plus the size. Each
        /// subsequent container will consume memory equal to the size. These sizes do not include space consumed for
        /// log files, volumes, configuration files, swap space, or checkpoints.
        /// </remarks>
        long Size { get; }

        /// <summary>
        /// Gets the names that the image is known by.
        /// </summary>
        IReadOnlyList<ImageName> Tags { get; }

        /// <summary>
        /// Gets the size, in bytes, of the image's read-only layers.
        /// </summary>
        /// <remarks>
        /// Mounting the first container for the image will consume memory equal to the virtual size plus the size. Each
        /// subsequent container will consume memory equal to the size. These sizes do not include space consumed for
        /// log files, volumes, configuration files, swap space, or checkpoints.
        /// </remarks>
        long VirtualSize { get; }

        /// <summary>
        /// Gets the image's initial working directory. If the image did not specify a working directory, this will be
        /// set to the filesystem's root path.
        /// </summary>
        /// <remarks>This directory is guaranteed to exist inside the image.</remarks>
        string WorkingDirectory { get; }
    }
}
