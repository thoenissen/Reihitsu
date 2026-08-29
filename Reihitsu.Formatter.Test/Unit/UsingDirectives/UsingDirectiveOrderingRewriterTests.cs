using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.UsingDirectives;
using Reihitsu.Formatter.Pipeline.UsingDirectives.Rewriter;
using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit.UsingDirectives;

/// <summary>
/// Tests for <see cref="UsingDirectiveOrderingPhase"/> using directive ordering
/// </summary>
[TestClass]
public class UsingDirectiveOrderingRewriterTests : FormatterPhaseTestsBase
{
    #region Tests

    /// <summary>
    /// Verifies that regular usings without trivia are reordered
    /// </summary>
    [TestMethod]
    public void RegularUsingsWithoutTriviaAreReordered()
    {
        // Arrange
        const string input = """
                             using System.Linq;
                             using System;
                             """;
        var expected = $"using System;{Environment.NewLine}using System.Linq;";

        // Assert
        Assert.AreEqual(expected, ApplyPhase(input));
    }

    /// <summary>
    /// Verifies that alias directives without trivia are reordered
    /// </summary>
    [TestMethod]
    public void AliasDirectivesWithoutTriviaAreReordered()
    {
        // Arrange
        const string input = """
                             using L = System.Linq;
                             using C = System.Collections;
                             """;
        var expected = $"using C = System.Collections;{Environment.NewLine}using L = System.Linq;";

        // Assert
        Assert.AreEqual(expected, ApplyPhase(input));
    }

    /// <summary>
    /// Verifies that using static directives without trivia are reordered
    /// </summary>
    [TestMethod]
    public void UsingStaticDirectivesWithoutTriviaAreReordered()
    {
        // Arrange
        const string input = """
                             using static System.Math;
                             using static System.Console;
                             """;
        var expected = $"using static System.Console;{Environment.NewLine}using static System.Math;";

        // Assert
        Assert.AreEqual(expected, ApplyPhase(input));
    }

    /// <summary>
    /// Verifies that cross-group usings without trivia and with no trailing content receive a blank-line
    /// separator instead of a single line break, when the using block is the only content in the file (issue #728)
    /// </summary>
    [TestMethod]
    public void CrossGroupUsingsWithoutTriviaAreReordered()
    {
        // Arrange
        const string input = """
                             using MyProject.Common;
                             using System;
                             """;
        var expected = $"using System;{Environment.NewLine}{Environment.NewLine}using MyProject.Common;";

        // Assert
        Assert.AreEqual(expected, ApplyPhase(input));
    }

    /// <summary>
    /// Verifies that the block's own terminating line break survives a reorder that moves the originally
    /// last directive away from the last position (issue #728)
    /// </summary>
    [TestMethod]
    public void RegularUsingsWithoutTriviaButWithTerminatingNewlineKeepTheNewline()
    {
        // Arrange
        const string input = "using System.Linq;\nusing System;\n";
        var expected = $"using System;{Environment.NewLine}using System.Linq;{Environment.NewLine}";

        // Assert
        Assert.AreEqual(expected, ApplyPhase(input));
    }

    /// <summary>
    /// Verifies that a trailing line comment on the originally last directive is not silently absorbed
    /// into the next reordered directive when the block has no terminating newline (issue #728)
    /// </summary>
    [TestMethod]
    public void TrailingCommentOnLastDirectiveIsNotAbsorbedIntoNextDirectiveAfterReorder()
    {
        // Arrange
        const string input = "using System.Linq;\nusing System.Collections.Generic; // tail";
        var expected = $"using System.Collections.Generic; // tail{Environment.NewLine}using System.Linq;";

        // Assert
        Assert.AreEqual(expected, ApplyPhase(input));
    }

    /// <summary>
    /// Verifies that a leading comment attached to a moved directive starts its own line rather than
    /// joining the line of a predecessor whose own trailing trivia carried no line break (issue #728)
    /// </summary>
    [TestMethod]
    public void LeadingCommentAfterTerminatingNewlineLessPredecessorStartsItsOwnLine()
    {
        // Arrange
        const string input = "using System.One;\n// note\nusing System.Zeta;\nusing System.Two;";
        var expected = $"using System.One;{Environment.NewLine}using System.Two;{Environment.NewLine}// note{Environment.NewLine}using System.Zeta;";

        // Assert
        Assert.AreEqual(expected, ApplyPhase(input));
    }

