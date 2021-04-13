using System;
using System.Collections.Generic;

namespace DockerSdk.Events
{
    /// <summary>
    /// Represents an event emitted by the Docker daemon. This is similar to <see cref="Event"/>, but in a less-processed form.
    /// </summary>
    public interface IEventLowLevel
    {
        /// <summary>
        /// Gets a value indicating what type of thing the event is about.
        /// </summary>
        public EventSubjectType SubjectType { get; }

        /// <summary>
        /// Gets the full ID of the event's subject.
        /// </summary>
        public string ActorId { get; }

        /// <summary>
        /// Gets a value indicating the type of event this is.
        /// </summary>
        public string Action { get; }

        /// <summary>
        /// Gets a dictionary of supplementary information provided by the daemon about the event's subject. The keys
        /// that appear here vary by subject type, event type, and the specifics of the subject.
        /// </summary>
        public IReadOnlyDictionary<string, string> ActorDetails { get; }

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
    }
}
