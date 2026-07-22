using System;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.Core.Test;

/// <summary>
/// Contains unit tests for <see cref="SyntaxTriviaUtilities"/>
/// </summary>
[TestClass]
public class SyntaxTriviaUtilitiesTests
{
    #region Tests

    /// <summary>
    /// Verifies that a preprocessor directive is recognized as directive trivia
    /// </summary>
    [TestMethod]
    public void IsDirectiveOrDisabledTextTriviaReturnsTrueForDirective()
    {
        var trivia = GetFirstTrivia("#if DEBUG\nvar x = 1;\n#endif\n", SyntaxKind.IfDirectiveTrivia);

        Assert.IsTrue(SyntaxTriviaUtilities.IsDirectiveOrDisabledTextTrivia(trivia));
    }

    /// <summary>
    /// Verifies that disabled text is recognized as protected trivia
    /// </summary>
    [TestMethod]
    public void IsDirectiveOrDisabledTextTriviaReturnsTrueForDisabledText()
    {
        var trivia = GetFirstTrivia("#if UNDEFINED\nvar x = 1;\n#endif\n", SyntaxKind.DisabledTextTrivia);

        Assert.IsTrue(SyntaxTriviaUtilities.IsDirectiveOrDisabledTextTrivia(trivia));
    }

    /// <summary>
    /// Verifies that a comment is not treated as a directive or disabled text
    /// </summary>
    [TestMethod]
    public void IsDirectiveOrDisabledTextTriviaReturnsFalseForComment()
    {
        var trivia = GetFirstTrivia("// note\nvar x = 1;\n", SyntaxKind.SingleLineCommentTrivia);

        Assert.IsFalse(SyntaxTriviaUtilities.IsDirectiveOrDisabledTextTrivia(trivia));
    }

    /// <summary>
    /// Verifies that whitespace is not treated as a directive or disabled text
    /// </summary>
    [TestMethod]
    public void IsDirectiveOrDisabledTextTriviaReturnsFalseForWhitespace()
    {
        var trivia = GetFirstTrivia("    var x = 1;\n", SyntaxKind.WhitespaceTrivia);

        Assert.IsFalse(SyntaxTriviaUtilities.IsDirectiveOrDisabledTextTrivia(trivia));
    }

    /// <summary>
    /// Verifies that the first significant trivia index skips whitespace and end-of-line trivia
    /// </summary>
    [TestMethod]
    public void FindFirstSignificantTriviaIndexSkipsWhitespaceAndEndOfLineTrivia()
    {
        var trivia = SyntaxFactory.TriviaList(SyntaxFactory.Whitespace("    "),
                                              SyntaxFactory.EndOfLine("\n"),
                                              SyntaxFactory.Comment("// note"));

        Assert.AreEqual(2, SyntaxTriviaUtilities.FindFirstSignificantTriviaIndex(trivia));
    }

    /// <summary>
    /// Verifies that the first significant trivia index is zero when content comes first
    /// </summary>
    [TestMethod]
    public void FindFirstSignificantTriviaIndexReturnsZeroForLeadingContent()
    {
        var trivia = SyntaxFactory.TriviaList(SyntaxFactory.Comment("// note"), SyntaxFactory.EndOfLine("\n"));

        Assert.AreEqual(0, SyntaxTriviaUtilities.FindFirstSignificantTriviaIndex(trivia));
    }

    /// <summary>
    /// Verifies that no significant trivia index is returned for whitespace-only trivia
    /// </summary>
    [TestMethod]
    public void FindFirstSignificantTriviaIndexReturnsMinusOneForWhitespaceOnlyTrivia()
    {
        var trivia = SyntaxFactory.TriviaList(SyntaxFactory.Whitespace("    "), SyntaxFactory.EndOfLine("\n"));

        Assert.AreEqual(-1, SyntaxTriviaUtilities.FindFirstSignificantTriviaIndex(trivia));
    }

    /// <summary>
    /// Verifies that comments prevent adjacent tokens from being joined
    /// </summary>
    [TestMethod]
    public void ContainsUnjoinableTriviaReturnsTrueForComment()
    {
        var comment = GetFirstTrivia("// note\nvar x = 1;\n", SyntaxKind.SingleLineCommentTrivia);

        Assert.IsTrue(SyntaxTriviaUtilities.ContainsUnjoinableTrivia(SyntaxFactory.TriviaList(comment)));
    }

    /// <summary>
    /// Verifies that directives prevent adjacent tokens from being joined
    /// </summary>
    [TestMethod]
    public void ContainsUnjoinableTriviaReturnsTrueForDirective()
    {
        var directive = GetFirstTrivia("#if DEBUG\nvar x = 1;\n#endif\n", SyntaxKind.IfDirectiveTrivia);

        Assert.IsTrue(SyntaxTriviaUtilities.ContainsUnjoinableTrivia(SyntaxFactory.TriviaList(directive)));
    }

