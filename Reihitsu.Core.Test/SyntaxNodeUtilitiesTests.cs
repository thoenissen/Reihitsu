using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.Core.Test;

/// <summary>
/// Contains unit tests for <see cref="SyntaxNodeUtilities"/>
/// </summary>
[TestClass]
public class SyntaxNodeUtilitiesTests
{
    #region Tests

    /// <summary>
    /// Verifies that a node without comments or directives is not reported
    /// </summary>
    [TestMethod]
    public void HasCommentsOrDirectivesReturnsFalseForCleanNode()
    {
        Assert.IsFalse(SyntaxNodeUtilities.HasCommentsOrDirectives(GetArgumentList("""Method("first", "second");""")));
    }

    /// <summary>
    /// Verifies that a single-line comment is reported
    /// </summary>
    [TestMethod]
    public void HasCommentsOrDirectivesReturnsTrueForSingleLineComment()
    {
        Assert.IsTrue(SyntaxNodeUtilities.HasCommentsOrDirectives(GetArgumentList("""
                                                                                  Method("first", // note
                                                                                         "second");
                                                                                  """)));
    }

    /// <summary>
    /// Verifies that a preprocessor directive is reported
    /// </summary>
    [TestMethod]
    public void HasCommentsOrDirectivesReturnsTrueForDirective()
    {
        Assert.IsTrue(SyntaxNodeUtilities.HasCommentsOrDirectives(GetArgumentList("""
                                                                                  Method("first",
                                                                                  #if FEATURE
                                                                                  #endif
                                                                                         "second");
                                                                                  """)));
    }

    /// <summary>
    /// Verifies that a documentation comment is deliberately not reported, which is the divergence that makes
    /// <see cref="SyntaxNodeUtilities.ContainsJoinRefusingTrivia"/> a separate predicate
    /// </summary>
    [TestMethod]
    public void HasCommentsOrDirectivesReturnsFalseForDocumentationComment()
    {
        Assert.IsFalse(SyntaxNodeUtilities.HasCommentsOrDirectives(GetArgumentList("""
                                                                                   Method(
                                                                                       /// note
                                                                                       "first",
                                                                                       "second");
                                                                                   """)));
    }

    /// <summary>
    /// Verifies that a node without comments or directives does not refuse a join
    /// </summary>
    [TestMethod]
    public void ContainsJoinRefusingTriviaReturnsFalseForCleanNode()
    {
        Assert.IsFalse(SyntaxNodeUtilities.ContainsJoinRefusingTrivia(GetArgumentList("""Method("first", "second");""")));
    }

    /// <summary>
    /// Verifies that a single-line comment refuses a join
    /// </summary>
    [TestMethod]
    public void ContainsJoinRefusingTriviaReturnsTrueForSingleLineComment()
    {
        Assert.IsTrue(SyntaxNodeUtilities.ContainsJoinRefusingTrivia(GetArgumentList("""
                                                                                     Method("first", // note
                                                                                            "second");
                                                                                     """)));
    }

    /// <summary>
    /// Verifies that a multi-line comment refuses a join
    /// </summary>
    [TestMethod]
    public void ContainsJoinRefusingTriviaReturnsTrueForMultiLineComment()
    {
        Assert.IsTrue(SyntaxNodeUtilities.ContainsJoinRefusingTrivia(GetArgumentList("""
                                                                                     Method("first", /* note */
                                                                                            "second");
                                                                                     """)));
    }

    /// <summary>
    /// Verifies that a documentation comment refuses a join, matching the formatter's join guard, so a
    /// registration guard never offers a fix the formatter then declines
    /// </summary>
    [TestMethod]
    public void ContainsJoinRefusingTriviaReturnsTrueForDocumentationComment()
    {
        Assert.IsTrue(SyntaxNodeUtilities.ContainsJoinRefusingTrivia(GetArgumentList("""
                                                                                     Method(
                                                                                         /// note
                                                                                         "first",
                                                                                         "second");
                                                                                     """)));
    }

