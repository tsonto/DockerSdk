using System;
using System.Threading;
using System.Threading.Tasks;

using Core = Docker.DotNet;
using CoreModels = Docker.DotNet.Models;

namespace DockerSdk.Images
{
    /// <summary>
    /// Provides methods for interacting with Docker images.
    /// </summary>
    public class ImageAccess
    {
        internal ImageAccess(DockerClient docker)
        {
            _docker = docker;
        }

        private readonly DockerClient _docker;

        /// <summary>
        /// Given an image ID, produces the short form of the image ID.
        /// </summary>
        /// <param name="id">The full ID or short ID.</param>
        /// <returns>The short image ID.</returns>
        public static string ShortenId(string id)
        {
            string result = id;

            if (result.Contains(':'))
                result = result.Split(':')[1];

            if (result.Length == 64)
                return result.Substring(0, 12);
            else if (result.Length == 12)
                return result;
            else
                throw new ArgumentException($"{id} is not a valid image ID.");
        }

        // TODO: ExportAsync (docker image save)

        /// <summary>
        /// Loads an object that can be used to interact with the indicated image.
        /// </summary>
        /// <param name="image">An image name or ID.</param>
        /// <param name="ct">A token used to cancel the operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> that resolves to the image object.</returns>
        /// <exception cref="ImageNotFoundException">No such image exists.</exception>
        /// <exception cref="System.Net.Http.HttpRequestException">
        /// The request failed due to an underlying issue such as loss of network connectivity.
        /// </exception>
        public async Task<Image> GetAsync(string image, CancellationToken ct = default)
        {
            try
            {
                CoreModels.ImageInspectResponse response = await _docker.Core.Images.InspectImageAsync(image, ct);
                return new Image(_docker, response.ID);
            }
            catch (Core.DockerImageNotFoundException ex)
            {
                throw ImageNotFoundException.Wrap(image, ex);
            }
            catch (Core.DockerApiException ex)
            {
                throw DockerException.Wrap(ex);
            }
        }

        /// <summary>
        /// Loads detailed information about an image.
        /// </summary>
        /// <param name="image">An image name or ID.</param>
        /// <param name="ct">A token used to cancel the operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> that resolves to the details.</returns>
        /// <exception cref="ImageNotFoundException">No such image exists.</exception>
        /// <exception cref="System.Net.Http.HttpRequestException">
        /// The request failed due to an underlying issue such as network connectivity.
        /// </exception>
        public Task<ImageDetails> GetDetailsAsync(string image, CancellationToken ct = default)
            => ImageDetails.LoadAsync(_docker, image, ct);

        // TODO: GetHistoryAsyn (docker image history)

        // TODO: ImportAsync

        // TODO: ListAsync

        // TODO: PullAsync

        // TODO: PushAsync

        // TODO: RemoveAsync

        // TODO: RemoveUnusedAsync (docker image prune)

        // TODO: TagAsync (docker image tag)
    }
}
