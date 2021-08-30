using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DockerSdk.Containers.Events;
using DockerSdk.Images.Events;
using DockerSdk.Networks.Events;
using Message = DockerSdk.Core.Models.Message;

namespace DockerSdk.Events
{
    /// <summary>
    /// Represents a Docker event.
    /// </summary>
    /// <remarks>If you need a less-processed form, cast the object to <see cref="IEventLowLevel"/>.</remarks>
    public abstract class Event : IEventLowLevel
    {
        internal Event(Message message, EventSubjectType type)
        {
            SubjectType = type;
            Timestamp = MakeTimestamp(message.Time, message.TimeNano);
            raw = message;
            delivery = new TaskCompletionSource();
        }

        /// <summary>
        /// Gets a value indicating the type of event this is.
        /// </summary>
        string IEventLowLevel.Action => raw.Action;

        /// <inheritdoc/>
        IReadOnlyDictionary<string, string> IEventLowLevel.ActorDetails => raw.Actor.Attributes.ToImmutableDictionary();

        /// <inheritdoc/>
        string IEventLowLevel.ActorId => raw.Actor.ID;

        /// <inheritdoc/>
        public EventSubjectType SubjectType { get; }

        /// <summary>
        /// Gets the date and time at which the event occurred, in UTC, per the daemon's clock.
        /// </summary>
        /// <remarks>
        /// This timestamp comes from the Docker daemon, and is based on the daemon's server's clock. Unless the caller
        /// and the daemon are on the same computer, their clocks might not be in sync. Attempting to compute elapsed
        /// time based on different clocks can cause meaningless values, including negative durations. Note that even
        /// two computers that synchronize to the same <a href="https://en.wikipedia.org/wiki/Time_server">time
        /// server</a> can have a small amount of clock drift.
        /// </remarks>
        public DateTimeOffset Timestamp { get; }

        // internal Task Delivered => delivery.Task;

        private readonly TaskCompletionSource delivery;
        private readonly Message raw;

        /// <summary>
        /// Given a message from the underlying API, produce a strongly-typed subclass of <see cref="Event"/>.
        /// </summary>
        /// <param name="message">The message to wrap.</param>
        /// <returns>The resulant event, or null if the type of subject/event wasn't recognized.</returns>
        internal static Event? Wrap(Message message)
            => message.Type switch
            {
                "container" => ContainerEvent.Wrap(message),
                "network" => NetworkEvent.Wrap(message),
                "image" => ImageEvent.Wrap(message),
                _ => null
            };

        // internal void MarkDelivered() => delivery.TrySetResult();

        private static DateTimeOffset MakeTimestamp(long seconds, long nanoseconds)
            => DateTimeOffset.FromUnixTimeSeconds(seconds).AddSeconds(nanoseconds / 1e9);
    }
}
