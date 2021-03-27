using System;
using Xunit;

namespace DockerSdk.Tests
{
    public sealed class Fixture : IDisposable
    {
        public Fixture()
        {
            // Check that the Docker CLI is installed.
            try
            {
                Cli.Run("docker version");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Cannot run the tests because the Docker CLI is either not installed or not in PATH.", ex);
            }

            // Start up the test environment.
            Cli.Run("cd scripts && docker-compose up --build --detach --no-color", ignoreErrors: true);
        }

        public void Dispose()
        {
            // Shut down the test environment.
            Cli.Run("cd scripts && docker-compose down", ignoreErrors: true);
        }
    }

    [CollectionDefinition("Common")]
    public class FixtureCollection : ICollectionFixture<Fixture>
    {
        // This class is never instantiated. It's simply a marker class used by XUnit.
    }
}
