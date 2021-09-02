using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DockerSdk.Core;
using DockerSdk.Core.Models;
using Message = DockerSdk.Core.Models.Message;

namespace DockerSdk.Events
{
    internal sealed class EventListener : IDisposable
    {
        public EventListener(Comm comm)
        {
            this.comm = comm;
        }

        internal async Task StartAsync(CancellationToken ct)
        {
            var observable = await comm.Build(HttpMethod.Get, "events")
                .AcceptStatus(HttpStatusCode.OK)
                .SendAndStreamResults<Message>(ct).ConfigureAwait(false);

            var connectable = observable
                .ObserveOn(ThreadPoolScheduler.Instance)
                .Select(message => Event.Wrap(message))
                .NotNull()
                .Publish();

            eventDisposable = connectable.Connect(); // make it a hot observable
            eventObservable = connectable;
        }

        private readonly Comm comm;
        private IDisposable? eventDisposable;
        private IObservable<Event>? eventObservable;

        public void Dispose()
        {
            eventDisposable?.Dispose();
        }

        public IDisposable Subscribe(IObserver<Event> observer)
        {
            if (eventObservable == null)
                throw new InvalidOperationException("Must call StartAsync before subscribing.");

            return eventObservable.Subscribe(observer);
        }
    }
}
