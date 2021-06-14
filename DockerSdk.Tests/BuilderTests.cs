using System;
using System.IO;
using System.Threading.Tasks;
using DockerSdk.Builders;
using DockerSdk.Images;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace DockerSdk.Tests
{
    [Collection("Common")]
    public class BuilderTests
    {
        public BuilderTests(ITestOutputHelper toh)
        {
            this.toh = toh;
        }

        private readonly ITestOutputHelper toh;

        [Fact]
        public async Task BasicCase()
        {
            using var client = await DockerClient.StartAsync();
            using var cli = new DockerCli(toh);
            var contextPath = MakeTempDirectory();
            ImageFullId? id = null;
            try
            {
                // Set up the build.
                MakeBareDockerfile(contextPath);
                var bundle = await Bundle.FromFilesAsync(contextPath, Array.Empty<string>()).ConfigureAwait(false);
                var uut = new Builder(client);

                // Build. This is the code under test.
                var actual = await uut.BuildAsync(bundle, new()).ConfigureAwait(false);

                // Verify that it created an image.
                actual.Should().NotBeNull();
                id = actual.Id;
            }
            finally
            {
                // Clean up.
                if (id is not null)
                    cli.RemoveImageIfPresent(id);
                DeleteDir(contextPath);
            }
        }

        [Fact]
        public async Task BasicCase_WithContextFilesUsingAbsolutePath()
        {
            using var client = await DockerClient.StartAsync();
            using var cli = new DockerCli(toh);
            var contextPath = MakeTempDirectory();
            ImageFullId? id = null;
            try
            {
                // Set up the build.
                var file1Path = Path.Combine(contextPath, "file1");
                File.WriteAllText(file1Path, "example contents 1");
                var file2Path = Path.Combine(contextPath, "file2");
                File.WriteAllBytes(file2Path, new byte[] { 1, 2, 3, 4, 5, 6, 8 });
                MakeSmallDockerfile(contextPath);
                var bundle = await Bundle.FromFilesAsync(contextPath, new[] { file1Path, file2Path }).ConfigureAwait(false);
                var uut = new Builder(client);

                // Build. This is the code under test.
                var actual = await uut.BuildAsync(bundle, new()).ConfigureAwait(false);

                // Verify that it created an image.
                actual.Should().NotBeNull();
                id = actual.Id;

                // Verify that the files were copied into the image.
                var output = cli.Invoke($"run {id} /bin/sh -c \"ls -1 file*\"");
                output.Should().BeEquivalentTo("file1", "file2");
            }
            finally
            {
                // Clean up.
                if (id is not null)
                    cli.RemoveImageIfPresent(id);
                DeleteDir(contextPath);
            }
        }

        [Fact]
        public async Task BasicCase_WithContextFilesUsingRelativePath()
        {
            using var client = await DockerClient.StartAsync();
            using var cli = new DockerCli(toh);
            var contextPath = MakeTempDirectory();
            ImageFullId? id = null;
            try
            {
                // Set up the build.
                var file1Path = "file1";
                File.WriteAllText(Path.Combine(contextPath, file1Path), "example contents 1");
                var file2Path = "file2";
                File.WriteAllBytes(Path.Combine(contextPath, file2Path), new byte[] { 1, 2, 3, 4, 5, 6, 8 });
                MakeSmallDockerfile(contextPath);
                var bundle = await Bundle.FromFilesAsync(contextPath, new[] { file1Path, file2Path }).ConfigureAwait(false);
                var uut = new Builder(client);

                // Build. This is the code under test.
                var actual = await uut.BuildAsync(bundle, new()).ConfigureAwait(false);

                // Verify that it created an image.
                actual.Should().NotBeNull();
                id = actual.Id;

                // Verify that the files were copied into the image.
                var output = cli.Invoke($"run {id} /bin/sh -c \"ls -1 file*\"");
                output.Should().BeEquivalentTo("file1", "file2");
            }
            finally
            {
                // Clean up.
                if (id is not null)
                    cli.RemoveImageIfPresent(id);
                DeleteDir(contextPath);
            }
        }

        [Fact]
        public async Task FilesMissingFromContext_ThrowsDockerImageBuildException()
        {
            using var client = await DockerClient.StartAsync();
            using var cli = new DockerCli(toh);
            var contextPath = MakeTempDirectory();
            IImage? image = null;
            try
            {
                // Set up the build.
                MakeBareDockerfile(contextPath, "COPY does-not-exist does-not-exist");
                var bundle = await Bundle.FromFilesAsync(contextPath, Array.Empty<string>()).ConfigureAwait(false);
                var uut = new Builder(client);

                // Build. This is the code under test.
                await Assert.ThrowsAsync<DockerImageBuildException>(
                    async () => image = await uut.BuildAsync(bundle, new()));
            }
            finally
            {
                // Clean up.
                if (image?.Id is not null)
                    cli.RemoveImageIfPresent(image.Id);
                DeleteDir(contextPath);
            }
        }

        [Fact]
        public async Task MalformedDockerfile_ThrowsDockerImageBuildException()
        {
            using var client = await DockerClient.StartAsync();
            using var cli = new DockerCli(toh);
            var contextPath = MakeTempDirectory();
            IImage? image = null;
            try
            {
                // Set up the build.
                MakeBrokenDockerfile(contextPath);
                var bundle = await Bundle.FromFilesAsync(contextPath, Array.Empty<string>()).ConfigureAwait(false);
                var uut = new Builder(client);

                // Build. This is the code under test.
                await Assert.ThrowsAsync<DockerImageBuildException>(
                    async () => image = await uut.BuildAsync(bundle, new()));
            }
            finally
            {
                // Clean up.
                if (image?.Id is not null)
                    cli.RemoveImageIfPresent(image.Id);
                DeleteDir(contextPath);
            }
        }

        [Fact]
        public async Task BasicCase_WithLabels()
        {
            using var client = await DockerClient.StartAsync();
            using var cli = new DockerCli(toh);
            var contextPath = MakeTempDirectory();
            ImageFullId? id = null;
            try
            {
                // Set up the build.
                MakeBareDockerfile(contextPath);
                var bundle = await Bundle.FromFilesAsync(contextPath, Array.Empty<string>()).ConfigureAwait(false);
                var uut = new Builder(client);
                var config = new BuildOptions
                {
                    Labels = { ["mammal"] = "gopher", ["bird"] = "falcon" },
                };

                // Build. This is the code under test.
                var actual = await uut.BuildAsync(bundle, config).ConfigureAwait(false);

                // Verify that it created an image.
                actual.Should().NotBeNull();
                id = actual.Id;

                // Verify that it has the expected tags.
                var output = cli.Invoke($"image inspect {id} --format=\"{{{{json .Config.Labels}}}}\"");
                output.Should().HaveCount(1);
                output[0].Should().Be("{\"bird\":\"falcon\",\"mammal\":\"gopher\"}");
            }
            finally
            {
                // Clean up.
                if (id is not null)
                    cli.RemoveImageIfPresent(id);
                DeleteDir(contextPath);
            }
        }

        [Fact]
        public async Task BasicCase_WithTags()
        {
            using var client = await DockerClient.StartAsync();
            using var cli = new DockerCli(toh);
            var contextPath = MakeTempDirectory();
            ImageFullId? id = null;
            try
            {
                // Set up the build.
                MakeBareDockerfile(contextPath);
                var bundle = await Bundle.FromFilesAsync(contextPath, Array.Empty<string>()).ConfigureAwait(false);
                var uut = new Builder(client);
                var config = new BuildOptions
                {
                    Tags = new[] { ImageName.Parse("ddnt:test-build-1"), ImageName.Parse("ddnt:test-build-2") },
                };

                // Build. This is the code under test.
                var actual = await uut.BuildAsync(bundle, config).ConfigureAwait(false);

                // Verify that it created an image.
                actual.Should().NotBeNull();
                id = actual.Id;

                // Verify that it has the expected tags.
                var output = cli.Invoke("image ls --filter=reference=ddnt:test-build-? -q");
                output.Should().HaveCount(2);
            }
            finally
            {
                // Clean up.
                if (id is not null)
                    cli.RemoveImageIfPresent(id);
                DeleteDir(contextPath);
            }
        }

        private static void DeleteDir(string path)
        {
            try
            {
                Directory.Delete(path, recursive: true);
            }
            catch (DirectoryNotFoundException)
            { }
        }

        private static void MakeBareDockerfile(string directory)
        {
            var path = Path.Combine(directory, "Dockerfile");
            File.WriteAllText(path, "FROM scratch\r\nCOPY . .");
        }

        private static void MakeBareDockerfile(string directory, string command)
        {
            var path = Path.Combine(directory, "Dockerfile");
            File.WriteAllText(path, "FROM scratch\r\n" + command);
        }

        private static void MakeBrokenDockerfile(string directory)
        {
            var path = Path.Combine(directory, "Dockerfile");
            File.WriteAllText(path, "@5RTEGRGQm$Q)#ERA(*GJ@#(@_!uI!@_$)%#^&(*^$%=%$^_&+%^$%)O#rKFVM VC\\IKRJGEIRPOgjSD{}:\"?><YUTHG,./F@`23");
        }

        private static void MakeSmallDockerfile(string directory)
        {
            var path = Path.Combine(directory, "Dockerfile");
            File.WriteAllText(path, "FROM busybox:1.33.1\r\nCOPY . .");
        }

        private static string MakeTempDirectory()
        {
            var path = Path.Combine(Path.GetTempPath(), "DockerSdk.Tests", Guid.NewGuid().ToString());
            Directory.CreateDirectory(path);
            return path;
        }
    }
}
