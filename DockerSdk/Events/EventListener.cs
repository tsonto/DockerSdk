using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using DockerSdk.Core.Models;
using Message = DockerSdk.Core.Models.Message;

namespace DockerSdk.Events
{
    internal sealed class EventListener : ObservableBase<Event>, IDisposable
    {
        public EventListener(DockerClient docker)
        {
            cts = new CancellationTokenSource();

            // Start the listener in the background, since it does not resolve until it's canceled.
            var progress = new Progress<Message>(Send);
            _ = Task.Run(async () =>
            {
                //docker.Comm.System.MonitorEventsAsync(new(), progress, cts.Token);
                var streamTask = docker.Comm.StartStreamAsync(HttpMethod.Get, "events", ct: cts.Token);
                await StreamUtil.MonitorStreamForMessagesAsync(
                    streamTask,
                    docker.Comm,
                    cts.Token,
                    progress).ConfigureAwait(false);
            });

        }

        private readonly CancellationTokenSource cts;

        private readonly ConcurrentDictionary<RunOnDispose, IObserver<Event>> observers = new();

        public void Dispose()
        {
            // Stop the monitor.
            cts.Cancel();
        }

        /// <inheritdoc/>
        protected override IDisposable SubscribeCore(IObserver<Event> observer)
        {
            var disposer = new RunOnDispose();
            observers.TryAdd(disposer, observer);
            disposer.Action = () => observers.TryRemove(disposer, out _);
            return disposer;
        }

        private void Send(Message message)
        {
            // Try to convert the message into an event. If we don't recognize it, we just drop it.
            var @event = Event.Wrap(message);
            if (@event is null)
                return;

            // Notify observers.
            foreach (var observer in observers.Values)
                observer.OnNext(@event);

            // Now that observers have been notified, mark the message as delivered.
            @event.MarkDelivered();

            //// Now that observers have been notified, evaluate the watches. They will automatically remove themselves
            //// when they're satisfied or canceled.
            //foreach (var watch in watches.Values)
            //    watch.Test(@event);
        }

        private sealed record RunOnDispose : IDisposable
        {
            public Action? Action { get; set; }
            public void Dispose() => Action?.Invoke();
        }
    }
}