    /// <summary>
    /// Verifies that disabled text prevents adjacent tokens from being joined
    /// </summary>
    [TestMethod]
    public void ContainsUnjoinableTriviaReturnsTrueForDisabledText()
    {
        var disabledText = GetFirstTrivia("#if UNDEFINED\nvar x = 1;\n#endif\n", SyntaxKind.DisabledTextTrivia);

        Assert.IsTrue(SyntaxTriviaUtilities.ContainsUnjoinableTrivia(SyntaxFactory.TriviaList(disabledText)));
    }

    /// <summary>
    /// Verifies that whitespace alone does not prevent adjacent tokens from being joined
    /// </summary>
    [TestMethod]
    public void ContainsUnjoinableTriviaReturnsFalseForWhitespace()
    {
        var triviaList = SyntaxFactory.TriviaList(SyntaxFactory.Whitespace("    "), SyntaxFactory.EndOfLine("\n"));

        Assert.IsFalse(SyntaxTriviaUtilities.ContainsUnjoinableTrivia(triviaList));
    }

    /// <summary>
    /// Verifies that a directive as the last non-whitespace, non-end-of-line trivia is detected
    /// </summary>
    [TestMethod]
    public void IsPrecededByDirectiveReturnsTrueForDirectiveAsLastContent()
    {
        var directive = GetFirstTrivia("#if true\nvar x = 1;\n#endif\n", SyntaxKind.IfDirectiveTrivia);
        var trivia = new[] { SyntaxFactory.EndOfLine("\n"), directive, SyntaxFactory.Whitespace("    ") };

        Assert.IsTrue(SyntaxTriviaUtilities.IsPrecededByDirective(trivia));
    }

    /// <summary>
    /// Verifies that a comment as the last non-whitespace, non-end-of-line trivia is not treated as a directive
    /// </summary>
    [TestMethod]
    public void IsPrecededByDirectiveReturnsFalseForCommentAsLastContent()
    {
        var comment = GetFirstTrivia("// note\nvar x = 1;\n", SyntaxKind.SingleLineCommentTrivia);
        var trivia = new[] { SyntaxFactory.EndOfLine("\n"), comment, SyntaxFactory.EndOfLine("\n"), SyntaxFactory.Whitespace("    ") };

        Assert.IsFalse(SyntaxTriviaUtilities.IsPrecededByDirective(trivia));
    }

    /// <summary>
    /// Verifies that an empty trivia sequence is not treated as directive-preceded
    /// </summary>
    [TestMethod]
    public void IsPrecededByDirectiveReturnsFalseForEmptySequence()
    {
        Assert.IsFalse(SyntaxTriviaUtilities.IsPrecededByDirective(Enumerable.Empty<SyntaxTrivia>()));
    }

    /// <summary>
    /// Verifies that a directive as the first non-whitespace, non-end-of-line trivia is detected
    /// </summary>
    [TestMethod]
    public void IsFollowedByDirectiveReturnsTrueForDirectiveAsFirstContent()
    {
        var directive = GetFirstTrivia("#if true\nvar x = 1;\n#endif\n", SyntaxKind.IfDirectiveTrivia);
        var trivia = new[] { SyntaxFactory.Whitespace("    "), directive, SyntaxFactory.EndOfLine("\n") };

        Assert.IsTrue(SyntaxTriviaUtilities.IsFollowedByDirective(trivia));
    }

    /// <summary>
    /// Verifies that a comment as the first non-whitespace, non-end-of-line trivia is not treated as a directive
    /// </summary>
    [TestMethod]
    public void IsFollowedByDirectiveReturnsFalseForCommentAsFirstContent()
    {
        var comment = GetFirstTrivia("// note\nvar x = 1;\n", SyntaxKind.SingleLineCommentTrivia);
        var trivia = new[] { SyntaxFactory.EndOfLine("\n"), comment, SyntaxFactory.EndOfLine("\n") };

        Assert.IsFalse(SyntaxTriviaUtilities.IsFollowedByDirective(trivia));
    }

    /// <summary>
    /// Verifies that the insertion index stays at zero when no directive is present
    /// </summary>
    [TestMethod]
    public void FindIndexAfterLeadingDirectivesReturnsZeroWhenNoDirectivePresent()
    {
        var trivia = SyntaxFactory.TriviaList(SyntaxFactory.Whitespace("    "));

        Assert.AreEqual(0, SyntaxTriviaUtilities.FindIndexAfterLeadingDirectives(trivia));
    }

    /// <summary>
    /// Verifies that the insertion index lands immediately after a single leading directive
    /// </summary>
    [TestMethod]
    public void FindIndexAfterLeadingDirectivesReturnsIndexAfterSingleDirective()
    {
        var directive = GetFirstTrivia("#if true\nvar x = 1;\n#endif\n", SyntaxKind.IfDirectiveTrivia);
        var trivia = SyntaxFactory.TriviaList(SyntaxFactory.Whitespace("    "), directive, SyntaxFactory.Whitespace("    "));

        Assert.AreEqual(2, SyntaxTriviaUtilities.FindIndexAfterLeadingDirectives(trivia));
    }