    /// <summary>
    /// Verifies that a reorder which leaves the originally last, terminating-newline-less directive in
    /// the last position produces the same output as before the fix, since no directive's terminal
    /// trailing trivia moves (issue #728)
    /// </summary>
    [TestMethod]
    public void TerminatingNewlineLessDirectiveThatStaysLastAfterReorderIsUnaffected()
    {
        // Arrange
        const string input = "using System.Linq;\nusing System.Collections.Generic;\nusing System.Xml;";
        var expected = $"using System.Collections.Generic;{Environment.NewLine}using System.Linq;{Environment.NewLine}using System.Xml;";

        // Assert
        Assert.AreEqual(expected, ApplyPhase(input));
    }

    /// <summary>
    /// Verifies that a reorder does not force a line break between directives whose block never had one
    /// to begin with, so the block stays on the one physical line it was authored on (issue #728)
    /// </summary>
    [TestMethod]
    public void DirectivesSharingOneLineWithNoLineBreakAnywhereInTheBlockStayOnThatLine()
    {
        // Arrange
        const string input = "namespace N { using System.Linq; using System.Collections.Generic; }";
        const string expected = "namespace N { using System.Collections.Generic; using System.Linq; }";

        // Assert
        Assert.AreEqual(expected, ApplyPhase(input));
    }

    /// <summary>
    /// Verifies that reordering a directive into the last position, where the block's own closing brace
    /// shares its line, preserves the space before that brace instead of gluing the directive to it
    /// (issue #728). The five-space gap after the first directive is a pre-existing, unrelated quirk of
    /// this phase's leading-trivia indentation extraction on a directive that no longer starts a fresh
    /// line, not a defect this fix introduces or is responsible for correcting
    /// </summary>
    [TestMethod]
    public void LastDirectiveSharingItsLineWithTheClosingBraceKeepsTheSpaceBeforeIt()
    {
        // Arrange
        const string input = "namespace N\n{\n    using System.Linq;\n    using System.Collections.Generic; }";
        const string expected = "namespace N\n{\n    using System.Collections.Generic;     using System.Linq; }";

        // Assert
        Assert.AreEqual(expected, ApplyPhase(input));
    }

    /// <summary>
    /// Verifies that reordering a directive with a trailing single-line comment into the last position,
    /// where the block's own terminator has no line break of its own, still terminates that comment
    /// instead of letting it absorb whatever the transplanted block terminator appends after it
    /// (issue #728)
    /// </summary>
    [TestMethod]
    public void CommentOnDirectiveMovedToLastPositionIsTerminatedBeforeTheBlockTerminatorIsAppended()
    {
        // Arrange
        const string input = "namespace N { using System.Linq; // keep\nusing System.Collections.Generic; }";
        const string expected = "namespace N { using System.Collections.Generic; using System.Linq; // keep\n }";

        // Assert
        Assert.AreEqual(expected, ApplyPhase(input));
    }

    /// <summary>
    /// Verifies that reordering a directive with a trailing single-line comment into the last position
    /// does not duplicate the line break when the block's own terminator already starts with one, which
    /// would otherwise insert a spurious blank line (issue #728)
    /// </summary>
    [TestMethod]
    public void CommentOnDirectiveMovedToLastPositionDoesNotDuplicateAnAlreadyPresentBlockTerminatorLineBreak()
    {
        // Arrange
        const string input = "using global::SYSTEM.Text; // Keep with the case variant\nusing System.Text;\n\nclass C;";
        const string expected = "using System.Text;\n\nusing global::SYSTEM.Text; // Keep with the case variant\n\nclass C;";

        // Assert
        Assert.AreEqual(expected, ApplyPhase(input));
    }

    /// <summary>
    /// Verifies that conditional directives skip reordering
    /// </summary>
    [TestMethod]
    public void ConditionalDirectiveSkipsReordering()
    {
        // Arrange
        const string input = """
                             using System;
                             #if DEBUG
                             using System.Linq;
                             #endif
                             """;

        // Assert
        Assert.AreEqual(input, ApplyPhase(input));
    }

    /// <summary>
    /// Verifies that a nullable directive on a later directive skips reordering
    /// </summary>
    [TestMethod]
    public void NullableDirectiveSkipsReordering()
    {
        // Arrange
        const string input = """
                             using System;
                             #nullable enable
                             using System.Linq;
                             """;

        // Assert
        Assert.AreEqual(input, ApplyPhase(input));
    }

