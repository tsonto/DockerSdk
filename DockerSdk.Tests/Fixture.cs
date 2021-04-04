using System;
using System.Linq;
using Xunit;

namespace DockerSdk.Tests
{
    public sealed class Fixture : IDisposable
    {
        public Fixture()
        {
            Cli.writer = s => { Console.WriteLine(s); Console.Out.Flush(); };

            // Check that the Docker CLI is installed.
            string[] output;
            try
            {
                output = Cli.Run("docker version");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Cannot run the tests because the Docker CLI is either not installed or not in PATH.", ex);
            }

            // Make sure that the daemon is in Linux mode, since the test images are based on Linux images. This only
            // applies to Docker for Windows, which can work with Windows containers -or- Linux containers but not both
            // at the same time. Other flavors of Docker only run Linux containers, so will never encounter this
            // problem.
            var osMode = GetServerMode(output);
            if (!string.Equals(osMode, "linux", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Cannot run the tests because the Docker daemon is in Windows mode. To proceed, you must switch it to use Linux containers.");

            // Start up the test environment.
            Cli.Run("cd scripts && docker-compose up --build --detach --no-color", ignoreErrors: true);
        }

        public void Dispose()
        {
            // Shut down the test environment.
            Cli.Run("cd scripts && docker-compose down", ignoreErrors: true);
        }

        private static string GetServerMode(string[] output)
                    => output
                // There are two OS/Arch lines. We want the one that's after the Server line.
                .SkipWhile(line => !line.StartsWith("Server", StringComparison.InvariantCultureIgnoreCase))
                // Get the OS/Arch line.
                .SkipWhile(line => !line.Trim().StartsWith("OS/Arch", StringComparison.InvariantCultureIgnoreCase))
                .First()
                // Extract the OS value.
                .Split(':')[1]
                .Split('/')[0]
                .Trim();
    }

    [CollectionDefinition("Common")]
    public class FixtureCollection : ICollectionFixture<Fixture>
    {
        // This class is never instantiated. It's simply a marker class used by XUnit.
    }
}