    /// <summary>
    /// Verifies that the insertion index lands after the last of multiple leading directives
    /// </summary>
    [TestMethod]
    public void FindIndexAfterLeadingDirectivesReturnsIndexAfterLastOfMultipleDirectives()
    {
        var ifDirective = GetFirstTrivia("#if true\nvar x = 1;\n#endif\n", SyntaxKind.IfDirectiveTrivia);
        var endIfDirective = GetFirstTrivia("#if true\nvar x = 1;\n#endif\n", SyntaxKind.EndIfDirectiveTrivia);
        var trivia = SyntaxFactory.TriviaList(ifDirective, endIfDirective, SyntaxFactory.Whitespace("    "));

        Assert.AreEqual(2, SyntaxTriviaUtilities.FindIndexAfterLeadingDirectives(trivia));
    }

    /// <summary>
    /// Verifies that a position inside a single-line comment is recognized as non-formattable
    /// </summary>
    [TestMethod]
    public void IsInsideCommentOrDisabledTextReturnsTrueForSingleLineComment()
    {
        const string source = "// note\r\nvar x = 1;\r\n";
        var root = GetRoot(source);

        Assert.IsTrue(SyntaxTriviaUtilities.IsInsideCommentOrDisabledText(root, source.IndexOf("note", StringComparison.Ordinal)));
    }

    /// <summary>
    /// Verifies that a position inside a multi-line comment is recognized as non-formattable
    /// </summary>
    [TestMethod]
    public void IsInsideCommentOrDisabledTextReturnsTrueForMultiLineComment()
    {
        const string source = "/* first\r\n   note */\r\nvar x = 1;\r\n";
        var root = GetRoot(source);

        Assert.IsTrue(SyntaxTriviaUtilities.IsInsideCommentOrDisabledText(root, source.IndexOf("note", StringComparison.Ordinal)));
    }

    /// <summary>
    /// Verifies that a position inside a single-line documentation comment is recognized as non-formattable
    /// </summary>
    [TestMethod]
    public void IsInsideCommentOrDisabledTextReturnsTrueForSingleLineDocumentationComment()
    {
        const string source = "/// note\r\nvar x = 1;\r\n";
        var root = GetRoot(source);

        Assert.IsTrue(SyntaxTriviaUtilities.IsInsideCommentOrDisabledText(root, source.IndexOf("note", StringComparison.Ordinal)));
    }

    /// <summary>
    /// Verifies that a position inside a multi-line documentation comment is recognized as non-formattable
    /// </summary>
    [TestMethod]
    public void IsInsideCommentOrDisabledTextReturnsTrueForMultiLineDocumentationComment()
    {
        const string source = "/**\r\n * note\r\n */\r\nvar x = 1;\r\n";
        var root = GetRoot(source);

        Assert.IsTrue(SyntaxTriviaUtilities.IsInsideCommentOrDisabledText(root, source.IndexOf("note", StringComparison.Ordinal)));
    }

    /// <summary>
    /// Verifies that a position inside preprocessor-disabled text is recognized as non-formattable
    /// </summary>
    [TestMethod]
    public void IsInsideCommentOrDisabledTextReturnsTrueForDisabledText()
    {
        const string source = "#if UNDEFINED\r\nvar note = 1;\r\n#endif\r\n";
        var root = GetRoot(source);

        Assert.IsTrue(SyntaxTriviaUtilities.IsInsideCommentOrDisabledText(root, source.IndexOf("note", StringComparison.Ordinal)));
    }

    /// <summary>
    /// Verifies that a position inside an active preprocessor directive is not treated as non-formattable,
    /// since the formatter can rewrite that content
    /// </summary>
    [TestMethod]
    public void IsInsideCommentOrDisabledTextReturnsFalseForActiveDirective()
    {
        const string source = "#pragma warning disable CS0168\r\nvar x = 1;\r\n";
        var root = GetRoot(source);

        Assert.IsFalse(SyntaxTriviaUtilities.IsInsideCommentOrDisabledText(root, source.IndexOf("warning", StringComparison.Ordinal)));
    }

    /// <summary>
    /// Verifies that a position inside ordinary code is not treated as non-formattable
    /// </summary>
    [TestMethod]
    public void IsInsideCommentOrDisabledTextReturnsFalseForOrdinaryCode()
    {
        const string source = "var note = 1;\r\n";
        var root = GetRoot(source);

        Assert.IsFalse(SyntaxTriviaUtilities.IsInsideCommentOrDisabledText(root, source.IndexOf("note", StringComparison.Ordinal)));
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Parses the source and returns the first trivia of the requested kind
    /// </summary>
    /// <param name="source">Source text</param>
    /// <param name="kind">The trivia kind to locate</param>
    /// <returns>The first matching trivia</returns>
    private static SyntaxTrivia GetFirstTrivia(string source, SyntaxKind kind)
    {
        var root = GetRoot(source);

        return root.DescendantTrivia(descendIntoTrivia: true).First(trivia => trivia.IsKind(kind));
    }

    /// <summary>
    /// Parses the source and returns its syntax root
    /// </summary>
    /// <param name="source">Source text</param>
    /// <returns>The parsed syntax root</returns>
    private static SyntaxNode GetRoot(string source)
    {
        return CSharpSyntaxTree.ParseText(source).GetRoot();
    }

    #endregion // Methods
}