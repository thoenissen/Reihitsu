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
    /// separator instead of a single line break, when the using block is the only content in the file.
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
    /// last directive away from the last position.
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
    /// into the next reordered directive when the block has no terminating newline.
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
    /// joining the line of a predecessor whose own trailing trivia carried no line break.
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
    /// trailing trivia moves.
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
    /// Verifies that a reorder splits directives that share one physical line even when their block never
    /// had a line break, and that the split-off directive gains no indentation because the scope's first
    /// directive does not start its own line either
    /// </summary>
    [TestMethod]
    public void DirectivesSharingOneLineWithNoLineBreakAnywhereInTheBlockAreSplit()
    {
        // Arrange
        const string input = "namespace N { using System.Linq; using System.Collections.Generic; }";
        var expected = $"namespace N {{ using System.Collections.Generic;{Environment.NewLine}using System.Linq; }}";

        // Assert
        Assert.AreEqual(expected, ApplyPhase(input));
    }

    /// <summary>
    /// Verifies that reordering a directive into the last position, where the block's own closing brace
    /// shares its line, preserves the space before that brace instead of gluing the directive to it, while
    /// the directive that now precedes it is split onto its own line at the scope's indentation
    /// </summary>
    [TestMethod]
    public void LastDirectiveSharingItsLineWithTheClosingBraceKeepsTheSpaceBeforeIt()
    {
        // Arrange
        const string input = "namespace N\n{\n    using System.Linq;\n    using System.Collections.Generic; }";
        var expected = $"namespace N\n{{\n    using System.Collections.Generic;{Environment.NewLine}    using System.Linq; }}";

        // Assert
        Assert.AreEqual(expected, ApplyPhase(input));
    }

    /// <summary>
    /// Verifies that reordering a directive with a trailing single-line comment into the last position,
    /// where the block's own terminator has no line break of its own, still terminates that comment
    /// instead of letting it absorb whatever the transplanted block terminator appends after it.
    /// </summary>
    [TestMethod]
    public void CommentOnDirectiveMovedToLastPositionIsTerminatedBeforeTheBlockTerminatorIsAppended()
    {
        // Arrange
        const string input = "namespace N { using System.Linq; // keep\nusing System.Collections.Generic; }";
        var expected = $"namespace N {{ using System.Collections.Generic;{Environment.NewLine}using System.Linq; // keep\n }}";

        // Assert
        Assert.AreEqual(expected, ApplyPhase(input));
    }

    /// <summary>
    /// Verifies that reordering a directive with a trailing single-line comment into the last position
    /// does not duplicate the line break when the block's own terminator already starts with one, which
    /// would otherwise insert a spurious blank line.
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
    /// Verifies that reordering a directive with a trailing single-line comment into the last position
    /// does not duplicate the line break when the block's own terminator contains one that is not its
    /// first trivia — trailing whitespace ahead of the terminator's own line break must not be mistaken
    /// for a terminator that needs a manually inserted break of its own, which would otherwise insert a
    /// spurious blank line and break idempotency.
    /// </summary>
    [TestMethod]
    public void CommentOnDirectiveMovedToLastPositionDoesNotDuplicateALineBreakThatIsNotTheBlockTerminatorsFirstTrivia()
    {
        // Arrange
        const string input = "using System.Linq; // keep\nusing System.Collections.Generic;  \n\nclass C;";
        const string expected = "using System.Collections.Generic;  \nusing System.Linq; // keep  \n\nclass C;";

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
    /// Verifies that ordered same-group directives separated only by a space are split onto separate lines
    /// without leaving the space behind as trailing whitespace
    /// </summary>
    [TestMethod]
    public void OrderedSameGroupDirectivesSeparatedBySpaceAreSplit()
    {
        // Arrange
        const string input = "using System; using System.Linq;";
        var expected = $"using System;{Environment.NewLine}using System.Linq;";

        // Assert
        Assert.AreEqual(expected, ApplyPhase(input));
    }

    /// <summary>
    /// Verifies that ordered same-group directives separated only by a tab are split onto separate lines
    /// </summary>
    [TestMethod]
    public void OrderedSameGroupDirectivesSeparatedByTabAreSplit()
    {
        // Arrange
        const string input = "using System;\tusing System.Linq;";
        var expected = $"using System;{Environment.NewLine}using System.Linq;";

        // Assert
        Assert.AreEqual(expected, ApplyPhase(input));
    }

    /// <summary>
    /// Verifies that a block comment between two directives on one line stays on the preceding directive's
    /// line when the directives are split
    /// </summary>
    [TestMethod]
    public void BlockCommentBetweenDirectivesOnOneLineStaysWithThePrecedingDirective()
    {
        // Arrange
        const string input = "using System; /* keep */ using System.Linq;";
        var expected = $"using System; /* keep */{Environment.NewLine}using System.Linq;";

        // Assert
        Assert.AreEqual(expected, ApplyPhase(input));
    }

    /// <summary>
    /// Verifies that a block comment spanning lines already places the following same-group directive on a
    /// later line, so no additional line break is inserted
    /// </summary>
    [TestMethod]
    public void SameGroupDirectiveAfterMultiLineBlockCommentIsNotSplit()
    {
        // Arrange
        const string input = "using System; /* first\n   second */ using System.Linq;";

        // Assert
        Assert.AreEqual(input, ApplyPhase(input));
    }

    /// <summary>
    /// Verifies that a cross-group directive after a block comment spanning lines is split behind the comment
    /// so that its group separator forms a blank line
    /// </summary>
    [TestMethod]
    public void CrossGroupDirectiveAfterMultiLineBlockCommentIsSplitWithBlankLine()
    {
        // Arrange
        const string input = "using System; /* first\n   second */ using Microsoft.Win32;";
        var expected = $"using System; /* first\n   second */{Environment.NewLine}{Environment.NewLine}using Microsoft.Win32;";

        // Assert
        Assert.AreEqual(expected, ApplyPhase(input));
    }

    /// <summary>
    /// Verifies that whitespace ahead of an existing line break is not treated as a same-line gap
    /// </summary>
    [TestMethod]
    public void OrderedDirectivesOnSeparateLinesKeepTheirTrivia()
    {
        // Arrange
        const string input = "using System;  \nusing System.Linq;";

        // Assert
        Assert.AreEqual(input, ApplyPhase(input));
    }

    /// <summary>
    /// Verifies that a single-line documentation comment behind a same-group directive already ends that
    /// directive's line, so no additional line break is inserted
    /// </summary>
    [TestMethod]
    public void SameGroupDirectiveAfterSingleLineDocumentationCommentIsNotSplit()
    {
        // Arrange
        const string input = "using System; /// core\nusing System.Linq;";

        // Assert
        Assert.AreEqual(input, ApplyPhase(input));
    }

    /// <summary>
    /// Verifies that a directive split off behind a block comment that precedes the scope's first directive
    /// receives the indentation of that directive's line
    /// </summary>
    [TestMethod]
    public void DirectiveSplitBehindCommentedFirstDirectiveUsesTheLineIndentation()
    {
        // Arrange
        const string input = "namespace N\n{\n    /* core */ using System; using System.Linq;\n}";
        var expected = $"namespace N\n{{\n    /* core */ using System;{Environment.NewLine}    using System.Linq;\n}}";

        // Assert
        Assert.AreEqual(expected, ApplyPhase(input));
    }

    /// <summary>
    /// Verifies that a directive split off below an unindented header comment receives the indentation of
    /// the first directive's line rather than the header's
    /// </summary>
    [TestMethod]
    public void DirectiveSplitBelowUnindentedHeaderCommentUsesTheDirectiveIndentation()
    {
        // Arrange
        const string input = "namespace N\n{\n// Header\n    using System; using Microsoft.Win32;\n}";
        var expected = $"namespace N\n{{\n// Header\n    using System;{Environment.NewLine}{Environment.NewLine}    using Microsoft.Win32;\n}}";

        // Assert
        Assert.AreEqual(expected, ApplyPhase(input));
    }

    /// <summary>
    /// Verifies that a directive that shared a line and is reordered behind a directive that already ends its
    /// line receives the scope's indentation
    /// </summary>
    [TestMethod]
    public void ReorderedSameLineDirectiveBehindTerminatedPredecessorUsesTheScopeIndentation()
    {
        // Arrange
        const string input = "namespace N\n{\n    using System; using System.Text;\n    using System.IO;\n}";
        var expected = $"namespace N\n{{\n    using System;{Environment.NewLine}    using System.IO;\n    using System.Text;\n}}";

        // Assert
        Assert.AreEqual(expected, ApplyPhase(input));
    }

    /// <summary>
    /// Verifies that a directive that shared a line and is reordered into a new group receives the scope's
    /// indentation behind its group separator
    /// </summary>
    [TestMethod]
    public void ReorderedSameLineDirectiveStartingNewGroupUsesTheScopeIndentation()
    {
        // Arrange
        const string input = "namespace N\n{\n    using System; using Microsoft.Win32;\n    using System.IO;\n}";
        var expected = $"namespace N\n{{\n    using System;{Environment.NewLine}    using System.IO;\n{Environment.NewLine}    using Microsoft.Win32;\n}}";

        // Assert
        Assert.AreEqual(expected, ApplyPhase(input));
    }

    /// <summary>
    /// Verifies that a cross-group pair on one line is split with the blank-line group separator in one pass
    /// instead of receiving only a single line break
    /// </summary>
    [TestMethod]
    public void CrossGroupDirectivesSeparatedBySpaceAreSplitWithBlankLine()
    {
        // Arrange
        const string input = "using System; using Microsoft.Win32;";
        var expected = $"using System;{Environment.NewLine}{Environment.NewLine}using Microsoft.Win32;";

        // Assert
        Assert.AreEqual(expected, ApplyPhase(input));
    }

    /// <summary>
    /// Verifies that a directive split off inside a block namespace receives the indentation of the scope's
    /// first directive
    /// </summary>
    [TestMethod]
    public void DirectiveSplitInsideNamespaceUsesTheScopeIndentation()
    {
        // Arrange
        const string input = "namespace N\n{\n    using System; using System.Linq;\n}";
        var expected = $"namespace N\n{{\n    using System;{Environment.NewLine}    using System.Linq;\n}}";

        // Assert
        Assert.AreEqual(expected, ApplyPhase(input));
    }

    /// <summary>
    /// Verifies that a directive-bearing block that cannot be reordered still has its same-line directives
    /// split, without reordering them and without touching the directive
    /// </summary>
    [TestMethod]
    public void DirectiveBearingBlockSplitsSameLineDirectivesWithoutReordering()
    {
        // Arrange
        const string input = "using System.Linq; using System;\n#pragma warning disable CS8019\nusing System.IO;";
        var expected = $"using System.Linq;{Environment.NewLine}using System;\n#pragma warning disable CS8019\nusing System.IO;";

        // Assert
        Assert.AreEqual(expected, ApplyPhase(input));
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