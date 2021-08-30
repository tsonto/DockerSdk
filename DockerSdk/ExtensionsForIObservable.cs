using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DockerSdk
{
    public static class ExtensionsForIObservable
    {
        public static IObservable<T> NotNull<T>(this IObservable<T?> input)
            => input
            .Where(x => x != null)
            .Select(x => (T)x!);
    }
}
