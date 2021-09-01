using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using DockerSdk.Events;
using FluentAssertions;
using Xunit;

namespace DockerSdk.Tests
{
    public class DamTests
    {
        [Fact]
        public void BuffersNext()
        {
            using var subj = new Subject<int>();
            var list = new List<int>();
            using var sub = subj.Dam(out Action open).Subscribe(i => list.Add(i));

            subj.OnNext(3);
            list.Should().BeEmpty();
            open();
            list.Should().BeEquivalentTo(new[] { 3 });
            subj.OnNext(5);
            list.Should().BeEquivalentTo(new[] { 3, 5 });
        }

        [Fact]
        public void BuffersException()
        {
            using var subj = new Subject<int>();
            var hasError = false;
            using var sub = subj.Dam(out Action open).Subscribe(i => { }, ex =>hasError = true);

            subj.OnError(new InvalidOperationException());
            hasError.Should().BeFalse();
            open();
            hasError.Should().BeTrue();
        }

        [Fact]
        public void Opened_DoesNotBufferException()
        {
            using var subj = new Subject<int>();
            var hasError = false;
            using var sub = subj.Dam(out Action open).Subscribe(i => { }, ex => hasError = true);

            open();
            subj.OnError(new InvalidOperationException());
            hasError.Should().BeTrue();
        }

        [Fact]
        public void BuffersCompletion()
        {
            using var subj = new Subject<int>();
            var isComplete = false;
            using var sub = subj.Dam(out Action open).Subscribe(_ => { }, _ => { }, () => isComplete = true);

            subj.OnCompleted();
            isComplete.Should().BeFalse();
            open();
            isComplete.Should().BeTrue();
        }

        [Fact]
        public void Opened_DoesNotBufferCompletion()
        {
            using var subj = new Subject<int>();
            var isComplete = false;
            using var sub = subj.Dam(out Action open).Subscribe(_ => { }, _ => { }, () => isComplete = true);

            open();
            subj.OnCompleted();
            isComplete.Should().BeTrue();
        }
    }
}
