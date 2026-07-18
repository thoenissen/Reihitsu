using System.Threading;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline;

namespace Reihitsu.Formatter.Test.Regression.Structural;

/// <summary>
/// Regression tests for issue #406: structural transforms that rebuild trivia (switch-case brace
/// insertion and removal, control-flow brace insertion, field-declaration splitting) must not drop
/// preprocessor directives, which would silently remove conditional compilation. The transform is
/// refused when a directive is entangled with the trivia it would rebuild, leaving the directive intact
/// </summary>
[TestClass]
public class DirectivePreservationTests
{
    #region Properties

    /// <summary>
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Formats the source through the full pipeline, parsing with <c>DEBUG</c> defined so the
    /// conditional branch stays active
    /// </summary>
    /// <param name="input">The C# source text</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The formatted source text</returns>
    private static string Format(string input, CancellationToken cancellationToken)
    {
        var parseOptions = new CSharpParseOptions(preprocessorSymbols: new[] { "DEBUG" });
        var tree = CSharpSyntaxTree.ParseText(input, parseOptions, cancellationToken: cancellationToken);
        var context = new FormattingContext("\n");
        var result = FormattingPipeline.Execute(tree.GetRoot(cancellationToken), context, cancellationToken);

        return result.ToFullString();
    }

    /// <summary>
    /// Normalizes raw string literals to the line ending used by <see cref="Format"/>
    /// </summary>
    /// <param name="text">The text to normalize</param>
    /// <returns>The normalized text</returns>
    private static string NormalizeLineEndings(string text)
    {
        return text.Replace("\r\n", "\n");
    }

    /// <summary>
    /// Asserts that formatting produces the expected output and is idempotent, confirming the
    /// directive was preserved rather than dropped by the transform
    /// </summary>
    /// <param name="input">The C# source text</param>
    /// <param name="expected">The expected formatted output</param>
    private void AssertFormatted(string input, string expected)
    {
        var actual = Format(input, TestContext.CancellationToken);

        Assert.AreEqual(expected, actual, "The directive must be preserved.");

        var secondPass = Format(actual, TestContext.CancellationToken);

        Assert.AreEqual(expected, secondPass, "Formatting must be idempotent.");
    }

    #endregion // Methods

    #region Tests

    /// <summary>
    /// Verifies that switch-case brace insertion is refused when a directive spans the section body,
    /// so the <c>#if</c>/<c>#endif</c> around the conditional statements are not deleted
    /// </summary>
    [TestMethod]
    public void SwitchCaseBraceInsertionKeepsDirectivesInSection()
    {
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                             #if DEBUG
                                             Log();
                                             More();
                             #endif
                                             break;
                                     }
                                 }
                             }
                             """;

        AssertFormatted(input, NormalizeLineEndings(input));
    }

    /// <summary>
    /// Verifies that control-flow brace insertion is refused when a directive wraps the statement,
    /// so the <c>#if</c>/<c>#endif</c> around the conditional body are not deleted
    /// </summary>
    [TestMethod]
    public void ControlFlowBraceInsertionKeepsDirectivesAroundBody()
    {
        const string input = """
                             class C
                             {
                                 void M(bool b)
                                 {
                                     if (b)
                             #if DEBUG
                                     Work();
                             #endif
                                 }
                             }
                             """;

        AssertFormatted(input, NormalizeLineEndings(input));
    }

    /// <summary>
    /// Verifies that a multi-variable field declaration carrying a directive is not split, so the
    /// <c>#if</c>/<c>#endif</c> around the initializer are not deleted
    /// </summary>
    [TestMethod]
    public void FieldDeclarationSplitKeepsDirectivesInDeclaration()
    {
        const string input = """
                             class C
                             {
                                 int a =
                             #if DEBUG
                                 1,
                             #endif
                                 b;
                             }
                             """;

        AssertFormatted(input, NormalizeLineEndings(input));
    }

    #endregion // Tests
}