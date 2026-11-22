using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Cli.Test.Helpers;

namespace Reihitsu.Cli.Test.EndToEnd;

/// <summary>
/// End-to-end tests for the language version the CLI parses files with (CLI-004): C# 15 is the newest supported version, so a
/// file that declares a union is formatted by default and skipped with syntax errors under <c>--lang-version 14</c>, and a type
/// named <c>union</c> or <c>closed</c> in a position C# 15 reads as a keyword is skipped by default and formatted under
/// <c>--lang-version 14</c>
/// </summary>
[TestClass]
[DoNotParallelize]
[TestCategory("E2E")]
public class CliLanguageVersionEndToEndTests
{
    #region Constants

    /// <summary>
    /// The union file of the change that makes C# 15 the newest supported version
    /// </summary>
    private const string UnionSource = "public union Pet(Cat, Dog)\n{\n    public int Legs() => 4;\n}\n\npublic class Cat { }\n\npublic class Dog { }";

    /// <summary>
    /// The formatted form of <see cref="UnionSource"/> under C# 15
    /// </summary>
    private const string UnionFormatted = "public union Pet(Cat, Dog)\n{\n    public int Legs()\n    {\n        return 4;\n    }\n}\n\npublic class Cat;\n\npublic class Dog;";

    /// <summary>
    /// The union declaration of the CLI-004 example
    /// </summary>
    private const string UnionOneLineSource = "public union Pet(Cat, Dog);";

    /// <summary>
    /// A union whose braces stand on one line around a member that gets a block body
    /// </summary>
    private const string UnionWithOneLineBracesSource = "public class Outer\n{\n    private union Inner(int,string) { public override string ToString() { return \"x\"; } }\n}";

    /// <summary>
    /// The formatted form of <see cref="UnionWithOneLineBracesSource"/> under C# 15: the union's braces and member start their
    /// own lines, as a class's do
    /// </summary>
    private const string UnionWithOneLineBracesFormatted = "public class Outer\n{\n    private union Inner(int, string)\n    {\n        public override string ToString()\n        {\n            return \"x\";\n        }\n    }\n}";

    /// <summary>
    /// A type named <c>union</c> used as a method's return type, which C# 15 reads as a union declaration
    /// </summary>
    private const string TypeNamedUnionSource = "class union { }\nclass C { union M(int a) { return null; } }";

    /// <summary>
    /// The formatted form of <see cref="TypeNamedUnionSource"/> under C# 14
    /// </summary>
    private const string TypeNamedUnionFormatted = "class union;\nclass C\n{\n    union M(int a)\n    {\n        return null;\n    }\n}";

    /// <summary>
    /// A type named <c>closed</c> used as a field type and a method's return type, which C# 15 reads as a modifier
    /// </summary>
    private const string TypeNamedClosedSource = "class closed { }\nclass C { closed F; closed M() => null; }";

    /// <summary>
    /// The formatted form of <see cref="TypeNamedClosedSource"/> under C# 14
    /// </summary>
    private const string TypeNamedClosedFormatted = "class closed;\nclass C\n{\n    closed F;\n    closed M()\n    {\n        return null;\n    }\n}";

    /// <summary>
    /// Top-level statements where one statement starts with a local named <c>union</c>, which C# 15 reads as the start of a
    /// union declaration
    /// </summary>
    private const string TopLevelUnionLocalSource = "using System.Linq;\nvar a = new[] { 1,2 };\nvar union = a.Union(new[] { 3 }).ToList();\nunion.Add(4);\nSystem.Console.WriteLine(union.Count);";

    /// <summary>
    /// The formatted form of <see cref="TopLevelUnionLocalSource"/> under C# 14
    /// </summary>
    private const string TopLevelUnionLocalFormatted = "using System.Linq;\nvar a = new[] { 1, 2 };\nvar union = a.Union(new[] { 3 }).ToList();\nunion.Add(4);\nSystem.Console.WriteLine(union.Count);";

    /// <summary>
    /// The warning for <c>--lang-version preview</c>
    /// </summary>
    private const string PreviewWarning = "Warning: C# language version 'preview' is newer than the newest supported version 15.0; formatting for 15.0.";

    /// <summary>
    /// The summary line for one file skipped with syntax errors
    /// </summary>
    private const string SyntaxErrorSummaryLine = "Skipped 1 file(s) with syntax errors.";

    #endregion // Constants

    #region Properties

    /// <summary>
    /// Test context
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that the help names C# 15 as the version <c>latest</c> selects
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    public async Task HelpNamesCSharp15AsTheVersionLatestSelects()
    {
        var (exitCode, output, _) = await Run(["--help"]);

        Assert.AreEqual(ExitCodes.Success, exitCode);
        Assert.Contains("latest selects 15.0, the newest supported version;", output);
    }

