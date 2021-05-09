using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Xunit.Abstractions;

namespace DockerSdk.Tests
{
    public delegate void CliWriter(string s);

    /// <summary>
    /// Runs commands on in a PowerShell command line. This is meant for setting up tests via the docker CLI.
    /// </summary>
    internal static class Cli
    {
        internal static CliWriter? staticWriter = null;

        private static void Write(CliWriter? writer, string message) => writer?.Invoke(message);

        /// <summary>
        /// Runs a command in a Powershell prompt.
        /// </summary>
        /// <param name="command">The command to run.</param>
        /// <returns>An array of lines written to stdout.</returns>
        /// <exception cref="InvalidOperationException">
        /// The command either wrote to stdout or returned a non-zero exit code.
        /// </exception>
        public static string[] Run(string command)
            => Run(staticWriter, command, false);

        /// <summary>
        /// Runs a command in a Powershell prompt.
        /// </summary>
        /// <param name="command">The command to run.</param>
        /// <returns>An array of lines written to stdout.</returns>
        /// <exception cref="InvalidOperationException">
        /// The command either wrote to stdout or returned a non-zero exit code.
        /// </exception>
        public static string[] Run(CliWriter? writer, string command)
            => Run(writer, command, false);

        /// <summary>
        /// Runs a command in a Powershell prompt.
        /// </summary>
        /// <param name="command">The command to run.</param>
        /// <param name="ignoreErrors">True to ignore any errors that the command may raise.</param>
        /// <returns>An array of lines written to stdout.</returns>
        /// <exception cref="InvalidOperationException">
        /// The command either wrote to stdout or returned a non-zero exit code, and <paramref name="ignoreErrors"/> is
        /// false.
        /// </exception>
        public static string[] Run(string command, bool ignoreErrors)
            => Run(staticWriter, command, ignoreErrors);

        /// <summary>
        /// Runs a command in a Powershell prompt.
        /// </summary>
        /// <param name="command">The command to run.</param>
        /// <param name="ignoreErrors">True to ignore any errors that the command may raise.</param>
        /// <returns>An array of lines written to stdout.</returns>
        /// <exception cref="InvalidOperationException">
        /// The command either wrote to stdout or returned a non-zero exit code, and <paramref name="ignoreErrors"/> is
        /// false.
        /// </exception>
        public static string[] Run(CliWriter? writer, string command, bool ignoreErrors)
        {
            // Get the name of the PowerShell executable, which depends on the platform.
            string powershellCommand
                = Environment.OSVersion.Platform == PlatformID.Win32NT
                ? "pwsh.exe"
                : "pwsh";

            // Declare how to start the process. The trailing - in the parameters means to read the command from stdin.
            var pi = new ProcessStartInfo(powershellCommand, "-Command -")
            {
                CreateNoWindow = true,
                UseShellExecute = false,  // needed for .Net Framework, which defaults it to true
                RedirectStandardInput = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
            };

            // Start the process.
            Process? process;
            try
            {
                process = Process.Start(pi);
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                throw new InvalidOperationException($"Failed to start PowerShell. Is it installed and in the PATH? (Command: {pi.FileName} {pi.Arguments})", ex);
            }
            if (process is null)
            {
                throw new InvalidOperationException($"Failed to start PowerShell. Is it installed and in the PATH? (Command: {pi.FileName} {pi.Arguments})");
            }

            using (process)
            {
                // Feed the command to PowerShell via stdin.
                Write(writer, "testcli << " + command);
                process.StandardInput.WriteLine(command);

                // Shut down the process.
                process.StandardInput.WriteLine("exit");
                process.WaitForExit();

                // Throw an exception if errors were detected. (Unless supressed.)
                if (!ignoreErrors)
                {
                    var errors = process.StandardError.ReadToEnd();
                    if (!string.IsNullOrEmpty(errors))
                        throw new InvalidOperationException(errors);

                    if (process.ExitCode != 0)
                        throw new InvalidOperationException($"The process exited with code {process.ExitCode}.");
                }

                // Get the text that the process wrote to stdout.
                var output = process.StandardOutput.ReadToEnd()
                    .Split('\n')
                    .Select(line => line.Trim())
                    .ToArray();

                // Write the input and outputs to the console so it will appear in the CI/CD pipeline.
                foreach (var line in output)
                    Write(writer, $"testcli >> {line}");
                if (process.ExitCode != 0)
                    Write(writer, $"testcli exited {process.ExitCode}");

                return output;
            }
        }
    }
}
