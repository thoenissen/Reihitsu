using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.ArchitectureTests.General;

/// <summary>
/// Verifies that both <c>apply-fix</c> wrapper scripts stop interpreting their own options once a literal
/// <c>--</c> has been seen, so a positional argument literally named <c>--no-install</c> reaches the runner
/// instead of being swallowed as the wrapper's own install-skip option
/// </summary>
[TestClass]
public sealed class ApplyFixWrapperPositionalArgumentTests
{
    #region Constants

    /// <summary>
    /// Diagnostic ID no shipped analyzer reports. Resolution failing on this ID proves every earlier argument,
    /// including the fixture directory, reached the runner rather than being consumed by a wrapper
    /// </summary>
    private const string UnknownDiagnosticId = "RH9999";

    #endregion // Constants

    #region Properties

    /// <summary>
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that the bash wrapper forwards a positional <c>--no-install</c> value that follows a literal
    /// <c>--</c>, instead of consuming it as the wrapper's own option and dropping the fixture directory
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    public async Task BashForwardsPositionalNoInstallAfterDoubleDash()
    {
        // Arrange
        if (FindExecutable("bash") is not { } bash)
        {
            Assert.Inconclusive("bash is not available in this environment.");

            return;
        }

        using (var scope = new ApplyFixWrapperFixtureDirectoryScope())
        {
            scope.CreateFixture("--no-install", "Sample.cs");

            // Act
            var (exitCode, _, error) = await RunAsync(bash,
                                                      [FindScript("apply-fix.sh"), UnknownDiagnosticId, "--", "--no-install"],
                                                      scope.Path);

            // Assert
            Assert.AreEqual(2, exitCode);
            Assert.Contains($"no analyzer reports '{UnknownDiagnosticId}'", error);
        }
    }

    /// <summary>
    /// Verifies that the bash wrapper still consumes <c>--no-install</c> as its own option when it appears before
    /// <c>--</c>, so the fix does not regress the option's existing behavior
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    public async Task BashStillConsumesNoInstallBeforeDoubleDash()
    {
        // Arrange
        if (FindExecutable("bash") is not { } bash)
        {
            Assert.Inconclusive("bash is not available in this environment.");

            return;
        }

        using (var scope = new ApplyFixWrapperFixtureDirectoryScope())
        {
            var directory = scope.CreateFixture("plain", "Sample.cs");

            // Act
            var (exitCode, _, error) = await RunAsync(bash,
                                                      [FindScript("apply-fix.sh"), UnknownDiagnosticId, directory, "--no-install"],
                                                      scope.Path);

            // Assert
            Assert.AreEqual(2, exitCode);
            Assert.Contains($"no analyzer reports '{UnknownDiagnosticId}'", error);
        }
    }

    /// <summary>
    /// Verifies that the PowerShell wrapper already forwards a positional <c>--no-install</c> value that follows a
    /// literal <c>--</c>. The wrapper needs no change for this case; the test is the cross-wrapper coverage owed
    /// alongside the bash fix so the two implementations stay provably in sync
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    public async Task PowerShellAlreadyForwardsPositionalNoInstallAfterDoubleDash()
    {
        // Arrange
        if (FindExecutable("pwsh") is not { } pwsh)
        {
            Assert.Inconclusive("pwsh is not available in this environment.");

            return;
        }

        using (var scope = new ApplyFixWrapperFixtureDirectoryScope())
        {
            scope.CreateFixture("--no-install", "Sample.cs");

            // Act
            var (exitCode, _, error) = await RunAsync(pwsh,
                                                      ["-NoProfile", "-File", FindScript("apply-fix.ps1"), UnknownDiagnosticId, "--", "--no-install"],
                                                      scope.Path);

            // Assert
            Assert.AreEqual(2, exitCode);
            Assert.Contains($"no analyzer reports '{UnknownDiagnosticId}'", error);
        }
    }

    /// <summary>
    /// Verifies that the PowerShell wrapper already forwards <c>-NoInstall</c> as a plain positional once it
    /// follows a literal <c>--</c>, so the runner (not the wrapper) rejects the resulting third positional
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    public async Task PowerShellAlreadyForwardsNoInstallSwitchAfterDoubleDashAsPositional()
    {
        // Arrange
        if (FindExecutable("pwsh") is not { } pwsh)
        {
            Assert.Inconclusive("pwsh is not available in this environment.");

            return;
        }

        using (var scope = new ApplyFixWrapperFixtureDirectoryScope())
        {
            var directory = scope.CreateFixture("plain", "Sample.cs");

            // Act
            var (exitCode, _, error) = await RunAsync(pwsh,
                                                      ["-NoProfile", "-File", FindScript("apply-fix.ps1"), UnknownDiagnosticId, directory, "--", "-NoInstall"],
                                                      scope.Path);

            // Assert
            Assert.AreEqual(2, exitCode);
            Assert.Contains("expected exactly one diagnostic ID and one fixture directory", error);
        }
    }

    /// <summary>
    /// Locates an executable on the current <c>PATH</c>
    /// </summary>
    /// <param name="name">Executable name without a platform-specific extension</param>
    /// <returns>The full path of the executable, or <see langword="null"/> when it cannot be found</returns>
    private static string FindExecutable(string name)
    {
        var pathVariable = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        var executableName = OperatingSystem.IsWindows() ? $"{name}.exe" : name;

        foreach (var directory in pathVariable.Split(Path.PathSeparator))
        {
            if (string.IsNullOrEmpty(directory))
            {
                continue;
            }

            var candidate = Path.Combine(directory, executableName);

            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        return null;
    }

    /// <summary>
    /// Resolves the absolute path of a repository script
    /// </summary>
    /// <param name="scriptName">Script file name under <c>scripts/</c></param>
    /// <returns>The absolute script path</returns>
    private static string FindScript(string scriptName)
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current != null)
        {
            if (File.Exists(Path.Combine(current.FullName, "Reihitsu.sln")))
            {
                return Path.Combine(current.FullName, "scripts", scriptName);
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