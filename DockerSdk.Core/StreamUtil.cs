using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DockerSdk.Core.Models
{
    internal static class StreamUtil
    {
        private static readonly JsonSerializer jsonSerializer = new();

        internal static async Task MonitorStreamAsync(Task<Stream> streamTask, Comm client, CancellationToken ct, IProgress<string> progress)
        {
            var tcs = new TaskCompletionSource<string>();

            using (var stream = await streamTask)
            using (var reader = new StreamReader(stream, new UTF8Encoding(false)))
            using (ct.Register(() => tcs.TrySetCanceled(ct)))
            {
                string line;
                while ((line = await await Task.WhenAny(reader.ReadLineAsync()!, tcs.Task)) != null)
                {
                    progress.Report(line);
                }
            }
        }

        internal static async Task MonitorStreamForMessagesAsync<T>(Task<Stream> streamTask, Comm client, CancellationToken ct, IProgress<T> progress)
        {
            var tcs = new TaskCompletionSource<bool>();

            using (var stream = await streamTask)
            using (var reader = new StreamReader(stream, new UTF8Encoding(false)))
            using (var jsonReader = new JsonTextReader(reader) { SupportMultipleContent = true })
            using (ct.Register(() => tcs.TrySetCanceled(ct)))
            {
                while (true)
                {
                    var completedTask = await Task.WhenAny(jsonReader.ReadAsync(ct), tcs.Task).ConfigureAwait(false);
                    if (!await completedTask) // ConfigureAwait not needed because the task has already resolved
                        return;

                    var ev = await jsonSerializer.DeserializeAsync<T>(jsonReader, ct);
                    progress.Report(ev!);
                }
            }
        }

        internal static async Task MonitorResponseForMessagesAsync<T>(Task<HttpResponseMessage> responseTask, Comm client, CancellationToken cancel, IProgress<T> progress)
        {
            using (var response = await responseTask)
            {
                await MonitorStreamForMessagesAsync<T>(response.Content.ReadAsStreamAsync(), client, cancel, progress);
            }
        }
    }
}
