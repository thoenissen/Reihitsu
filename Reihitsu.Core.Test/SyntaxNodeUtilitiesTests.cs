using System.Collections.Generic;
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
    public void ContainsCommentOrDirectiveReturnsFalseForCleanNode()
    {
        Assert.IsFalse(SyntaxNodeUtilities.ContainsCommentOrDirective(GetArgumentList("""Method("first", "second");""")));
    }

    /// <summary>
    /// Verifies that a single-line comment is reported
    /// </summary>
    [TestMethod]
    public void ContainsCommentOrDirectiveReturnsTrueForSingleLineComment()
    {
        Assert.IsTrue(SyntaxNodeUtilities.ContainsCommentOrDirective(GetArgumentList("""
                                                                                     Method("first", // note
                                                                                            "second");
                                                                                     """)));
    }

    /// <summary>
    /// Verifies that a multi-line comment is reported
    /// </summary>
    [TestMethod]
    public void ContainsCommentOrDirectiveReturnsTrueForMultiLineComment()
    {
        Assert.IsTrue(SyntaxNodeUtilities.ContainsCommentOrDirective(GetArgumentList("""
                                                                                     Method("first", /* note */
                                                                                            "second");
                                                                                     """)));
    }

    /// <summary>
    /// Verifies that a documentation comment is reported, so a guard protecting a reshape never lets it run
    /// over one and never offers a fix the formatter then declines
    /// </summary>
    [TestMethod]
    public void ContainsCommentOrDirectiveReturnsTrueForDocumentationComment()
    {
        Assert.IsTrue(SyntaxNodeUtilities.ContainsCommentOrDirective(GetArgumentList("""
                                                                                     Method(
                                                                                         /// note
                                                                                         "first",
                                                                                         "second");
                                                                                     """)));
    }

    /// <summary>
    /// Verifies that a preprocessor directive is reported
    /// </summary>
    [TestMethod]
    public void ContainsCommentOrDirectiveReturnsTrueForDirective()
    {
        Assert.IsTrue(SyntaxNodeUtilities.ContainsCommentOrDirective(GetArgumentList("""
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

    /// <summary>
    /// Verifies that the full-span predicate also counts the documentation comment attached in front of the
    /// node, which is the scope that makes it wrong for a guard over a node's interior
    /// </summary>
    [TestMethod]
    public void ContainsCommentOrDirectiveCountsLeadingDocumentationComment()
    {
        Assert.IsTrue(SyntaxNodeUtilities.ContainsCommentOrDirective(GetAttributeList("""
                                                                                      /// <summary>Documented.</summary>
                                                                                      [Serializable, Obsolete]
                                                                                      internal class TestClass;
                                                                                      """)));
    }

    /// <summary>
    /// Verifies that the interior predicate ignores the documentation comment attached in front of the node,
    /// so a documented member is not treated differently from an undocumented one
    /// </summary>
    [TestMethod]
    public void InteriorContainsCommentOrDirectiveIgnoresLeadingDocumentationComment()
    {
        Assert.IsFalse(SyntaxNodeUtilities.InteriorContainsCommentOrDirective(GetAttributeList("""
                                                                                               /// <summary>Documented.</summary>
                                                                                               [Serializable, Obsolete]
                                                                                               internal class TestClass;
                                                                                               """)));
    }

    /// <summary>
    /// Verifies that the interior predicate still reports a comment inside the node's own span
    /// </summary>
    [TestMethod]
    public void InteriorContainsCommentOrDirectiveReportsInteriorComment()
    {
        Assert.IsTrue(SyntaxNodeUtilities.InteriorContainsCommentOrDirective(GetAttributeList("""
                                                                                              [Serializable, /* note */ Obsolete]
                                                                                              internal class TestClass;
                                                                                              """)));
    }

    /// <summary>
    /// Verifies that the group predicate ignores a comment in front of the first sibling, so a documented
    /// member is not treated differently from an undocumented one
    /// </summary>
    [TestMethod]
    public void GroupInteriorContainsCommentOrDirectiveIgnoresCommentBeforeTheGroup()
    {
        Assert.IsFalse(SyntaxNodeUtilities.GroupInteriorContainsCommentOrDirective(GetAttributeLists("""
                                                                                                     /// <summary>Documented.</summary>
                                                                                                     [Serializable]
                                                                                                     [Obsolete]
                                                                                                     internal class TestClass;
                                                                                                     """)));
    }

    /// <summary>
    /// Verifies that the group predicate reports a comment sitting between two siblings, which a fold would
    /// consume
    /// </summary>
    [TestMethod]
    public void GroupInteriorContainsCommentOrDirectiveReportsCommentBetweenSiblings()
    {
        Assert.IsTrue(SyntaxNodeUtilities.GroupInteriorContainsCommentOrDirective(GetAttributeLists("""
                                                                                                    [Serializable]
                                                                                                    // keep me
                                                                                                    [Obsolete]
                                                                                                    internal class TestClass;
                                                                                                    """)));
    }

    /// <summary>
    /// Verifies that the group predicate reports a comment trailing the last sibling, because a fold discards
    /// the trailing siblings whole, including the trivia that trails them
    /// </summary>
    [TestMethod]
    public void GroupInteriorContainsCommentOrDirectiveReportsCommentAfterTheLastSibling()
    {
        Assert.IsTrue(SyntaxNodeUtilities.GroupInteriorContainsCommentOrDirective(GetAttributeLists("""
                                                                                                    [Serializable]
                                                                                                    [Obsolete] // keep me
                                                                                                    internal class TestClass;
                                                                                                    """)));
    }

    /// <summary>
    /// Verifies that the group predicate reports a preprocessor directive between two siblings
    /// </summary>
    [TestMethod]
    public void GroupInteriorContainsCommentOrDirectiveReportsDirectiveBetweenSiblings()
    {
        Assert.IsTrue(SyntaxNodeUtilities.GroupInteriorContainsCommentOrDirective(GetAttributeLists("""
                                                                                                    [Serializable]
                                                                                                    #if FEATURE
                                                                                                    #endif
                                                                                                    [Obsolete]
                                                                                                    internal class TestClass;
                                                                                                    """)));
    }

    /// <summary>
    /// Verifies that the narrow predicate reports an ordinary comment
    /// </summary>
    [TestMethod]
    public void ContainsNonDocumentationCommentOrDirectiveReportsOrdinaryComment()
    {
        Assert.IsTrue(SyntaxNodeUtilities.ContainsNonDocumentationCommentOrDirective(GetArgumentList("""
                                                                                                     Method("first", // note
                                                                                                            "second");
                                                                                                     """)));
    }

    /// <summary>
    /// Verifies that the narrow predicate deliberately ignores a documentation comment, which is what lets a
    /// guard over a reshape that preserves one avoid withholding a fix that works
    /// </summary>
    [TestMethod]
    public void ContainsNonDocumentationCommentOrDirectiveIgnoresDocumentationComment()
    {
        Assert.IsFalse(SyntaxNodeUtilities.ContainsNonDocumentationCommentOrDirective(GetArgumentList("""
                                                                                                      Method(
                                                                                                          /// note
                                                                                                          "first",
                                                                                                          "second");
                                                                                                      """)));
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Parses the source and returns all of its attribute lists
    /// </summary>
    /// <param name="source">Source text</param>
    /// <returns>The attribute lists</returns>
    private static IReadOnlyList<AttributeListSyntax> GetAttributeLists(string source)
    {
        return CoreSyntaxTestHelper.ParseCompilationUnit("using System;\n\n" + source)
                                   .DescendantNodes()
                                   .OfType<AttributeListSyntax>()
                                   .ToList();
    }

    /// <summary>
    /// Parses the source and returns its first attribute list
    /// </summary>
    /// <param name="source">Source text</param>
    /// <returns>The attribute list</returns>
    private static AttributeListSyntax GetAttributeList(string source)
    {
        return CoreSyntaxTestHelper.ParseCompilationUnit("using System;\n\n" + source)
                                   .DescendantNodes()
                                   .OfType<AttributeListSyntax>()
                                   .First();
    }

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