    /// <summary>
    /// Verifies that a pragma directive on a later directive skips reordering
    /// </summary>
    [TestMethod]
    public void PragmaDirectiveSkipsReordering()
    {
        // Arrange
        const string input = """
                             using System;
                             #pragma warning disable CS8019
                             using System.Linq;
                             """;

        // Assert
        Assert.AreEqual(input, ApplyPhase(input));
    }

    /// <summary>
    /// Verifies that using directives are reordered inside a namespace declaration
    /// </summary>
    [TestMethod]
    public void NamespaceUsingsAreReordered()
    {
        // Arrange
        const string input = """
                             namespace Example
                             {
                                 using System.Linq;
                                 using System;
                             }
                             """;
        const string expected = """
                                namespace Example
                                {
                                    using System;
                                    using System.Linq;
                                }
                                """;

        // Assert
        Assert.AreEqual(expected, ApplyPhase(input));
    }

    /// <summary>
    /// Verifies that attached comments remain with non-first namespace usings after reordering
    /// </summary>
    [TestMethod]
    public void NamespaceUsingsWithCommentsKeepAttachedTrivia()
    {
        // Arrange
        const string input = """
                             namespace Example
                             {
                                 using Zeta;
                                 using System.Collections;
                                 // Keep with Alpha
                                 using Alpha;
                             }
                             """;
        const string expected = """
                                namespace Example
                                {
                                    using System.Collections;

                                    // Keep with Alpha
                                    using Alpha;

                                    using Zeta;
                                }
                                """;

        // Assert
        Assert.AreEqual(expected, ApplyPhase(input));
    }

    /// <summary>
    /// Verifies that a comment between same-group usings does not gain a blank group separator
    /// </summary>
    [TestMethod]
    public void CommentSeparatedSameGroupUsingsRemainTogether()
    {
        // Arrange
        const string input = "using System;\n// I/O helpers\nusing System.IO;";

        // Assert
        Assert.AreEqual(input, ApplyPhase(input));
    }

    /// <summary>
    /// Verifies that a comment attached to a different-group using receives a preceding blank separator
    /// </summary>
    [TestMethod]
    public void CommentPrefixedDifferentGroupUsingReceivesSeparator()
    {
        // Arrange
        const string input = "using System;\n// Alpha helpers\nusing Alpha;";
        var expected = $"using System;\n{Environment.NewLine}// Alpha helpers\nusing Alpha;";

        // Assert
        Assert.AreEqual(expected, ApplyPhase(input));
    }

    /// <summary>
    /// Verifies that using directives are reordered inside a file-scoped namespace
    /// </summary>
    [TestMethod]
    public void FileScopedNamespaceUsingsAreReordered()
    {
        // Arrange
        const string input = """
                             namespace Example;

                             using System.Linq;
                             using System;

                             class C
                             {
                             }
                             """;
        const string expected = """
                                namespace Example;

                                using System;
                                using System.Linq;

                                class C
                                {
                                }
                                """;

        // Assert
        Assert.AreEqual(expected, ApplyPhase(input));
    }

    /// <summary>
    /// Verifies that a single using directive is not processed
    /// </summary>
    [TestMethod]
    public void SingleUsingDirectiveIsNotProcessed()
    {
        // Arrange
        const string input = """
                             using System;
                             """;

        // Assert
        Assert.AreEqual(input, ApplyPhase(input));
    }

    /// <summary>
    /// Verifies that organizing a single directive returns the original list
    /// </summary>
    [TestMethod]
    public void OrganizeUsingDirectivesReturnsOriginalListWhenOnlyOneDirectiveExists()
    {
        var cancellationToken = TestContext.CancellationToken;
        var root = (CompilationUnitSyntax)CSharpSyntaxTree.ParseText("using System;", cancellationToken: cancellationToken).GetRoot(cancellationToken);

        var result = UsingDirectiveOrderingRewriter.OrganizeUsingDirectives(root.Usings, Environment.NewLine, cancellationToken);

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(root.Usings[0].ToFullString(), result[0].ToFullString());
    }

    #endregion // Tests

    #region FormatterPhaseTestsBase

    /// <inheritdoc/>
    protected override SyntaxNode ExecutePhase(SyntaxNode root, CancellationToken cancellationToken)
    {
        var context = new FormattingContext(Environment.NewLine);

        return new UsingDirectiveOrderingPhase().Execute(root, context, cancellationToken);
    }

    #endregion // FormatterPhaseTestsBase
}