    /// <summary>
    /// Verifies that a file declaring a union is formatted without <c>--lang-version</c>, and that a check of the result finds
    /// nothing to format
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [Requirement("CLI-004")]
    public async Task UnionIsFormattedWithoutLangVersion()
    {
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            var filePath = tempDir.CreateFile("Pet.cs", UnionSource);

            var (exitCode, output, error) = await Run([filePath]);
            var (checkExitCode, checkOutput, _) = await Run(["--check", filePath]);

            Assert.AreEqual(ExitCodes.Success, exitCode);
            Assert.AreEqual(string.Empty, error);
            Assert.AreEqual(Join($"Formatted: {filePath}", string.Empty, "Formatted 1 of 1 file(s)."), output);
            Assert.AreEqual(UnionFormatted, await File.ReadAllTextAsync(filePath, TestContext.CancellationToken));
            Assert.AreEqual(ExitCodes.Success, checkExitCode);
            Assert.AreEqual(Join(string.Empty, "0 of 1 file(s) need formatting."), checkOutput);
        }
    }

    /// <summary>
    /// Verifies that a file declaring a union with CRLF line endings is formatted without <c>--lang-version</c> and keeps its line
    /// endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [Requirement("CLI-004")]
    public async Task UnionWithCrlfIsFormattedWithoutLangVersion()
    {
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            var filePath = tempDir.CreateFile("Pet.cs", UnionSource.Replace("\n", "\r\n", StringComparison.Ordinal));

            var (exitCode, _, error) = await Run([filePath]);
            var (checkExitCode, _, _) = await Run(["--check", filePath]);

            Assert.AreEqual(ExitCodes.Success, exitCode);
            Assert.AreEqual(string.Empty, error);
            Assert.AreEqual(UnionFormatted.Replace("\n", "\r\n", StringComparison.Ordinal), await File.ReadAllTextAsync(filePath, TestContext.CancellationToken));
            Assert.AreEqual(ExitCodes.Success, checkExitCode);
        }
    }

    /// <summary>
    /// Verifies that <c>--lang-version 15</c> formats a file declaring a union without a warning
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [Requirement("CLI-004")]
    public async Task LangVersion15FormatsUnionWithoutWarning()
    {
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            var filePath = tempDir.CreateFile("Pet.cs", UnionSource);

            var (exitCode, output, error) = await Run(["--lang-version", "15", filePath]);

            Assert.AreEqual(ExitCodes.Success, exitCode);
            Assert.AreEqual(string.Empty, error);
            Assert.AreEqual(Join($"Formatted: {filePath}", string.Empty, "Formatted 1 of 1 file(s)."), output);
            Assert.AreEqual(UnionFormatted, await File.ReadAllTextAsync(filePath, TestContext.CancellationToken));
        }
    }

    /// <summary>
    /// Verifies that <c>--lang-version 15.0</c> formats a file declaring a union without a warning
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [Requirement("CLI-004")]
    public async Task LangVersion15Point0FormatsUnionWithoutWarning()
    {
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            var filePath = tempDir.CreateFile("Pet.cs", UnionSource);

            var (exitCode, output, error) = await Run(["--lang-version", "15.0", filePath]);

            Assert.AreEqual(ExitCodes.Success, exitCode);
            Assert.AreEqual(string.Empty, error);
            Assert.AreEqual(Join($"Formatted: {filePath}", string.Empty, "Formatted 1 of 1 file(s)."), output);
            Assert.AreEqual(UnionFormatted, await File.ReadAllTextAsync(filePath, TestContext.CancellationToken));
        }
    }

    /// <summary>
    /// Verifies that <c>--lang-version preview</c> writes the one warning naming 15.0 and formats a file declaring a union for
    /// C# 15
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [Requirement("CLI-004")]
    public async Task LangVersionPreviewWarnsWithCSharp15AndFormatsUnion()
    {
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            var filePath = tempDir.CreateFile("Pet.cs", UnionSource);

            var (exitCode, output, error) = await Run(["--lang-version", "preview", filePath]);

            Assert.AreEqual(ExitCodes.Success, exitCode);
            Assert.AreEqual(Join(PreviewWarning), error);
            Assert.AreEqual(Join($"Formatted: {filePath}", string.Empty, "Formatted 1 of 1 file(s)."), output);
            Assert.AreEqual(UnionFormatted, await File.ReadAllTextAsync(filePath, TestContext.CancellationToken));
        }
    }

    /// <summary>
    /// Verifies the CLI-004 example: a check of the file <c>public union Pet(Cat, Dog);</c> without <c>--lang-version</c> parses it
    /// without a syntax error
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [Requirement("CLI-004")]
    public async Task OneLineUnionHasNoSyntaxErrorWithoutLangVersion()
    {
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            var filePath = tempDir.CreateFile("Pet.cs", UnionOneLineSource);

            var (_, output, error) = await Run(["--check", "--verbose", filePath]);

            Assert.AreEqual(string.Empty, error);
            Assert.DoesNotContain("syntax errors", output);
        }
    }

    /// <summary>
    /// Verifies the CLI-004 example: under <c>--lang-version 14</c> the file <c>public union Pet(Cat, Dog);</c> has syntax errors,
    /// is skipped, and does not affect the exit code
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [Requirement("CLI-004")]
    public async Task OneLineUnionIsSkippedUnderLangVersion14()
    {
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            var filePath = tempDir.CreateFile("Pet.cs", UnionOneLineSource);

            var (exitCode, output, error) = await Run(["--check", "--lang-version", "14", filePath]);

            Assert.AreEqual(ExitCodes.Success, exitCode);
            Assert.AreEqual(string.Empty, error);
            Assert.AreEqual(Join(string.Empty, "0 of 1 file(s) need formatting.", SyntaxErrorSummaryLine), output);
            Assert.AreEqual(UnionOneLineSource, await File.ReadAllTextAsync(filePath, TestContext.CancellationToken));
        }
    }

    /// <summary>
    /// Verifies that under <c>--lang-version 14</c> a file declaring a union is skipped with syntax errors and left unchanged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [Requirement("CLI-004")]
    public async Task UnionIsSkippedUnderLangVersion14()
    {
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            var filePath = tempDir.CreateFile("Pet.cs", UnionSource);

            var (exitCode, output, error) = await Run(["--lang-version", "14", filePath]);

            Assert.AreEqual(ExitCodes.Success, exitCode);
            Assert.AreEqual(string.Empty, error);
            Assert.AreEqual(Join(string.Empty, "Formatted 0 of 1 file(s).", SyntaxErrorSummaryLine), output);
            Assert.AreEqual(UnionSource, await File.ReadAllTextAsync(filePath, TestContext.CancellationToken));
        }
    }

    /// <summary>
    /// Verifies that a union whose braces stand on one line gets its braces and member on their own lines without
    /// <c>--lang-version</c>, also when its member gets a block body, and that a check of the result finds nothing to format
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [Requirement("FMT-BREAK-002")]
    [Requirement("FMT-BREAK-007")]
    public async Task UnionWithOneLineBracesIsLaidOutWithoutLangVersion()
    {
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            var filePath = tempDir.CreateFile("Outer.cs", UnionWithOneLineBracesSource);

            var (exitCode, _, error) = await Run([filePath]);
            var (checkExitCode, _, _) = await Run(["--check", filePath]);

            Assert.AreEqual(ExitCodes.Success, exitCode);
            Assert.AreEqual(string.Empty, error);
            Assert.AreEqual(UnionWithOneLineBracesFormatted, await File.ReadAllTextAsync(filePath, TestContext.CancellationToken));
            Assert.AreEqual(ExitCodes.Success, checkExitCode);
        }
    }

    /// <summary>
    /// Verifies that a type named <c>union</c> used as a method's return type is skipped with syntax errors without
    /// <c>--lang-version</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [Requirement("CLI-004")]
    public async Task TypeNamedUnionIsSkippedWithoutLangVersion()
    {
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            var filePath = tempDir.CreateFile("C.cs", TypeNamedUnionSource);

            var (exitCode, output, error) = await Run([filePath]);

            Assert.AreEqual(ExitCodes.Success, exitCode);
            Assert.AreEqual(string.Empty, error);
            Assert.AreEqual(Join(string.Empty, "Formatted 0 of 1 file(s).", SyntaxErrorSummaryLine), output);
            Assert.AreEqual(TypeNamedUnionSource, await File.ReadAllTextAsync(filePath, TestContext.CancellationToken));
        }
    }

    /// <summary>
    /// Verifies that <c>--lang-version 14</c> formats a type named <c>union</c> used as a method's return type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [Requirement("CLI-004")]
    public async Task TypeNamedUnionIsFormattedUnderLangVersion14()
    {
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            var filePath = tempDir.CreateFile("C.cs", TypeNamedUnionSource);

            var (exitCode, output, error) = await Run(["--lang-version", "14", filePath]);

            Assert.AreEqual(ExitCodes.Success, exitCode);
            Assert.AreEqual(string.Empty, error);
            Assert.AreEqual(Join($"Formatted: {filePath}", string.Empty, "Formatted 1 of 1 file(s)."), output);
            Assert.AreEqual(TypeNamedUnionFormatted, await File.ReadAllTextAsync(filePath, TestContext.CancellationToken));
        }
    }

    /// <summary>
    /// Verifies that a type named <c>closed</c> used as a field type and a method's return type is skipped with syntax errors
    /// without <c>--lang-version</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [Requirement("CLI-004")]
    public async Task TypeNamedClosedIsSkippedWithoutLangVersion()
    {
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            var filePath = tempDir.CreateFile("C.cs", TypeNamedClosedSource);

            var (exitCode, output, error) = await Run([filePath]);

            Assert.AreEqual(ExitCodes.Success, exitCode);
            Assert.AreEqual(string.Empty, error);
            Assert.AreEqual(Join(string.Empty, "Formatted 0 of 1 file(s).", SyntaxErrorSummaryLine), output);
            Assert.AreEqual(TypeNamedClosedSource, await File.ReadAllTextAsync(filePath, TestContext.CancellationToken));
        }
    }

    /// <summary>
    /// Verifies that <c>--lang-version 14</c> formats a type named <c>closed</c> used as a field type and a method's return type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [Requirement("CLI-004")]
    public async Task TypeNamedClosedIsFormattedUnderLangVersion14()
    {
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            var filePath = tempDir.CreateFile("C.cs", TypeNamedClosedSource);

            var (exitCode, output, error) = await Run(["--lang-version", "14", filePath]);

            Assert.AreEqual(ExitCodes.Success, exitCode);
            Assert.AreEqual(string.Empty, error);
            Assert.AreEqual(Join($"Formatted: {filePath}", string.Empty, "Formatted 1 of 1 file(s)."), output);
            Assert.AreEqual(TypeNamedClosedFormatted, await File.ReadAllTextAsync(filePath, TestContext.CancellationToken));
        }
    }

    /// <summary>
    /// Verifies that a file whose top-level statement starts with a local named <c>union</c> is skipped with syntax errors
    /// without <c>--lang-version</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [Requirement("CLI-004")]
    public async Task TopLevelStatementStartingWithUnionLocalIsSkippedWithoutLangVersion()
    {
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            var filePath = tempDir.CreateFile("Program.cs", TopLevelUnionLocalSource);

            var (exitCode, output, error) = await Run([filePath]);

            Assert.AreEqual(ExitCodes.Success, exitCode);
            Assert.AreEqual(string.Empty, error);
            Assert.AreEqual(Join(string.Empty, "Formatted 0 of 1 file(s).", SyntaxErrorSummaryLine), output);
            Assert.AreEqual(TopLevelUnionLocalSource, await File.ReadAllTextAsync(filePath, TestContext.CancellationToken));
        }
    }

    /// <summary>
    /// Verifies that <c>--lang-version 14</c> formats a file whose top-level statement starts with a local named <c>union</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test</returns>
    [TestMethod]
    [Requirement("CLI-004")]
    public async Task TopLevelStatementStartingWithUnionLocalIsFormattedUnderLangVersion14()
    {
        using (var tempDir = new TemporaryDirectoryFixture())
        {
            var filePath = tempDir.CreateFile("Program.cs", TopLevelUnionLocalSource);

            var (exitCode, output, error) = await Run(["--lang-version", "14", filePath]);

            Assert.AreEqual(ExitCodes.Success, exitCode);
            Assert.AreEqual(string.Empty, error);
            Assert.AreEqual(Join($"Formatted: {filePath}", string.Empty, "Formatted 1 of 1 file(s)."), output);
            Assert.AreEqual(TopLevelUnionLocalFormatted, await File.ReadAllTextAsync(filePath, TestContext.CancellationToken));
        }
    }

    /// <summary>
    /// Runs the CLI and captures its exit code and console output
    /// </summary>
    /// <param name="arguments">The command-line arguments</param>
    /// <returns>The exit code, standard output, and standard error</returns>
    private static async Task<(int ExitCode, string Output, string Error)> Run(string[] arguments)
    {
        using (var capture = new ConsoleCapture())
        {
            var exitCode = await Program.Main(arguments);

            return (exitCode, capture.StandardOutput, capture.StandardError);
        }
    }

    /// <summary>
    /// Joins lines with the console's line break and ends the last one with it
    /// </summary>
    /// <param name="lines">The lines</param>
    /// <returns>The text the console writes for the lines</returns>
    private static string Join(params string[] lines)
    {
        return string.Concat(lines.Select(line => line + Environment.NewLine));
    }

    #endregion // Methods
}