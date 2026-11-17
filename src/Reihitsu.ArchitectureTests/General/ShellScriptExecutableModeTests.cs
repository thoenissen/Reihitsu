using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.ArchitectureTests.General;

/// <summary>
/// Guards that every tracked shell script is committed with the executable bit, so the documented
/// <c>scripts/&lt;name&gt;.sh</c> invocation works without a <c>bash</c> prefix on Linux and macOS
/// </summary>
[TestClass]
public sealed class ShellScriptExecutableModeTests
{
    #region Constants

    /// <summary>
    /// Git index mode of a regular file with the executable bit
    /// </summary>
    private const string ExecutableMode = "100755";

    #endregion // Constants

    #region Properties

    /// <summary>
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that the git index records every tracked <c>*.sh</c> file with mode <c>100755</c>
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    public async Task EveryTrackedShellScriptIsExecutable()
    {
        // Arrange
        var repositoryRoot = FindRepositoryRoot();

        if (FindExecutable("git") is not { } git)
        {
            Assert.Inconclusive("git is not available in this environment.");

            return;
        }

        // Act
        var (exitCode, output, error) = await RunAsync(git, ["ls-files", "--stage", "--", "*.sh"], repositoryRoot);

        if (exitCode != 0)
        {
            Assert.Inconclusive($"The repository root is not a git work tree: {error}");

            return;
        }

        var entries = ParseStageEntries(output);
        var notExecutable = entries.Where(entry => entry.Mode != ExecutableMode)
                                   .Select(entry => $"{entry.Path} ({entry.Mode})")
                                   .ToList();

        // Assert
        Assert.Contains(entry => entry.Path == "scripts/trace.sh", entries, "The guard found no tracked shell script it is meant to cover.");
        Assert.IsEmpty(notExecutable, $"Tracked shell scripts without the executable bit; run 'git update-index --chmod=+x <path>': {string.Join(", ", notExecutable)}");
    }

    /// <summary>
    /// Parses the output of <c>git ls-files --stage</c> into mode and path pairs
    /// </summary>
    /// <param name="output">Output of the command, one entry per line</param>
    /// <returns>The mode and path of every entry</returns>
    private static List<(string Mode, string Path)> ParseStageEntries(string output)
    {
        var entries = new List<(string Mode, string Path)>();

        foreach (var line in output.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var tabIndex = line.IndexOf('\t');
            var mode = line.Substring(0, line.IndexOf(' '));

            entries.Add((mode, line.Substring(tabIndex + 1)));
        }

        return entries;
    }

    /// <summary>
    /// Finds an executable on the <c>PATH</c>
    /// </summary>
    /// <param name="name">Executable name without extension</param>
    /// <returns>The full path of the executable, or <see langword="null"/> when it is not on the <c>PATH</c></returns>
    private static string FindExecutable(string name)
    {
        var path = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        var candidates = OperatingSystem.IsWindows()
                             ? new[] { name + ".exe", name + ".cmd" }
                             : new[] { name };

        foreach (var directory in path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            foreach (var candidate in candidates)
            {
                var fullPath = Path.Combine(directory, candidate);

                if (File.Exists(fullPath))
                {
                    return fullPath;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Locates the repository root by walking up from the test assembly
    /// </summary>
    /// <returns>The repository root directory</returns>
    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current != null)
        {
            if (File.Exists(Path.Combine(current.FullName, "Reihitsu.sln")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate the Reihitsu repository root.");
    }

    /// <summary>
    /// Runs an executable with the provided arguments and working directory, capturing its exit code and streams
    /// </summary>
    /// <param name="fileName">Executable to run</param>
    /// <param name="arguments">Arguments passed to the executable</param>
    /// <param name="workingDirectory">Working directory the executable is started in</param>
    /// <returns>The exit code and the captured output and error text</returns>
    private async Task<(int ExitCode, string Output, string Error)> RunAsync(string fileName, string[] arguments, string workingDirectory)
    {
        var startInfo = new ProcessStartInfo(fileName)
                        {
                            WorkingDirectory = workingDirectory,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false
                        };

        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using (var process = new Process
                             {
                                 StartInfo = startInfo
                             })
        {
            process.Start();

            var outputTask = process.StandardOutput.ReadToEndAsync(TestContext.CancellationToken);
            var errorTask = process.StandardError.ReadToEndAsync(TestContext.CancellationToken);

            await process.WaitForExitAsync(TestContext.CancellationToken);

            return (process.ExitCode, await outputTask, await errorTask);
        }
    }

    #endregion // Methods
}