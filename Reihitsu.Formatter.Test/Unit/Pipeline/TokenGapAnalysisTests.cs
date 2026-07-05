using System.Collections.Generic;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline;

namespace Reihitsu.Formatter.Test.Unit.Pipeline;

/// <summary>
/// Tests for <see cref="TokenGapAnalysis"/>
/// </summary>
[TestClass]
public class TokenGapAnalysisTests
{
    #region Properties

    /// <summary>
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Between - HasLineBreak

    /// <summary>
    /// Verifies that two statements on the same line produce no line break
    /// </summary>
    [TestMethod]
    public void BetweenHasLineBreakReturnsFalseForSameLine()
    {
        // Arrange
        var (prev, next) = GetStatementBoundary("class C { void M() { int a = 0; int b = 0; } }", TestContext.CancellationToken);

        // Act
        var result = TokenGapAnalysis.Between(prev, next);

        // Assert
        Assert.IsFalse(result.HasLineBreak);
    }

    /// <summary>
    /// Verifies that tokens separated by an LF line ending produce a line break
    /// </summary>
    [TestMethod]
    public void BetweenHasLineBreakReturnsTrueForLf()
    {
        // Arrange
        var (prev, next) = GetStatementBoundary("class C\n{\n    void M()\n    {\n        int a = 0;\n        int b = 0;\n    }\n}", TestContext.CancellationToken);

        // Act
        var result = TokenGapAnalysis.Between(prev, next);

        // Assert
        Assert.IsTrue(result.HasLineBreak);
    }

    /// <summary>
    /// Verifies that tokens separated by a CR line ending produce a line break
    /// </summary>
    [TestMethod]
    public void BetweenHasLineBreakReturnsTrueForCr()
    {
        // Arrange
        var (prev, next) = GetStatementBoundary("class C\r{\r    void M()\r    {\r        int a = 0;\r        int b = 0;\r    }\r}", TestContext.CancellationToken);

        // Act
        var result = TokenGapAnalysis.Between(prev, next);

        // Assert
        Assert.IsTrue(result.HasLineBreak);
    }

    /// <summary>
    /// Verifies that tokens separated by a CRLF line ending produce a line break
    /// </summary>
    [TestMethod]
    public void BetweenHasLineBreakReturnsTrueForCrLf()
    {
        // Arrange
        var (prev, next) = GetStatementBoundary("class C\r\n{\r\n    void M()\r\n    {\r\n        int a = 0;\r\n        int b = 0;\r\n    }\r\n}", TestContext.CancellationToken);

        // Act
        var result = TokenGapAnalysis.Between(prev, next);

        // Assert
        Assert.IsTrue(result.HasLineBreak);
    }

    #endregion // Between - HasLineBreak

    #region Between - BlankLineCount

