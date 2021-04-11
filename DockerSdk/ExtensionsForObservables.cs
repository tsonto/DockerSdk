using System;
using DockerSdk.Events;

namespace DockerSdk
{
    internal static class ExtensionsForObservables
    {
        public static IObservable<T> Dam<T>(this IObservable<T> input, out Action open)
        {
            var subject = new Dam<T>(input);
            open = () => subject.Open();
            return subject;
        }
    }
}
