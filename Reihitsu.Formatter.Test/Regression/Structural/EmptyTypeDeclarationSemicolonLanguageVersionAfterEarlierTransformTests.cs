using System.Threading;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Structural;

/// <summary>
/// Regression tests for <see cref="ReihitsuFormatter.FormatSyntaxTree"/>: the C# 12 gate that
/// <see cref="Pipeline.StructuralTransforms.Rewriter.EmptyTypeDeclarationSemicolonTransform"/> reads through
/// <see cref="Reihitsu.Core.EmptyTypeDeclarationSemicolonAnalysisUtilities"/> must hold even after an earlier
/// rewriter in the same <see cref="Pipeline.StructuralTransforms.StructuralTransformPhase"/> pass has already
/// replaced the tree (thoenissen/Reihitsu#821)
/// </summary>
[TestClass]
public class EmptyTypeDeclarationSemicolonLanguageVersionAfterEarlierTransformTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies the issue's literal scenario: an empty class parsed at C# 11 must keep its braced body when a
    /// later structural transform (here, <see cref="Pipeline.StructuralTransforms.Rewriter.ControlFlowBraceTransform"/>
    /// adding braces to an <c>if</c> statement elsewhere in the file) has already replaced the tree
    /// </summary>
    [TestMethod]
    public void EmptyClassStaysBracedBelowCSharp12AfterControlFlowBraceTransformRewroteTree()
    {
        const string input = """
                             public class A { }

                             public class B
                             {
                                 public void M(int x)
                                 {
                                     if (x > 0)
                                         x = 0;
                                 }
                             }
                             """;
        const string expected = """
                                public class A
                                {
                                }

                                public class B
                                {
                                    public void M(int x)
                                    {
                                        if (x > 0)
                                        {
                                            x = 0;
                                        }
                                    }
                                }
                                """;
        var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp11);

        foreach (var endOfLine in _lineEndings)
        {
            var normalizedExpected = NormalizeLineEndings(expected, endOfLine);
            var actual = FormatThroughSyntaxTree(input, endOfLine, parseOptions);

            Assert.AreEqual(normalizedExpected, actual, $"FormatSyntaxTree output mismatch under {DescribeLineEnding(endOfLine)} line endings.");
        }
    }

    /// <summary>
    /// Verifies the same defect for the struct declaration kind, the nearest sibling shape to the issue's class example
    /// </summary>
    [TestMethod]
    public void EmptyStructStaysBracedBelowCSharp12AfterControlFlowBraceTransformRewroteTree()
    {
        const string input = """
                             public struct S { }

                             public class B
                             {
                                 public void M(int x)
                                 {
                                     if (x > 0)
                                         x = 0;
                                 }
                             }
                             """;
        const string expected = """
                                public struct S
                                {
                                }

                                public class B
                                {
                                    public void M(int x)
                                    {
                                        if (x > 0)
                                        {
                                            x = 0;
                                        }
                                    }
                                }
                                """;
        var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp11);

        foreach (var endOfLine in _lineEndings)
        {
            var normalizedExpected = NormalizeLineEndings(expected, endOfLine);
            var actual = FormatThroughSyntaxTree(input, endOfLine, parseOptions);

            Assert.AreEqual(normalizedExpected, actual, $"FormatSyntaxTree output mismatch under {DescribeLineEnding(endOfLine)} line endings.");
        }
    }

    /// <summary>
    /// Verifies the same defect when a convertible accessor block, rather than an unbraced <c>if</c>, is the
    /// earlier rewriter that replaces the tree — the second trigger the issue itself calls out
    /// </summary>
    [TestMethod]
    public void EmptyClassStaysBracedBelowCSharp12AfterAccessorExpressionBodyTransformRewroteTree()
    {
        const string input = """
                             public class A { }

                             public class B
                             {
                                 private int _x;

                                 public int X
                                 {
                                     get { return _x; }
                                 }
                             }
                             """;
        const string expected = """
                                public class A
                                {
                                }

                                public class B
                                {
                                    private int _x;

                                    public int X
                                    {
                                        get => _x;
                                    }
                                }
                                """;
        var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp11);

        foreach (var endOfLine in _lineEndings)
        {
            var normalizedExpected = NormalizeLineEndings(expected, endOfLine);
            var actual = FormatThroughSyntaxTree(input, endOfLine, parseOptions);

            Assert.AreEqual(normalizedExpected, actual, $"FormatSyntaxTree output mismatch under {DescribeLineEnding(endOfLine)} line endings.");
        }
    }

    /// <summary>
    /// Baseline: the same empty class, alone in the file so no earlier rewriter replaces the tree, must already
    /// keep its braced body below C# 12 — establishes that the defect is specific to the multi-rewriter chain,
    /// not to the empty-type gate in general
    /// </summary>
    [TestMethod]
    public void EmptyClassStaysBracedBelowCSharp12WhenNoEarlierTransformRan()
    {
        const string input = """
                             public class A { }
                             """;
        const string expected = """
                                public class A
                                {
                                }
                                """;
        var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp11);

        // Single-line input carries no end-of-line sequence for ReihitsuFormatterHelpers.DetectEndOfLine to key
        // off, so this baseline (unlike the reproducing scenarios above, which contain multiple lines) is
        // exercised under LF only.
        var actual = FormatThroughSyntaxTree(input, "\n", parseOptions);

        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Formats the given source through <see cref="ReihitsuFormatter.FormatSyntaxTree"/> with explicit parse options
    /// </summary>
    /// <param name="input">The input source text</param>
    /// <param name="endOfLine">The end-of-line sequence to normalize the input to</param>
    /// <param name="parseOptions">The parse options to parse the input with</param>
    /// <returns>The formatted source text</returns>
    private static string FormatThroughSyntaxTree(string input, string endOfLine, CSharpParseOptions parseOptions)
    {
        var normalizedInput = NormalizeLineEndings(input, endOfLine);
        var tree = CSharpSyntaxTree.ParseText(normalizedInput, parseOptions);
        var formattedTree = ReihitsuFormatter.FormatSyntaxTree(tree, CancellationToken.None);

        return formattedTree.GetRoot().ToFullString();
    }

    #endregion // Methods
}