    /// <summary>
    /// Verifies that adjacent statements with no blank line produce a blank-line count of zero
    /// </summary>
    [TestMethod]
    public void BetweenBlankLineCountReturnsZeroForNoBlankLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     int a = 0;
                                     int b = 0;
                                 }
                             }
                             """;

        var (prev, next) = GetStatementBoundary(input, TestContext.CancellationToken);

        // Act
        var result = TokenGapAnalysis.Between(prev, next);

        // Assert
        Assert.AreEqual(0, result.BlankLineCount);
    }

    /// <summary>
    /// Verifies that one blank line between statements produces a blank-line count of one
    /// </summary>
    [TestMethod]
    public void BetweenBlankLineCountReturnsOneForOneBlankLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     int a = 0;

                                     int b = 0;
                                 }
                             }
                             """;

        var (prev, next) = GetStatementBoundary(input, TestContext.CancellationToken);

        // Act
        var result = TokenGapAnalysis.Between(prev, next);

        // Assert
        Assert.AreEqual(1, result.BlankLineCount);
    }

    /// <summary>
    /// Verifies that two blank lines between statements produce a blank-line count of two
    /// </summary>
    [TestMethod]
    public void BetweenBlankLineCountReturnsTwoForTwoBlankLines()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     int a = 0;


                                     int b = 0;
                                 }
                             }
                             """;

        var (prev, next) = GetStatementBoundary(input, TestContext.CancellationToken);

        // Act
        var result = TokenGapAnalysis.Between(prev, next);

        // Assert
        Assert.AreEqual(2, result.BlankLineCount);
    }

    /// <summary>
    /// Verifies that a blank line before a leading multi-line block comment counts as one blank line
    /// </summary>
    [TestMethod]
    public void BetweenBlankLineCountReturnsOneForLeadingMultiLineBlockComment()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var url = "";

                                     /* Explanation
                                        of the branch */
                                     var client = new object();
                                 }
                             }
                             """;

        var (prev, next) = GetStatementBoundary(input, TestContext.CancellationToken);

        // Act
        var result = TokenGapAnalysis.Between(prev, next);

        // Assert
        Assert.AreEqual(1, result.BlankLineCount);
    }

    /// <summary>
    /// Verifies that a blank line before a leading multi-line documentation comment counts as one blank line
    /// </summary>
    [TestMethod]
    public void BetweenBlankLineCountReturnsOneForLeadingMultiLineDocumentationComment()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var value = 0;

                                     /**
                                      * Helper.
                                      */
                                     void Local()
                                     {
                                     }
                                 }
                             }
                             """;

        var (prev, next) = GetStatementBoundary(input, TestContext.CancellationToken);

        // Act
        var result = TokenGapAnalysis.Between(prev, next);

        // Assert
        Assert.AreEqual(1, result.BlankLineCount);
    }

    /// <summary>
    /// Verifies that a comment-only line in the gap does not count as a blank line
    /// </summary>
    [TestMethod]
    public void BetweenBlankLineCountReturnsZeroForCommentOnlyGap()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     int a = 0;
                                     // comment
                                     int b = 0;
                                 }
                             }
                             """;

        var (prev, next) = GetStatementBoundary(input, TestContext.CancellationToken);

        // Act
        var result = TokenGapAnalysis.Between(prev, next);

        // Assert
        Assert.AreEqual(0, result.BlankLineCount);
    }

    /// <summary>
    /// Verifies that blank lines before a trailing comment still count correctly
    /// </summary>
    [TestMethod]
    public void BetweenBlankLineCountReturnsOneForBlankLineThenComment()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     int a = 0;

                                     // comment
                                     int b = 0;
                                 }
                             }
                             """;

        var (prev, next) = GetStatementBoundary(input, TestContext.CancellationToken);

        // Act
        var result = TokenGapAnalysis.Between(prev, next);

        // Assert
        Assert.AreEqual(1, result.BlankLineCount);
    }

    #endregion // Between - BlankLineCount

    #region OfLeadingTrivia

    /// <summary>
    /// Verifies that leading trivia with no blank line returns zero
    /// </summary>
    [TestMethod]
    public void OfLeadingTriviaBlankLineCountReturnsZeroForNoBlankLine()
    {
        // Arrange
        const string input = """
                             // comment
                             class C { }
                             """;

        var token = GetFirstClassToken(input, TestContext.CancellationToken);

        // Act
        var result = TokenGapAnalysis.OfLeadingTrivia(token, token.LeadingTrivia.Count);

        // Assert
        Assert.AreEqual(0, result.BlankLineCount);
    }

    /// <summary>
    /// Verifies that a blank line in leading trivia before the first token returns a count of one
    /// </summary>
    [TestMethod]
    public void OfLeadingTriviaBlankLineCountReturnsOneForOneBlankLine()
    {
        // Arrange
        const string input = "\r\n\r\nclass C { }";

        var token = GetFirstClassToken(input, TestContext.CancellationToken);

        // Act
        var result = TokenGapAnalysis.OfLeadingTrivia(token, token.LeadingTrivia.Count);

        // Assert
        Assert.AreEqual(1, result.BlankLineCount);
    }

    #endregion // OfLeadingTrivia

    #region OfTriviaRange

    /// <summary>
    /// Verifies that a whitespace-only trivia subrange with one blank line returns a count of one
    /// </summary>
    [TestMethod]
    public void OfTriviaRangeBlankLineCountReturnsOneForOneBlankLine()
    {
        // Arrange
        var triviaList = SyntaxFactory.TriviaList(SyntaxFactory.EndOfLine("\n"),
                                                  SyntaxFactory.EndOfLine("\n"),
                                                  SyntaxFactory.Whitespace("    "));

        // Act — analyse only the two EOL entries (indices 0 and 1)
        var result = TokenGapAnalysis.OfTriviaRange(triviaList, 0, 2);

        // Assert
        Assert.AreEqual(1, result.BlankLineCount);
    }

    /// <summary>
    /// Verifies that a whitespace-only trivia subrange without blank lines returns zero
    /// </summary>
    [TestMethod]
    public void OfTriviaRangeBlankLineCountReturnsZeroForNoBlankLine()
    {
        // Arrange
        var triviaList = SyntaxFactory.TriviaList(SyntaxFactory.EndOfLine("\n"),
                                                  SyntaxFactory.Whitespace("    "));

        // Act
        var result = TokenGapAnalysis.OfTriviaRange(triviaList, 0, triviaList.Count);

        // Assert
        Assert.AreEqual(0, result.BlankLineCount);
    }

    #endregion // OfTriviaRange

    #region Embedded comment line breaks

    /// <summary>
    /// Verifies that an LF embedded inside a block comment is detected as a line break and adds no blank line
    /// </summary>
    [TestMethod]
    public void OfTriviaRangeDetectsLfInsideBlockComment()
    {
        // Arrange
        var triviaList = SyntaxFactory.TriviaList(SyntaxFactory.Comment("/* first\nsecond */"));

        // Act
        var result = TokenGapAnalysis.OfTriviaRange(triviaList, 0, triviaList.Count);

        // Assert
        Assert.IsTrue(result.HasLineBreak);
        Assert.AreEqual(0, result.BlankLineCount);
    }

    /// <summary>
    /// Verifies that a lone CR embedded inside a block comment is detected as a line break and adds no blank line
    /// </summary>
    [TestMethod]
    public void OfTriviaRangeDetectsCrInsideBlockComment()
    {
        // Arrange
        var triviaList = SyntaxFactory.TriviaList(SyntaxFactory.Comment("/* first\rsecond */"));

        // Act
        var result = TokenGapAnalysis.OfTriviaRange(triviaList, 0, triviaList.Count);

        // Assert
        Assert.IsTrue(result.HasLineBreak);
        Assert.AreEqual(0, result.BlankLineCount);
    }

    /// <summary>
    /// Verifies that a CRLF embedded inside a block comment is treated as a single line break and adds no blank line
    /// </summary>
    [TestMethod]
    public void OfTriviaRangeDetectsCrLfInsideBlockComment()
    {
        // Arrange
        var triviaList = SyntaxFactory.TriviaList(SyntaxFactory.Comment("/* first\r\nsecond */"));

        // Act
        var result = TokenGapAnalysis.OfTriviaRange(triviaList, 0, triviaList.Count);

        // Assert
        Assert.IsTrue(result.HasLineBreak);
        Assert.AreEqual(0, result.BlankLineCount);
    }

    /// <summary>
    /// Verifies that a blank line embedded inside a block comment does not count as a blank line
    /// </summary>
    [TestMethod]
    public void OfTriviaRangeDoesNotCountBlankLineInsideBlockComment()
    {
        // Arrange
        var triviaList = SyntaxFactory.TriviaList(SyntaxFactory.Comment("/* first\n\nthird */"));

        // Act
        var result = TokenGapAnalysis.OfTriviaRange(triviaList, 0, triviaList.Count);

        // Assert
        Assert.IsTrue(result.HasLineBreak);
        Assert.AreEqual(0, result.BlankLineCount);
    }

    #endregion // Embedded comment line breaks

    #region IsBlankLine

    /// <summary>
    /// Verifies that a trivia line containing only an end-of-line is classified as blank
    /// </summary>
    [TestMethod]
    public void IsBlankLineReturnsTrueForEndOfLineOnly()
    {
        // Arrange
        var line = new List<SyntaxTrivia>
                   {
                       SyntaxFactory.EndOfLine("\n")
                   };

        // Act
        var result = TokenGapAnalysis.IsBlankLine(line);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Verifies that a trivia line containing whitespace and an end-of-line is classified as blank
    /// </summary>
    [TestMethod]
    public void IsBlankLineReturnsTrueForWhitespaceAndEndOfLine()
    {
        // Arrange
        var line = new List<SyntaxTrivia>
                   {
                       SyntaxFactory.Whitespace("    "),
                       SyntaxFactory.EndOfLine("\r\n")
                   };

        // Act
        var result = TokenGapAnalysis.IsBlankLine(line);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Verifies that a trivia line containing only whitespace without an end-of-line is not classified as blank
    /// </summary>
    [TestMethod]
    public void IsBlankLineReturnsFalseForWhitespaceWithoutEndOfLine()
    {
        // Arrange
        var line = new List<SyntaxTrivia>
                   {
                       SyntaxFactory.Whitespace("    ")
                   };

        // Act
        var result = TokenGapAnalysis.IsBlankLine(line);

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Verifies that a trivia line containing a comment is not classified as blank
    /// </summary>
    [TestMethod]
    public void IsBlankLineReturnsFalseForLineWithComment()
    {
        // Arrange
        var line = new List<SyntaxTrivia>
                   {
                       SyntaxFactory.Comment("// remark"),
                       SyntaxFactory.EndOfLine("\n")
                   };

        // Act
        var result = TokenGapAnalysis.IsBlankLine(line);

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Verifies that an empty trivia line is not classified as blank
    /// </summary>
    [TestMethod]
    public void IsBlankLineReturnsFalseForEmptyLine()
    {
        // Arrange
        var line = new List<SyntaxTrivia>();

        // Act
        var result = TokenGapAnalysis.IsBlankLine(line);

        // Assert
        Assert.IsFalse(result);
    }

    #endregion // IsBlankLine

    #region Helpers

    /// <summary>
    /// Gets the boundary tokens between the first two sibling statements in a method body
    /// </summary>
    /// <param name="input">The source text to parse</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The previous statement's last token and the current statement's first token</returns>
    private static (SyntaxToken PreviousToken, SyntaxToken CurrentToken) GetStatementBoundary(string input, CancellationToken cancellationToken)
    {
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: cancellationToken);
        var root = tree.GetRoot(cancellationToken);
        var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().Single();

        return (method.Body!.Statements[0].GetLastToken(), method.Body.Statements[1].GetFirstToken());
    }

    /// <summary>
    /// Gets the first token of the class keyword from the parsed source
    /// </summary>
    /// <param name="input">The source text to parse</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The class keyword token</returns>
    private static SyntaxToken GetFirstClassToken(string input, CancellationToken cancellationToken)
    {
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: cancellationToken);
        var root = tree.GetRoot(cancellationToken);

        return root.DescendantNodes().OfType<ClassDeclarationSyntax>().Single().Keyword;
    }

    #endregion // Helpers
}