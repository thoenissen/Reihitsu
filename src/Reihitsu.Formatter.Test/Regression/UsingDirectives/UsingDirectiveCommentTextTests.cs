using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.UsingDirectives;

/// <summary>
/// Full-pipeline tests for using blocks whose trivia holds a line that starts with <c>#</c>: a line of a delimited comment is
/// comment text and does not prevent reordering, while a preprocessor directive anywhere in the trivia of the block's
/// directives, also between their tokens, does
/// </summary>
[TestClass]
public class UsingDirectiveCommentTextTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies the specification example: a line that starts with <c>#</c> inside a file header comment is comment text, so the
    /// block is reordered with the header kept on top
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    public void HashLineInFileHeaderCommentDoesNotPreventReordering()
    {
        // Arrange
        const string input = """
                             /*
                             # Header
                             */
                             using System.Linq;
                             using System;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                /*
                                # Header
                                */
                                using System;
                                using System.Linq;

                                class C
                                {
                                    int x;
                                }
                                """;

        // Act & Assert
        AssertReordered(input, expected);
    }

    /// <summary>
    /// Verifies that an indented line that starts with <c>#</c> inside a comment between two directives does not prevent
    /// reordering, and that the comment moves with the directive it leads
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    public void HashLineInCommentBetweenDirectivesDoesNotPreventReordering()
    {
        // Arrange
        const string input = """
                             using System.Linq;
                             /*
                                 # Note
                             */
                             using System;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                /*
                                    # Note
                                */
                                using System;
                                using System.Linq;

                                class C
                                {
                                    int x;
                                }
                                """;

        // Act & Assert
        AssertReordered(input, expected);
    }

    /// <summary>
    /// Verifies that a line that starts with <c>#</c> inside a comment that starts behind a directive's semicolon does not
    /// prevent reordering, and that the comment stays behind its directive
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    public void HashLineInTrailingCommentDoesNotPreventReordering()
    {
        // Arrange
        const string input = """
                             using System.Linq; /*
                             # Note
                             */
                             using System;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                using System;
                                using System.Linq; /*
                                # Note
                                */

                                class C
                                {
                                    int x;
                                }
                                """;

        // Act & Assert
        AssertReordered(input, expected);
    }

    /// <summary>
    /// Verifies that a line that starts with <c>#</c> inside a delimited documentation comment before the first directive does
    /// not prevent reordering
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    public void HashLineInDelimitedDocumentationCommentDoesNotPreventReordering()
    {
        // Arrange
        const string input = """
                             /**
                             # Header
                             */
                             using System.Linq;
                             using System;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                /**
                                # Header
                                */
                                using System;
                                using System.Linq;

                                class C
                                {
                                    int x;
                                }
                                """;

        // Act & Assert
        AssertReordered(input, expected);
    }

    /// <summary>
    /// Verifies that a line that starts with <c>#</c> inside a comment between the tokens of a directive does not prevent
    /// reordering
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    public void HashLineInCommentBetweenTokensOfDirectiveDoesNotPreventReordering()
    {
        // Arrange
        const string input = """
                             using System.Linq;
                             using /*
                             # Note
                             */ System;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                using /*
                                # Note
                                */ System;
                                using System.Linq;

                                class C
                                {
                                    int x;
                                }
                                """;

        // Act & Assert
        AssertReordered(input, expected);
    }

    /// <summary>
    /// Verifies that a line that starts with <c>#</c> inside a comment before the first directive of a block-scoped namespace
    /// does not prevent reordering
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    public void HashLineInCommentOfBlockNamespaceDoesNotPreventReordering()
    {
        // Arrange
        const string input = """
                             namespace N
                             {
                                 /*
                                 # Header
                                 */
                                 using System.Linq;
                                 using System;

                                 class C
                                 {
                                     int x;
                                 }
                             }
                             """;
        const string expected = """
                                namespace N
                                {
                                    /*
                                    # Header
                                    */
                                    using System;
                                    using System.Linq;

                                    class C
                                    {
                                        int x;
                                    }
                                }
                                """;

        // Act & Assert
        AssertReordered(input, expected);
    }

    /// <summary>
    /// Verifies that a line that starts with <c>#</c> inside a comment before the first directive of a file-scoped namespace
    /// does not prevent reordering
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    public void HashLineInCommentOfFileScopedNamespaceDoesNotPreventReordering()
    {
        // Arrange
        const string input = """
                             namespace N;

                             /*
                             # Header
                             */
                             using System.Linq;
                             using System;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                namespace N;

                                /*
                                # Header
                                */
                                using System;
                                using System.Linq;

                                class C
                                {
                                    int x;
                                }
                                """;

        // Act & Assert
        AssertReordered(input, expected);
    }

    /// <summary>
    /// Verifies that a commented-out conditional block of directives is comment text and does not prevent reordering
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    public void CommentedOutConditionalDirectivesDoNotPreventReordering()
    {
        // Arrange
        const string input = """
                             /*
                             #if DEBUG
                             using System.Diagnostics;
                             #endif
                             */
                             using System.Linq;
                             using System;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                /*
                                #if DEBUG
                                using System.Diagnostics;
                                #endif
                                */
                                using System;
                                using System.Linq;

                                class C
                                {
                                    int x;
                                }
                                """;

        // Act & Assert
        AssertReordered(input, expected);
    }

    /// <summary>
    /// Verifies the specification example: a <c>#pragma</c> directive between the tokens of a using directive makes the block
    /// unsafe, so it is left unchanged
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    public void PragmaBetweenTokensOfDirectivePreventsReordering()
    {
        // Arrange
        const string input = """
                             using System.Linq;
                             using System
                             #pragma warning disable CS0105
                                 .Text;
                             using System;

                             class C
                             {
                                 int x;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
        AssertUnchangedThroughEntryPoint(input);
    }

    /// <summary>
    /// Verifies that an <c>#if</c> and its <c>#endif</c> between the tokens of one using directive make the block unsafe, so it
    /// is left unchanged
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    public void ConditionalGroupBetweenTokensOfDirectivePreventsReordering()
    {
        // Arrange
        const string input = """
                             using System.Linq;
                             using System
                             #if A
                             .Text
                             #endif
                                 ;

                             class C
                             {
                                 int x;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
        AssertUnchangedThroughEntryPoint(input);
    }

    /// <summary>
    /// Verifies that an <c>#if</c> between the tokens of one using directive and its <c>#endif</c> between the tokens of the next
    /// make the block unsafe, so it is left unchanged
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    public void ConditionalGroupAcrossTokensOfTwoDirectivesPreventsReordering()
    {
        // Arrange
        const string input = """
                             using System
                             #if !A
                                 .Text;
                             using System.Collections
                             #endif
                                 ;
                             using System.Buffers;

                             class C
                             {
                                 int x;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
        AssertUnchangedThroughEntryPoint(input);
    }

    /// <summary>
    /// Verifies that a namespace whose block follows a comment with a line that reads as <c>#else</c> in another build, started
    /// inside a conditional group that closes behind the block, is left unchanged by whole-document formatting
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-033")]
    public void HashLineReadAsDirectiveByAnotherBuildLeavesTheDocumentUnchanged()
    {
        // Arrange
        const string input = """
                             #if !A
                             namespace N
                             {
                                 /*
                             #else
                             namespace M
                             {
                                 // */
                                 using System.Linq;
                                 using System;
                             #endif

                                 class C
                                 {
                                     int x;
                                 }
                             }
                             """;

        // Act & Assert
        AssertUnchangedThroughEntryPoint(input);
    }

    /// <summary>
    /// Verifies under LF and CRLF line endings that the pipeline and <see cref="ReihitsuFormatter.FormatSyntaxTree"/> format the
    /// input to the expected text, and that the expected text is a fixed point of the formatter
    /// </summary>
    /// <param name="input">The input source text</param>
    /// <param name="expected">The expected output</param>
    private void AssertReordered(string input, string expected)
    {
        AssertRuleResult(input, expected);

        foreach (var endOfLine in _lineEndings)
        {
            var tree = CSharpSyntaxTree.ParseText(NormalizeLineEndings(input, endOfLine), cancellationToken: TestContext.CancellationToken);
            var result = ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken);

            Assert.IsNull(result.Unsupported);
            Assert.AreEqual(NormalizeLineEndings(expected, endOfLine), result.Output.GetText(TestContext.CancellationToken).ToString(), $"Formatter output mismatch under {DescribeLineEnding(endOfLine)} line endings.");
        }
    }

    /// <summary>
    /// Verifies under LF and CRLF line endings that <see cref="ReihitsuFormatter.FormatSyntaxTree"/> returns the input unchanged
    /// and does not report it as unsupported
    /// </summary>
    /// <param name="input">The input source text</param>
    private void AssertUnchangedThroughEntryPoint(string input)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var text = NormalizeLineEndings(input, endOfLine);
            var tree = CSharpSyntaxTree.ParseText(text, cancellationToken: TestContext.CancellationToken);

            Assert.DoesNotContain(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error, tree.GetDiagnostics(TestContext.CancellationToken), "The fixture must parse without an error.");

            var result = ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken);

            Assert.IsNull(result.Unsupported);
            Assert.AreEqual(text, result.Output.GetText(TestContext.CancellationToken).ToString(), $"The formatter must return the document unchanged under {DescribeLineEnding(endOfLine)} line endings.");
        }
    }

    #endregion // Methods
}