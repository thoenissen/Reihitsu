using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using NSubstitute;

using Reihitsu.Cli.Abstractions;
using Reihitsu.Cli.Test.Helpers;
using Reihitsu.Formatter;
using Reihitsu.Formatter.Utilities;

namespace Reihitsu.Cli.Test.Unit;

/// <summary>
/// Tests for two selected paths that name one physical file in write mode with a degree of parallelism above one: whichever
/// path is named and counted, the file holds the formatted text (CLI-022)
/// </summary>
[TestClass]
public sealed class FormatCommandHandlerAliasTests
{
    #region Constants

    /// <summary>
    /// The path the file is reached through first
    /// </summary>
    private const string RealPath = "/work/real/A.cs";

    /// <summary>
    /// A second path that names the same physical file, as through a symbolic link
    /// </summary>
    private const string LinkPath = "/work/link/A.cs";

    /// <summary>
    /// The file's original content, two classes with an unformatted field each
    /// </summary>
    private const string OriginalContent = "class A\n{\n    int  x;\n}\n\nclass B\n{\n    int  y;\n}\n";

    /// <summary>
    /// The formatted form of <see cref="OriginalContent"/>
    /// </summary>
    private const string FormattedContent = "class A\n{\n    int x;\n}\n\nclass B\n{\n    int y;\n}";

    /// <summary>
    /// The first class of <see cref="FormattedContent"/> with its line break: what a file holds while the write of the formatted
    /// text has written its first chunk only; it parses without errors
    /// </summary>
    private const string FormattedPrefix = "class A\n{\n    int x;\n}\n";

    /// <summary>
    /// The formatted form of <see cref="FormattedPrefix"/>, which differs from it
    /// </summary>
    private const string FormattedPrefixFormatted = "class A\n{\n    int x;\n}";

    #endregion // Constants

    #region Fields

    /// <summary>
    /// How long a held fake waits for the event it is held for before it gives up, so a run that never produces the event
    /// goes on instead of hanging
    /// </summary>
    private static readonly TimeSpan _holdTimeout = TimeSpan.FromSeconds(2);

    #endregion // Fields

    #region Helper Methods

    /// <summary>
    /// Creates a formatter fake that maps <see cref="OriginalContent"/> to <see cref="FormattedContent"/> and
    /// <see cref="FormattedPrefix"/> to <see cref="FormattedPrefixFormatted"/>, and returns every other text as it is
    /// </summary>
    /// <returns>The formatter fake</returns>
    private static ISourceFormatter CreateFormatter()
    {
        var formatter = Substitute.For<ISourceFormatter>();
        var formatted = new Dictionary<string, string>(StringComparer.Ordinal)
                        {
                            [OriginalContent] = FormattedContent,
                            [FormattedPrefix] = FormattedPrefixFormatted
                        };

        formatter.FormatSyntaxTree(Arg.Any<SyntaxTree>(), Arg.Any<CancellationToken>())
                 .Returns(callInfo =>
                          {
                              var syntaxTree = callInfo.Arg<SyntaxTree>();
                              var text = syntaxTree.GetText().ToString();
                              var output = formatted.GetValueOrDefault(text, text);

                              return new FormattingResult<SyntaxTree>(CSharpSyntaxTree.ParseText(output, path: syntaxTree.FilePath), null);
                          });

        return formatter;
    }

    /// <summary>
    /// Runs write mode with a degree of two over both paths of the shared file
    /// </summary>
    /// <param name="fileSystem">The file system fake</param>
    /// <param name="utf8Bom">Whether to normalize processed files to UTF-8 with a byte order mark</param>
    /// <returns>The exit code</returns>
    private static async Task<int> RunWriteModeOverBothPaths(SharedFileFileSystem fileSystem, bool utf8Bom)
    {
        var console = new CapturedConsoleOutput();
        var dependencies = new FormatCommandDependencies(fileSystem, console, Substitute.For<IConsoleInput>(), CreateFormatter(), Substitute.For<IDiffGenerator>());
        var handler = new FormatCommandHandler([RealPath, LinkPath],
                                               checkOnly: false,
                                               dryRun: false,
                                               verbose: true,
                                               force: true,
                                               utf8Bom,
                                               LanguageVersionResolver.MaxSupportedLanguageVersion,
                                               dependencies,
                                               suppressUnsupported: false,
                                               maxParallelism: 2);

        return await handler.ExecuteAsync(CancellationToken.None);
    }

    #endregion // Helper Methods

    #region Tests

    /// <summary>
    /// Verifies that the file holds the formatted text with a byte order mark when <c>--utf8-bom</c> is given and the file is
    /// empty while a write to it is in progress, whichever path is read when
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    [Requirement("CLI-022")]
    public async Task FileReachedThroughTwoPathsHoldsFormattedTextWhenEmptyDuringWrite()
    {
        var fileSystem = new SharedFileFileSystem([RealPath, LinkPath], OriginalContent, contentDuringWrite: string.Empty, _holdTimeout);

        var exitCode = await RunWriteModeOverBothPaths(fileSystem, utf8Bom: true);

        Assert.AreEqual(ExitCodes.Success, exitCode);
        Assert.AreEqual(FormattedContent, fileSystem.Content);
        Assert.HasCount(3, fileSystem.Encoding.GetPreamble());
    }

    /// <summary>
    /// Verifies that the file holds the formatted text when a write to it shows only a prefix of the new text while it is in
    /// progress, a prefix that parses and that the formatter changes, whichever path is read when
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    [Requirement("CLI-022")]
    public async Task FileReachedThroughTwoPathsHoldsFormattedTextWhenPrefixDuringWrite()
    {
        var fileSystem = new SharedFileFileSystem([RealPath, LinkPath], OriginalContent, contentDuringWrite: FormattedPrefix, _holdTimeout);

        var exitCode = await RunWriteModeOverBothPaths(fileSystem, utf8Bom: false);

        Assert.AreEqual(ExitCodes.Success, exitCode);
        Assert.AreEqual(FormattedContent, fileSystem.Content);
        Assert.IsEmpty(fileSystem.Encoding.GetPreamble());
    }

    #endregion // Tests
}