using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DockerSdk.Images;
using CoreModels = Docker.DotNet.Models;

namespace DockerSdk.Builders
{
    public class Builder
    {
        public Builder(DockerClient client)
        {
            this.client = client;
        }

        private readonly DockerClient client;

        public async Task<IImage> BuildAsync(IBundle bundle, BuildOptions options, CancellationToken ct = default)
        {
            var request = new CoreModels.ImageBuildParameters
            {
                Dockerfile = bundle.DockerfilePath,
                
                Labels = options.Labels,
                NoCache = !options.UseBuildCache,
                Tags = options.Tags.Select(name => name.ToString()).ToList(),
                Target = options.TargetBuildStage,
            };

            using var bundleReader = await bundle.OpenTarForReadAsync().ConfigureAwait(false);

            Stream imageStream;
            try
            {
                imageStream = await client.Core.Images.BuildImageFromDockerfileAsync(bundleReader, request, ct).ConfigureAwait(false);
            }
            catch
            {
                throw;
            }

            using var progressReader = new StreamReader(imageStream);
            var lines = progressReader.ReadToEnd();

            var messages = lines.Split('\n')
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => JsonSerializer.Deserialize<JsonElement>(line))
                .ToArray();
            var idMessage = messages.Single(msg => msg.TryGetProperty("aux", out _));  // TODO: better error messages
            var imageIdString = idMessage.GetProperty("aux").GetProperty("ID").GetString() ?? throw new InvalidOperationException("The image ID is blank.");
            return new Image(client, ImageFullId.Parse(imageIdString));
        }
    }
}
