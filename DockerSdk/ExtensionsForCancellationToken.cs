using System;
using System.Threading;
using System.Threading.Tasks;

namespace DockerSdk
{
    /// <summary>
    /// Holds extension methods for <see cref="CancellationToken"/> objects.
    /// </summary>
    public static class ExtensionsForCancellationToken
    {
        /// <summary>
        /// Creates a wrapper for the given cancellation token that activates after a specified amount of time has passed.
        /// </summary>
        /// <param name="original">The cancellation token to wrap.</param>
        /// <param name="duration">How long to wait.</param>
        /// <returns>A new cancellation token.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The duration is zero or negative.</exception>
        public static CancellationToken WithTimeout(this CancellationToken original, TimeSpan duration)
        {
            if (duration <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(duration), "Non-positive timeouts are not allowed.");
            if (duration == Timeout.InfiniteTimeSpan)
                return original;

            var tcs = new CancellationTokenSource();

            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(duration, original);
                }
                catch (TaskCanceledException) { }
                catch (ObjectDisposedException) { }
                tcs.Cancel();
                tcs.Dispose();
            });

            return tcs.Token;
        }
    }
}
