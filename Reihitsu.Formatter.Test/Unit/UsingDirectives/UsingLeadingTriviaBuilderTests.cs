using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.UsingDirectives;

namespace Reihitsu.Formatter.Test.Unit.UsingDirectives;

/// <summary>
/// Tests for <see cref="UsingLeadingTriviaBuilder"/>, the leading-trivia reconstruction half of the
/// using-directive ordering phase. These pin how indentation, attached comments and group separators
/// are rebuilt independently of the ordering policy
/// </summary>
[TestClass]
public class UsingLeadingTriviaBuilderTests
{
    #region Properties

    /// <summary>
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that the whitespace prefix stops at the first significant trivia
    /// </summary>
    [TestMethod]
    public void GetWhitespacePrefixStopsAtFirstSignificantTrivia()
    {
        // Arrange
        var triviaList = SyntaxFactory.TriviaList(SyntaxFactory.Whitespace("    "),
                                                  SyntaxFactory.Comment("// note"),
                                                  SyntaxFactory.Whitespace("  "));

        // Act
        var result = UsingLeadingTriviaBuilder.GetWhitespacePrefix(triviaList);

        // Assert
        Assert.AreEqual("    ", result.ToFullString());
    }

    /// <summary>
    /// Verifies that an empty prefix is returned when the first trivia is significant
    /// </summary>
    [TestMethod]
    public void GetWhitespacePrefixReturnsEmptyWhenFirstTriviaIsSignificant()
    {
        // Arrange
        var triviaList = SyntaxFactory.TriviaList(SyntaxFactory.Comment("// note"));

        // Act
        var result = UsingLeadingTriviaBuilder.GetWhitespacePrefix(triviaList);

        // Assert
        Assert.AreEqual(string.Empty, result.ToFullString());
    }

    /// <summary>
    /// Verifies that the whole list is returned when it contains only whitespace and end-of-line trivia
    /// </summary>
    [TestMethod]
    public void GetWhitespacePrefixReturnsWholeListWhenAllWhitespace()
    {
        // Arrange
        var triviaList = SyntaxFactory.TriviaList(SyntaxFactory.EndOfLine("\n"), SyntaxFactory.Whitespace("    "));

        // Act
        var result = UsingLeadingTriviaBuilder.GetWhitespacePrefix(triviaList);

        // Assert
        Assert.AreEqual("\n    ", result.ToFullString());
    }

    /// <summary>
    /// Verifies that the first directive without significant trivia receives the shared prefix
    /// </summary>
    [TestMethod]
    public void CreateLeadingTriviaForFirstDirectiveReturnsPrefix()
    {
        // Arrange
        var usingDirective = ParseFirstUsing("using System;");
        var prefix = SyntaxFactory.TriviaList(SyntaxFactory.Whitespace("    "));

        // Act
        var result = UsingLeadingTriviaBuilder.CreateLeadingTrivia(usingDirective, prefix, startsNewGroup: false, isFirst: true, "\n");

        // Assert
        Assert.AreEqual("    ", result.ToFullString());
    }

    /// <summary>
    /// Verifies that a following directive in the same group keeps only its indentation
    /// </summary>
    [TestMethod]
    public void CreateLeadingTriviaForSameGroupReturnsIndentationOnly()
    {
        // Arrange
        var usingDirective = ParseFirstUsing("using System;").WithLeadingTrivia(SyntaxFactory.Whitespace("    "));

        // Act
        var result = UsingLeadingTriviaBuilder.CreateLeadingTrivia(usingDirective, SyntaxFactory.TriviaList(), startsNewGroup: false, isFirst: false, "\n");

        // Assert
        Assert.AreEqual("    ", result.ToFullString());
    }

    /// <summary>
    /// Verifies that a following directive starting a new group is prefixed with a blank line
    /// </summary>
    [TestMethod]
    public void CreateLeadingTriviaForNewGroupPrependsEndOfLine()
    {
        // Arrange
        var usingDirective = ParseFirstUsing("using System;").WithLeadingTrivia(SyntaxFactory.Whitespace("    "));

        // Act
        var result = UsingLeadingTriviaBuilder.CreateLeadingTrivia(usingDirective, SyntaxFactory.TriviaList(), startsNewGroup: true, isFirst: false, "\n");

        // Assert
        Assert.AreEqual("\n    ", result.ToFullString());
    }

    /// <summary>
    /// Verifies that an attached comment and its indentation survive a new-group move
    /// </summary>
    [TestMethod]
    public void CreateLeadingTriviaForNewGroupPreservesAttachedComment()
    {
        // Arrange
        var leadingTrivia = SyntaxFactory.TriviaList(SyntaxFactory.Whitespace("    "),
                                                     SyntaxFactory.Comment("// keep"),
                                                     SyntaxFactory.EndOfLine("\n"),
                                                     SyntaxFactory.Whitespace("    "));
        var usingDirective = ParseFirstUsing("using System;").WithLeadingTrivia(leadingTrivia);

        // Act
        var result = UsingLeadingTriviaBuilder.CreateLeadingTrivia(usingDirective, SyntaxFactory.TriviaList(), startsNewGroup: true, isFirst: false, "\n");

        // Assert
        Assert.AreEqual("\n    // keep\n    ", result.ToFullString());
    }

    /// <summary>
    /// Verifies that the first directive keeps its attached comment after the shared prefix
    /// </summary>
    [TestMethod]
    public void CreateLeadingTriviaForFirstDirectivePreservesAttachedComment()
    {
        // Arrange
        var leadingTrivia = SyntaxFactory.TriviaList(SyntaxFactory.Comment("// header"), SyntaxFactory.EndOfLine("\n"));
        var usingDirective = ParseFirstUsing("using System;").WithLeadingTrivia(leadingTrivia);
        var prefix = SyntaxFactory.TriviaList(SyntaxFactory.Whitespace("  "));

        // Act
        var result = UsingLeadingTriviaBuilder.CreateLeadingTrivia(usingDirective, prefix, startsNewGroup: false, isFirst: true, "\n");

        // Assert
        Assert.AreEqual("  // header\n", result.ToFullString());
    }

    /// <summary>
    /// Parses the given source and returns the first using directive
    /// </summary>
    /// <param name="code">The C# code to parse</param>
    /// <returns>The first using directive</returns>
    private UsingDirectiveSyntax ParseFirstUsing(string code)
    {
        var tree = CSharpSyntaxTree.ParseText(code, cancellationToken: TestContext.CancellationToken);
        var root = (CompilationUnitSyntax)tree.GetRoot(TestContext.CancellationToken);

        return root.Usings[0];
    }

    #endregion // Methods
}