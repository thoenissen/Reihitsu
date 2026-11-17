using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline;
using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Structural;

/// <summary>
/// Structural transforms that rebuild trivia (switch-case brace insertion and removal,
/// control-flow brace insertion, field-declaration splitting) must not drop preprocessor
/// directives, which would silently remove conditional compilation. The transform is refused
/// when a directive is entangled with the trivia it would rebuild, leaving the directive intact
/// </summary>
[TestClass]
public class DirectivePreservationTests : FormatterTestsBase
{
    #region Fields

    /// <summary>
    /// Parse options with <c>DEBUG</c> defined, which keep the directive branch active
    /// </summary>
    private static readonly CSharpParseOptions _parseOptions = new(preprocessorSymbols: ["DEBUG"]);

    #endregion // Fields

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

        AssertRuleResult(input, parseOptions: _parseOptions);
    }

    /// <summary>
    /// Verifies that control-flow brace insertion is refused when a directive wraps the statement, so the
    /// <c>#if</c>/<c>#endif</c> around the conditional body are not deleted, and that the still-unbraced body
    /// is indented one level deeper than the <c>if</c> statement rather than left at its column
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-003")]
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

        const string expected = """
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

        AssertRuleResult(input, expected, _parseOptions);
    }

    /// <summary>
    /// Verifies that a <c>#pragma</c> directive between an unbraced then-branch and its <c>else</c> keyword
    /// is kept on its own line, and the whole document still parses, once braces are inserted around
    /// both branches
    /// </summary>
    [TestMethod]
    public void ControlFlowBraceInsertionKeepsDirectiveBeforeElse()
    {
        const string input = """
                             class C
                             {
                                 void M(bool b)
                                 {
                                     if (b)
                                         Foo();
                             #pragma warning disable CS0168
                                     else
                                         Bar();
                             #pragma warning restore CS0168
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M(bool b)
                                    {
                                        if (b)
                                        {
                                            Foo();
                                        }
                                #pragma warning disable CS0168
                                        else
                                        {
                                            Bar();
                                        }
                                #pragma warning restore CS0168
                                    }
                                }
                                """;

        AssertRuleResult(input, expected, _parseOptions);
    }

    /// <summary>
    /// Verifies that a multi-variable field declaration with a conditional group around the first initializer value is refused
    /// as unsupported rather than split, so the <c>#if</c>/<c>#endif</c> around the initializer are neither deleted nor moved:
    /// the pipeline returns the input unchanged and reports the <c>#if DEBUG</c> directive
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-010")]
    public void FieldDeclarationWithDirectivesInDeclarationIsRefused()
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

        var text = NormalizeLineEndings(input, "\n");
        var tree = CSharpSyntaxTree.ParseText(text, _parseOptions, cancellationToken: TestContext.CancellationToken);
        var result = FormattingPipeline.Execute(tree.GetRoot(TestContext.CancellationToken), new FormattingContext("\n"), TestContext.CancellationToken);

        Assert.AreEqual(text, result.Output.ToFullString(), "The refused input must be returned unchanged.");
        Assert.IsNotNull(result.Unsupported, "The input must be refused as unsupported.");
        Assert.AreEqual("a directive inside a field declaration with several declarators", result.Unsupported.Reason);
        Assert.AreEqual(text.IndexOf("#if DEBUG", System.StringComparison.Ordinal), result.Unsupported.Location.SourceSpan.Start);
    }

    #endregion // Tests
}