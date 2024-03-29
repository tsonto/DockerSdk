﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DockerSdk.Builders
{
    /// <summary>
    /// Provides convenience methods for creating <see cref="IBundle"/> objects.
    /// </summary>
    public static class Bundle
    {
        /// <summary>
        /// Creates a <see cref="IBundle"/> object from provided files.
        /// </summary>
        /// <param name="contextPath">
        /// The path to the folder that holds the Dockerfile and that serves as the context root for files specified in
        /// <paramref name="filePaths"/>. If this is a relative path, it's taken as relative to the process's current
        /// working path.
        /// </param>
        /// <param name="filePaths">
        /// The paths to additional files to make available to the build process. These files must all be within the
        /// context path ( <paramref name="contextPath"/>). Any relative paths are taken as relative to the context
        /// path.
        /// </param>
        /// <param name="ct">A token used to cancel the operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> that resolves to the resultant <see cref="IBundle"/>.</returns>
        /// <remarks>
        /// The bundle generated by this method holds a compressed copy of the input files as they were at the time the
        /// method ran.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// One or more of the files are not located within the context path.
        /// </exception>
        /// <exception cref="IOException">An I/O error occurred while trying to read one of the files.</exception>
        /// <exception cref="UnauthorizedAccessException">
        /// Either one of the files is a actually directory, or the process does not have permission to access one of
        /// the local files.
        /// </exception>
        /// <exception cref="NotSupportedException">One of the file paths is in an invalid format.</exception>
        public static Task<IBundle> FromFilesAsync(string contextPath, IEnumerable<string> filePaths, CancellationToken ct = default)
            => FromFilesAsync(contextPath, "Dockerfile", filePaths, ct);

        /// <summary>
        /// Creates a <see cref="IBundle"/> object from provided files.
        /// </summary>
        /// <param name="contextPath">
        /// The path to the folder that serves as the context root for files specified in <paramref
        /// name="dockerfilePath"/> and <paramref name="filePaths"/>. If this is a relative path, it's taken as relative
        /// to the process's current working path.
        /// </param>
        /// <param name="dockerfilePath">
        /// The path to the Dockerfile. This file must be within the context path ( <paramref name="contextPath"/>). If
        /// this is a relative path, it's taken as relative to the context path.
        /// </param>
        /// <param name="filePaths">
        /// The paths to additional files to make available to the build process. These files must all be within the
        /// context path ( <paramref name="contextPath"/>). Any relative paths are taken as relative to the context
        /// path.
        /// </param>
        /// <param name="ct">A token used to cancel the operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> that resolves to the resultant <see cref="IBundle"/>.</returns>
        /// <remarks>
        /// The bundle generated by this method holds a compressed copy of the input files as they were at the time the
        /// method ran.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// One or more of the files are not located within the context path.
        /// </exception>
        /// <exception cref="IOException">An I/O error occurred while trying to read one of the files.</exception>
        /// <exception cref="UnauthorizedAccessException">
        /// Either one of the files is a actually directory, or the process does not have permission to access one of
        /// the local files.
        /// </exception>
        /// <exception cref="NotSupportedException">One of the file paths is in an invalid format.</exception>
        public static async Task<IBundle> FromFilesAsync(string contextPath, string dockerfilePath, IEnumerable<string> filePaths, CancellationToken ct = default)
        {
            // Validate the input.
            if (string.IsNullOrEmpty(contextPath))
                throw new ArgumentException($"'{nameof(contextPath)}' cannot be null or empty.", nameof(contextPath));
            if (string.IsNullOrEmpty(dockerfilePath))
                throw new ArgumentException($"'{nameof(dockerfilePath)}' cannot be null or empty.", nameof(dockerfilePath));
            if (filePaths is null)
                throw new ArgumentNullException(nameof(filePaths));

            // Expand the context path.
            contextPath = Path.GetFullPath(contextPath);

            // Convert the Dockerfile path to a path relative to the context path. Throw if it's not within the context
            // path.
            dockerfilePath = GetRelativePath(contextPath, dockerfilePath);
            if (dockerfilePath.Contains(".."))
                throw new ArgumentException("The Dockerfile must be within the context path.", nameof(dockerfilePath));

            // Add the Dockerfile to the paths, if it isn't already in the list. The daemon requires that it be included
            // in the TAR.
            filePaths = filePaths.Prepend(dockerfilePath).Distinct();

            // Create a pair for each file: ReadPath is its absolute location on disk, and LabelPath is its path
            // relative to the archive's root. Throw an exception if any of the files aren't within the context.
            var entries = filePaths
                .Select(path => (
                    ReadPath: Path.GetFullPath(path, contextPath),
                    LabelPath: GetRelativePath(contextPath, path).Replace('\\', '/')))
                .Distinct()
                .ToArray();
            if (entries.Any(entry => entry.LabelPath.Contains("..")))
                throw new ArgumentException("All file paths must be within the context path.", nameof(filePaths));

            // Create the TAR file. FromFilesInner is a blocking method that might take a noticeable time to run, so
            // shunt it to a background task.
            var tar = await Task.Run(() => MakeTarArchive(entries, ct), ct).ConfigureAwait(false);

            // Create the bundle object.
            return new InMemoryBundle(dockerfilePath, tar);
        }

        private static string GetRelativePath(string contextPath, string filePath)
        {
            if (Path.IsPathRooted(filePath))
                return Path.GetRelativePath(contextPath, filePath);
            else
                return Path.GetRelativePath(contextPath, Path.Combine(contextPath, filePath));
        }

        private static byte[] MakeTarArchive(IEnumerable<(string ReadPath, string LabelPath)> entries, CancellationToken ct)
        {
            using var destination = new MemoryStream();
            var options = new SharpCompress.Writers.Tar.TarWriterOptions(SharpCompress.Common.CompressionType.GZip, finalizeArchiveOnClose: true);
            using var writer = new SharpCompress.Writers.Tar.TarWriter(destination, options);
            foreach (var (readPath, labelPath) in entries)
            {
                ct.ThrowIfCancellationRequested();
                using var readStream = File.OpenRead(readPath);
                writer.Write(labelPath, readStream, DateTime.Now);
            }

            ct.ThrowIfCancellationRequested();

            writer.Dispose(); // must dispose this before ToArray or we'll get an empty array
            return destination.ToArray();
        }
    }
}
