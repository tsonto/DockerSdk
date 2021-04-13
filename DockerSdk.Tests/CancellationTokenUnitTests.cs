using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace DockerSdk.Tests
{
    public class CancellationTokenUnitTests
    {
        [Fact]
        public void WithTimeout_TimeElapsed_IsActivated()
        {
            using var cts = new CancellationTokenSource();
            var input = cts.Token;

            var output = input.WithTimeout(TimeSpan.FromMilliseconds(1));

            Thread.Sleep(100);

            output.IsCancellationRequested.Should().BeTrue();
        }

        [Fact]
        public void WithTimeout_OriginalIsCancelled_IsActivated()
        {
            using var cts = new CancellationTokenSource();
            var input = cts.Token;

            var output = input.WithTimeout(TimeSpan.FromDays(1));

            cts.Cancel();

            Thread.Sleep(100); // it might take a moment for the cancellation to propagate

            output.IsCancellationRequested.Should().BeTrue();
        }
    }
}
