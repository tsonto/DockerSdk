using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DockerSdk.Builders
{
    public static class Bundle
    {
        public static Task<IBundle> FromFilesAsync(string contextPath, IEnumerable<string> filePaths, CancellationToken ct = default)
            => FromFilesAsync(contextPath, "Dockerfile", filePaths, ct);

        public static async Task<IBundle> FromFilesAsync(string contextPath, string dockerfilePath, IEnumerable<string> filePaths, CancellationToken ct = default)
        {
            if (string.IsNullOrEmpty(contextPath))
                throw new ArgumentException($"'{nameof(contextPath)}' cannot be null or empty.", nameof(contextPath));
            if (string.IsNullOrEmpty(dockerfilePath))
                throw new ArgumentException($"'{nameof(dockerfilePath)}' cannot be null or empty.", nameof(dockerfilePath));
            if (filePaths is null)
                throw new ArgumentNullException(nameof(filePaths));

            contextPath = Path.GetFullPath(contextPath);

            dockerfilePath = GetRelativePath(contextPath, dockerfilePath);
            if (dockerfilePath.Contains(".."))
                throw new ArgumentException("The Dockerfile must be within the context path.", nameof(dockerfilePath));

            filePaths = filePaths.Prepend(dockerfilePath).Distinct();

            var entries = filePaths
                .Select(path => (
                    ReadPath: Path.GetFullPath(path, contextPath),
                    LabelPath: GetRelativePath(contextPath, path).Replace('\\', '/')))
                .ToArray();
            if (entries.Any(entry => entry.LabelPath.Contains("..")))
                throw new ArgumentException("All file paths must be within the context path.", nameof(filePaths));

            // FromFilesInner is a blocking method that might take a noticeable time to run, so shunt it to a background
            // task.
            var tar = await Task.Run(() => MakeTarArchive(entries, ct), ct).ConfigureAwait(false);

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

    public interface IBundle
    {
        /// <summary>
        /// Gets the relative path to the Dockerfile, from the archive's root.
        /// </summary>
        public string DockerfilePath { get; }

        public Task<Stream> OpenTarForReadAsync();
    }

    internal class InMemoryBundle : IBundle
    {
        public InMemoryBundle(string dockerfilePath, byte[] tar)
        {
            DockerfilePath = dockerfilePath;
            this.tar = tar;
        }

        public string DockerfilePath { get; }
        private readonly byte[] tar;

        public Task<Stream> OpenTarForReadAsync()
            => Task.FromResult<Stream>(new MemoryStream(tar));
    }
}
