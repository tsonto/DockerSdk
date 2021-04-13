using System;
using System.Collections.Concurrent;
using System.Reactive;
using System.Reactive.Disposables;

namespace DockerSdk.Events
{
    internal sealed class Dam<T> : ObservableBase<T>, IDisposable
    {
        public Dam(IObservable<T> input)
        {
            subscription = input.Subscribe(OnNext, OnError, OnCompleted);
        }

        private readonly ConcurrentQueue<T> elements = new();
        private readonly ConcurrentDictionary<IObserver<T>, IObserver<T>> observers = new();

        private readonly IDisposable subscription;
        private Exception? exception;
        private bool isComplete;
        private bool open;

        public void Dispose()
        {
            subscription.Dispose();
        }

        public void Open()
        {
            lock (elements)
            {
                if (open)
                    return;

                while (elements.TryDequeue(out T? element))
                    EmitNext(element);

                if (exception is not null)
                    EmitError(exception);
                else if (isComplete)
                    EmitCompleted();

                open = true;
            }
        }

        protected override IDisposable SubscribeCore(IObserver<T> observer)
        {
            observers.TryAdd(observer, observer);
            return Disposable.Create(() =>
            {
                observers.TryRemove(observer, out _);
            });
        }

        private void EmitCompleted()
        {
            foreach (var observer in observers.Values)
                observer.OnCompleted();
        }

        private void EmitError(Exception error)
        {
            foreach (var observer in observers.Values)
                observer.OnError(error);
        }

        private void EmitNext(T element)
        {
            foreach (var observer in observers.Values)
                observer.OnNext(element);
        }

        private void OnCompleted()
        {
            lock (elements)
            {
                if (open)
                    EmitCompleted();
                else
                    isComplete = true;
            }
        }

        private void OnError(Exception error)
        {
            lock (elements)
            {
                if (open)
                    EmitError(error);
                else
                    exception = error;
            }
        }

        private void OnNext(T value)
        {
            lock (elements)
            {
                if (open)
                    EmitNext(value);
                else
                    elements.Enqueue(value);
            }
        }
    }
}
