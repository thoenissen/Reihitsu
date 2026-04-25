using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.Analyzer.Test.SelfHosting;

/// <summary>
/// Self-hosting tests that run a solution build and watch for selected analyzer diagnostics in repository sources.
/// </summary>
[TestClass]
[DoNotParallelize]
[Ignore("Enable this test once the repository sources are analyzer-clean for the selected diagnostics.")]
public class SelfHostingTests
{
    #region Constants

    /// <summary>
    /// Diagnostic IDs monitored by the self-hosting build.
    /// </summary>
    private static readonly string[] SelfHostedDiagnosticIds = [
                                                                   "RH0334",
                                                                   "RH0335",
                                                                   "RH0337",
                                                                   "RH0338",
                                                                   "RH0342",
                                                                   "RH0343",
                                                                   "RH0350",
                                                                   "RH0351",
                                                                   "RH0358",
                                                                   "RH0361",
                                                                   "RH0363",
                                                                   "RH0365",
                                                                   "RH0366",
                                                                   "RH0369",
                                                                   "RH0370",
                                                                   "RH0374",
                                                                   "RH0376",
                                                                   "RH0445",
                                                                   "RH0608",
                                                               ];

    /// <summary>
    /// Pattern used to extract monitored diagnostics from build output.
    /// </summary>
    private static readonly Regex DiagnosticRegex = new($@"\b({string.Join("|", SelfHostedDiagnosticIds)})\b",
                                                        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    #endregion // Constants

    #region Properties

    /// <summary>
    /// Gets or sets the test context for the current test.
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that repository sources do not trigger the selected analyzers when the solution is built.
    /// </summary>
    [TestMethod]
    public void RepositorySourcesDoNotTriggerSelectedAnalyzers()
    {
        var solutionRoot = FindSolutionRoot();
        var buildOutput = RunBuild(solutionRoot);

        TestContext?.WriteLine(buildOutput);

        var matchingDiagnostics = ExtractMatchingDiagnostics(buildOutput);

        if (matchingDiagnostics.Count > 0)
        {
            Assert.Fail($"Self-hosting build reported {matchingDiagnostics.Count} selected analyzer diagnostic(s):{Environment.NewLine}{string.Join(Environment.NewLine, matchingDiagnostics)}");
        }
    }

    /// <summary>
    /// Extracts build output lines that mention one of the selected diagnostics.
    /// </summary>
    /// <param name="buildOutput">Captured build output</param>
    /// <returns>The matching output lines</returns>
    private static List<string> ExtractMatchingDiagnostics(string buildOutput)
    {
        var matchingDiagnostics = new List<string>();
        var lines = buildOutput.Split(["\r\n", "\n"], StringSplitOptions.None);

        foreach (var line in lines)
        {
            if (DiagnosticRegex.IsMatch(line))
            {
                matchingDiagnostics.Add(line);
            }
        }

        return matchingDiagnostics;
    }

    /// <summary>
    /// Finds the solution root directory by walking up from the test assembly's location.
    /// </summary>
    /// <returns>The absolute path to the solution root directory.</returns>
    private static string FindSolutionRoot()
    {
        var directory = AppContext.BaseDirectory;

        while (directory != null)
        {
            if (File.Exists(Path.Combine(directory, "Reihitsu.sln")))
            {
                return directory;
            }

            directory = Directory.GetParent(directory)?.FullName;
        }

        throw new InvalidOperationException("Could not find solution root (Reihitsu.sln).");
    }

    /// <summary>
    /// Runs a solution build and captures its output.
    /// </summary>
    /// <param name="solutionRoot">The solution root directory.</param>
    /// <returns>The captured output.</returns>
    private static string RunBuild(string solutionRoot)
    {
        using var process = new Process
                            {
                                StartInfo = new ProcessStartInfo
                                            {
                                                FileName = "dotnet",
                                                Arguments = "build Reihitsu.sln -c Release --no-restore --verbosity minimal",
                                                WorkingDirectory = solutionRoot,
                                                RedirectStandardOutput = true,
                                                RedirectStandardError = true,
                                                UseShellExecute = false,
                                                CreateNoWindow = true,
                                            },
                            };

        process.Start();

        var standardOutput = process.StandardOutput.ReadToEnd();
        var standardError = process.StandardError.ReadToEnd();

        process.WaitForExit();

        return standardOutput + Environment.NewLine + standardError;
    }

    #endregion // Methods
}