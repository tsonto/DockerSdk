using System;
using System.Linq;
using System.Reactive.Linq;

namespace DockerSdk
{
    /// <summary>
    /// Contains extension methods for the <see cref="IObservable{T}"/> class.
    /// </summary>
    public static class ExtensionsForIObservable
    {
        /// <summary>
        /// Converts a sequence of nullable objects to a sequence of non-nullable objects, dropping all nulls found
        /// along the way.
        /// </summary>
        /// <typeparam name="T">The (non-nullable) element type.</typeparam>
        /// <param name="input">The sequence source to observe.</param>
        /// <returns>A modified observable sequence.</returns>
        public static IObservable<T> NotNull<T>(this IObservable<T?> input)
            => input
            .Where(x => x != null)
            .Select(x => (T)x!);
    }
}