    /// <summary>
    /// Verifies that a preprocessor directive refuses a join
    /// </summary>
    [TestMethod]
    public void ContainsJoinRefusingTriviaReturnsTrueForDirective()
    {
        Assert.IsTrue(SyntaxNodeUtilities.ContainsJoinRefusingTrivia(GetArgumentList("""
                                                                                     Method("first",
                                                                                     #if FEATURE
                                                                                     #endif
                                                                                            "second");
                                                                                     """)));
    }

    /// <summary>
    /// Verifies that a node written on one line is single line
    /// </summary>
    [TestMethod]
    public void IsSingleLineReturnsTrueForSingleLineNode()
    {
        Assert.IsTrue(SyntaxNodeUtilities.IsSingleLine(GetArgumentList("""Method("first", "second");""")));
    }

    /// <summary>
    /// Verifies that a node spanning several lines is not single line
    /// </summary>
    [TestMethod]
    public void IsSingleLineReturnsFalseForMultiLineNode()
    {
        Assert.IsFalse(SyntaxNodeUtilities.IsSingleLine(GetArgumentList("""
                                                                        Method("first",
                                                                               "second");
                                                                        """)));
    }

    /// <summary>
    /// Verifies that a missing node is not single line instead of throwing
    /// </summary>
    [TestMethod]
    public void IsSingleLineReturnsFalseForMissingNode()
    {
        Assert.IsFalse(SyntaxNodeUtilities.IsSingleLine(null));
    }

    /// <summary>
    /// Verifies that a span contained in one line is reported as single line
    /// </summary>
    [TestMethod]
    public void IsSingleLineSpanReturnsTrueForSpanOnOneLine()
    {
        var argumentList = GetArgumentList("""Method("first", "second");""");

        Assert.IsTrue(SyntaxNodeUtilities.IsSingleLineSpan(argumentList.SyntaxTree, argumentList.Span));
    }

    /// <summary>
    /// Verifies that a span crossing a line break is not reported as single line
    /// </summary>
    [TestMethod]
    public void IsSingleLineSpanReturnsFalseForSpanCrossingLines()
    {
        var argumentList = GetArgumentList("""
                                           Method("first",
                                                  "second");
                                           """);

        Assert.IsFalse(SyntaxNodeUtilities.IsSingleLineSpan(argumentList.SyntaxTree, TextSpan.FromBounds(argumentList.SpanStart, argumentList.Span.End)));
    }

    /// <summary>
    /// Verifies that a sequence of single-line nodes is accepted
    /// </summary>
    [TestMethod]
    public void AreAllSingleLineReturnsTrueWhenEveryNodeIsSingleLine()
    {
        var argumentList = GetArgumentList("""
                                           Method("first",
                                                  "second");
                                           """);

        Assert.IsTrue(SyntaxNodeUtilities.AreAllSingleLine(argumentList.Arguments));
    }

    /// <summary>
    /// Verifies that a sequence containing a multi-line node is rejected
    /// </summary>
    [TestMethod]
    public void AreAllSingleLineReturnsFalseWhenANodeSpansSeveralLines()
    {
        var argumentList = GetArgumentList("""
                                           Method("first",
                                                  Inner("second",
                                                        "third"));
                                           """);

        Assert.IsFalse(SyntaxNodeUtilities.AreAllSingleLine(argumentList.Arguments));
    }

    /// <summary>
    /// Verifies that an empty sequence is accepted
    /// </summary>
    [TestMethod]
    public void AreAllSingleLineReturnsTrueForEmptySequence()
    {
        Assert.IsTrue(SyntaxNodeUtilities.AreAllSingleLine(GetArgumentList("Method();").Arguments));
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Parses the statement and returns the argument list of its outermost invocation
    /// </summary>
    /// <param name="statement">Statement source text</param>
    /// <returns>The argument list</returns>
    private static ArgumentListSyntax GetArgumentList(string statement)
    {
        var source = $$"""
                       internal class TestClass
                       {
                           private void TestMethod()
                           {
                       {{statement}}
                           }
                       }
                       """;

        return CoreSyntaxTestHelper.ParseCompilationUnit(source)
                                   .DescendantNodes()
                                   .OfType<InvocationExpressionSyntax>()
                                   .First()
                                   .ArgumentList;
    }

    #endregion // Methods
}