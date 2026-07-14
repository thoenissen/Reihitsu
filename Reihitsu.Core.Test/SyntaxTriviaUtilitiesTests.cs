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
        var root = CSharpSyntaxTree.ParseText(source).GetRoot();

        return root.DescendantTrivia(descendIntoTrivia: true).First(trivia => trivia.IsKind(kind));
    }

    #endregion // Methods
}