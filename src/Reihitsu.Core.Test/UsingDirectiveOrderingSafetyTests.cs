using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.Core.Test;

/// <summary>
/// Contains unit tests for <see cref="UsingDirectiveOrderingSafety"/>: only a preprocessor directive anywhere in the trivia of
/// a block's directives makes the block unsafe, while a line of a delimited comment that starts with <c>#</c> is comment text
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
    /// Verifies that a directive in the leading trivia of the first using directive makes the block unsafe
    /// </summary>
    /// <param name="endOfLine">Line ending of the source</param>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public void NullableDirectiveBeforeFirstDirectiveIsUnsafe(string endOfLine)
    {
        const string source = """
                              #nullable enable
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