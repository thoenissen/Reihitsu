using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.Core.Test;

/// <summary>
/// Contains unit tests for <see cref="UsingDirectiveOrderingSafety"/>: a preprocessor directive between a block's directives or
/// between the tokens of one makes the block unsafe, and so does a conditional directive in front of the block, while any other
/// directive in front of the block does not, and a line of a delimited comment that starts with <c>#</c> is comment text
/// </summary>
[TestClass]
public class UsingDirectiveOrderingSafetyTests
{
    #region Tests

    /// <summary>
    /// Verifies that a file header comment before the first using directive, with a line that starts with <c>#</c>, does not
    /// make the compilation unit's block unsafe
    /// </summary>
    /// <param name="endOfLine">Line ending of the source</param>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public void HashLineInFileHeaderCommentIsSafe(string endOfLine)
    {
        const string source = """
                              /*
                              # Header
                              */
                              using System.Linq;
                              using System;
                              """;

        Assert.IsTrue(CanSafelyReorder(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a comment between two using directives, with an indented line that starts with <c>#</c>, does not make
    /// the block unsafe
    /// </summary>
    /// <param name="endOfLine">Line ending of the source</param>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public void IndentedHashLineInCommentBetweenDirectivesIsSafe(string endOfLine)
    {
        const string source = """
                              using System.Linq;
                              /*
                                  # Note
                              */
                              using System;
                              """;

        Assert.IsTrue(CanSafelyReorder(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a comment that starts behind a directive's semicolon and holds a line that starts with <c>#</c> does not
    /// make the block unsafe
    /// </summary>
    /// <param name="endOfLine">Line ending of the source</param>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public void HashLineInTrailingCommentIsSafe(string endOfLine)
    {
        const string source = """
                              using System.Linq; /*
                              # Note
                              */
                              using System;
                              """;

        Assert.IsTrue(CanSafelyReorder(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a delimited documentation comment before the first using directive, with a line that starts with
    /// <c>#</c>, does not make the block unsafe
    /// </summary>
    /// <param name="endOfLine">Line ending of the source</param>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public void HashLineInDelimitedDocumentationCommentIsSafe(string endOfLine)
    {
        const string source = """
                              /**
                              # Header
                              */
                              using System.Linq;
                              using System;
                              """;

        Assert.IsTrue(CanSafelyReorder(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a comment between the tokens of a using directive, with a line that starts with <c>#</c>, does not make
    /// the block unsafe
    /// </summary>
    /// <param name="endOfLine">Line ending of the source</param>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public void HashLineInCommentBetweenTokensOfDirectiveIsSafe(string endOfLine)
    {
        const string source = """
                              using System.Linq;
                              using /*
                              # Note
                              */ System;
                              """;

        Assert.IsTrue(CanSafelyReorder(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a comment with a line that starts with <c>#</c> before the first using directive of a block-scoped
    /// namespace does not make the namespace's block unsafe
    /// </summary>
    /// <param name="endOfLine">Line ending of the source</param>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public void HashLineInCommentOfBlockNamespaceIsSafe(string endOfLine)
    {
        const string source = """
                              namespace N
                              {
                                  /*
                                  # Header
                                  */
                                  using System.Linq;
                                  using System;
                              }
                              """;

        Assert.IsTrue(CanSafelyReorder(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a comment with a line that starts with <c>#</c> before the first using directive of a file-scoped
    /// namespace does not make the namespace's block unsafe
    /// </summary>
    /// <param name="endOfLine">Line ending of the source</param>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public void HashLineInCommentOfFileScopedNamespaceIsSafe(string endOfLine)
    {
        const string source = """
                              namespace N;

                              /*
                              # Header
                              */
                              using System.Linq;
                              using System;
                              """;

        Assert.IsTrue(CanSafelyReorder(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a commented-out conditional block of using directives is comment text and does not make the block unsafe
    /// </summary>
    /// <param name="endOfLine">Line ending of the source</param>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public void CommentedOutConditionalDirectivesAreSafe(string endOfLine)
    {
        const string source = """
                              /*
                              #if DEBUG
                              using System.Diagnostics;
                              #endif
                              */
                              using System.Linq;
                              using System;
                              """;

        Assert.IsTrue(CanSafelyReorder(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a <c>#pragma</c> directive between the tokens of a using directive makes the block unsafe
    /// </summary>
    /// <param name="endOfLine">Line ending of the source</param>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public void PragmaBetweenTokensOfDirectiveIsUnsafe(string endOfLine)
    {
        const string source = """
                              using System.Linq;
                              using System
                              #pragma warning disable CS0105
                              .Text;
                              using System;
                              """;

        Assert.IsFalse(CanSafelyReorder(source, endOfLine));
    }

    /// <summary>
    /// Verifies that an <c>#if</c> and its <c>#endif</c> between the tokens of one using directive make the block unsafe
    /// </summary>
    /// <param name="endOfLine">Line ending of the source</param>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public void ConditionalGroupBetweenTokensOfDirectiveIsUnsafe(string endOfLine)
    {
        const string source = """
                              using System.Linq;
                              using System
                              #if A
                              .Text
                              #endif
                              ;
                              """;

        Assert.IsFalse(CanSafelyReorder(source, endOfLine));
    }

    /// <summary>
    /// Verifies that an <c>#if</c> between the tokens of one using directive and its <c>#endif</c> between the tokens of the
    /// next make the block unsafe
    /// </summary>
    /// <param name="endOfLine">Line ending of the source</param>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public void ConditionalGroupAcrossTokensOfTwoDirectivesIsUnsafe(string endOfLine)
    {
        const string source = """
                              using System
                              #if !A
                              .Text;
                              using System.Collections
                              #endif
                              ;
                              using System.Buffers;
                              """;

        Assert.IsFalse(CanSafelyReorder(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a conditional group in the leading trivia of a using directive makes the block unsafe
    /// </summary>
    /// <param name="endOfLine">Line ending of the source</param>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public void ConditionalGroupBetweenDirectivesIsUnsafe(string endOfLine)
    {
        const string source = """
                              using System.Linq;
                              #if DEBUG
                              using System.Diagnostics;
                              #endif
                              using System;
                              """;

        Assert.IsFalse(CanSafelyReorder(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a <c>#nullable</c> directive in the leading trivia of the first token of the block's first directive stands in front of the block and does not make it unsafe
    /// </summary>
    /// <param name="endOfLine">Line ending of the source</param>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public void NullableDirectiveBeforeFirstDirectiveIsSafe(string endOfLine)
    {
        const string source = """
                              #nullable enable
                              using System.Linq;
                              using System;
                              """;

        Assert.IsTrue(CanSafelyReorder(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a <c>#nullable</c> directive with a blank line below it in front of the block does not make it unsafe
    /// </summary>
    /// <param name="endOfLine">Line ending of the source</param>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public void NullableDirectiveFollowedByBlankLineBeforeFirstDirectiveIsSafe(string endOfLine)
    {
        const string source = """
                              #nullable enable

                              using System.Linq;
                              using System;
                              """;

        Assert.IsTrue(CanSafelyReorder(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a <c>#pragma warning disable</c> directive in front of the block does not make it unsafe
    /// </summary>
    /// <param name="endOfLine">Line ending of the source</param>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public void PragmaDirectiveBeforeFirstDirectiveIsSafe(string endOfLine)
    {
        const string source = """
                              #pragma warning disable CS0105
                              using System.Linq;
                              using System;
                              """;

        Assert.IsTrue(CanSafelyReorder(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a <c>#define</c> directive in front of the block does not make it unsafe
    /// </summary>
    /// <param name="endOfLine">Line ending of the source</param>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public void DefineDirectiveBeforeFirstDirectiveIsSafe(string endOfLine)
    {
        const string source = """
                              #define TRACE_X
                              using System.Linq;
                              using System;
                              """;

        Assert.IsTrue(CanSafelyReorder(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a <c>#region</c> directive that opens the block, with its <c>#endregion</c> behind the block, does not make it unsafe
    /// </summary>
    /// <param name="endOfLine">Line ending of the source</param>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public void RegionDirectiveBeforeFirstDirectiveIsSafe(string endOfLine)
    {
        const string source = """
                              #region Usings
                              using System.Linq;
                              using System;
                              #endregion
                              """;

        Assert.IsTrue(CanSafelyReorder(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a comment header, a blank line, and two directives in front of the block, with a directive behind the block, do not make it unsafe
    /// </summary>
    /// <param name="endOfLine">Line ending of the source</param>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public void HeaderCommentAndTwoDirectivesBeforeFirstDirectiveAreSafe(string endOfLine)
    {
        const string source = """
                              // <copyright file="C.cs">
                              // Header
                              // </copyright>

                              #nullable enable
                              #pragma warning disable CS0105
                              using System.Linq;
                              using System;
                              #pragma warning restore CS0105
                              """;

        Assert.IsTrue(CanSafelyReorder(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a <c>#nullable</c> directive between an extern alias and the block's first directive stands in front of the block and does not make it unsafe
    /// </summary>
    /// <param name="endOfLine">Line ending of the source</param>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public void NullableDirectiveBehindExternAliasBeforeFirstDirectiveIsSafe(string endOfLine)
    {
        const string source = """
                              extern alias Foo;
                              #nullable enable
                              using System.Linq;
                              using System;
                              """;

        Assert.IsTrue(CanSafelyReorder(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a <c>#nullable</c> directive in front of a block whose first directive starts with <c>global</c> does not make it unsafe
    /// </summary>
    /// <param name="endOfLine">Line ending of the source</param>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public void NullableDirectiveBeforeFirstGlobalDirectiveIsSafe(string endOfLine)
    {
        const string source = """
                              #nullable enable
                              global using System.Linq;
                              global using System;
                              """;

        Assert.IsTrue(CanSafelyReorder(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a <c>#nullable</c> directive in front of the block of a namespace with a body does not make the namespace's block unsafe
    /// </summary>
    /// <param name="endOfLine">Line ending of the source</param>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public void NullableDirectiveBeforeFirstDirectiveOfBlockNamespaceIsSafe(string endOfLine)
    {
        const string source = """
                              namespace N
                              {
                                  #nullable enable
                                  using System.Linq;
                                  using System;
                              }
                              """;

        Assert.IsTrue(CanSafelyReorder(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a <c>#nullable</c> directive in front of the block of a file-scoped namespace does not make the namespace's block unsafe
    /// </summary>
    /// <param name="endOfLine">Line ending of the source</param>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public void NullableDirectiveBeforeFirstDirectiveOfFileScopedNamespaceIsSafe(string endOfLine)
    {
        const string source = """
                              namespace N;
                              #nullable enable
                              using System.Linq;
                              using System;
                              """;

        Assert.IsTrue(CanSafelyReorder(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a directive in front of the block does not keep a directive in the leading trivia of a later directive from making the block unsafe
    /// </summary>
    /// <param name="endOfLine">Line ending of the source</param>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public void HeaderDirectiveWithDirectiveInLeadingTriviaOfLaterDirectiveIsUnsafe(string endOfLine)
    {
        const string source = """
                              #nullable enable
                              using System.Linq;
                              #pragma warning disable CS0105
                              using System;
                              """;

        Assert.IsFalse(CanSafelyReorder(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a <c>#pragma</c> directive in the leading trivia of a later token of the block's first directive stands between that directive's tokens and makes the block unsafe
    /// </summary>
    /// <param name="endOfLine">Line ending of the source</param>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public void PragmaBetweenTokensOfFirstDirectiveIsUnsafe(string endOfLine)
    {
        const string source = """
                              using
                              #pragma warning disable CS0105
                                  System.Linq;
                              using System;
                              """;

        Assert.IsFalse(CanSafelyReorder(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a directive in front of the block does not keep a directive between the tokens of the block's first directive from making the block unsafe
    /// </summary>
    /// <param name="endOfLine">Line ending of the source</param>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public void HeaderDirectiveWithPragmaBetweenTokensOfFirstDirectiveIsUnsafe(string endOfLine)
    {
        const string source = """
                              #nullable enable
                              using
                              #pragma warning disable CS0105
                                  System.Linq;
                              using System;
                              """;

        Assert.IsFalse(CanSafelyReorder(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a conditional group that ends in front of the block, with its excluded code as disabled text in the leading trivia of the block's first directive, makes the block unsafe
    /// </summary>
    /// <param name="endOfLine">Line ending of the source</param>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public void ConditionalGroupBeforeFirstDirectiveIsUnsafe(string endOfLine)
    {
        const string source = """
                              #if DEBUG
                              using System.Diagnostics;
                              #endif
                              using System.Linq;
                              using System;
                              """;

        Assert.IsFalse(CanSafelyReorder(source, endOfLine));
    }

    /// <summary>
    /// Verifies that an <c>#if</c> in front of the block whose <c>#endif</c> stands behind the block makes the block unsafe
    /// </summary>
    /// <param name="endOfLine">Line ending of the source</param>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public void ConditionalGroupEnclosingTheBlockIsUnsafe(string endOfLine)
    {
        const string source = """
                              #if !LEGACY
                              using System.Linq;
                              using System;
                              #endif
                              """;

        Assert.IsFalse(CanSafelyReorder(source, endOfLine));
    }

    /// <summary>
    /// Verifies that an <c>#endif</c> in front of the block, whose <c>#if</c> stands in front of an extern alias, makes the block unsafe
    /// </summary>
    /// <param name="endOfLine">Line ending of the source</param>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public void EndIfDirectiveBeforeFirstDirectiveIsUnsafe(string endOfLine)
    {
        const string source = """
                              #if !A
                              extern alias Foo;
                              #endif
                              using System.Linq;
                              using System;
                              """;

        Assert.IsFalse(CanSafelyReorder(source, endOfLine));
    }

    /// <summary>
    /// Verifies that an <c>#else</c> and its <c>#endif</c> in front of the block, whose <c>#if</c> stands in front of an extern alias, make the block unsafe
    /// </summary>
    /// <param name="endOfLine">Line ending of the source</param>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public void ElseDirectiveBeforeFirstDirectiveIsUnsafe(string endOfLine)
    {
        const string source = """
                              #if !A
                              extern alias Foo;
                              #else
                              extern alias Bar;
                              #endif
                              using System.Linq;
                              using System;
                              """;

        Assert.IsFalse(CanSafelyReorder(source, endOfLine));
    }

    /// <summary>
    /// Verifies that an <c>#elif</c> and its <c>#endif</c> in front of the block, whose <c>#if</c> stands in front of an extern alias, make the block unsafe
    /// </summary>
    /// <param name="endOfLine">Line ending of the source</param>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public void ElifDirectiveBeforeFirstDirectiveIsUnsafe(string endOfLine)
    {
        const string source = """
                              #if !A
                              extern alias Foo;
                              #elif B
                              extern alias Bar;
                              #endif
                              using System.Linq;
                              using System;
                              """;

        Assert.IsFalse(CanSafelyReorder(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a directive in the leading trivia of a later using directive makes the block unsafe
    /// </summary>
    /// <param name="endOfLine">Line ending of the source</param>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public void DirectiveInLeadingTriviaOfLaterDirectiveIsUnsafe(string endOfLine)
    {
        const string source = """
                              using System.Linq;
                              #pragma warning disable CS0105
                              using System;
                              """;

        Assert.IsFalse(CanSafelyReorder(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a directive after the last using directive belongs to the next declaration and does not make the block
    /// unsafe
    /// </summary>
    /// <param name="endOfLine">Line ending of the source</param>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public void DirectiveAfterLastDirectiveIsSafe(string endOfLine)
    {
        const string source = """
                              using System.Linq;
                              using System;
                              #if DEBUG
                              class D
                              {
                              }
                              #endif
                              """;

        Assert.IsTrue(CanSafelyReorder(source, endOfLine));
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Parses the source with the given line ending and checks the first using block it holds: the compilation unit's, or
    /// else the one of its first namespace
    /// </summary>
    /// <param name="source">Source text</param>
    /// <param name="endOfLine">Line ending of the source</param>
    /// <returns>The result of <see cref="UsingDirectiveOrderingSafety.CanSafelyReorder"/> for the block</returns>
    private static bool CanSafelyReorder(string source, string endOfLine)
    {
        var compilationUnit = CoreSyntaxTestHelper.ParseCompilationUnit(source.Replace("\r\n", "\n").Replace("\n", endOfLine));

        Assert.DoesNotContain(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error, compilationUnit.GetDiagnostics(), "The fixture must parse without an error.");

        var usingDirectives = compilationUnit.Usings.Count > 0
                                  ? compilationUnit.Usings
                                  : compilationUnit.Members.OfType<BaseNamespaceDeclarationSyntax>().First().Usings;

        Assert.IsGreaterThanOrEqualTo(2, usingDirectives.Count, "The fixture must hold a block of at least two using directives.");

        return UsingDirectiveOrderingSafety.CanSafelyReorder(usingDirectives);
    }

    #endregion // Methods
}