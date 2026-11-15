using System.Collections.Generic;
using System.Linq;

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
    /// Verifies that an argument list is partly wrapped when it is multi-line by its arguments and two arguments start on the same line: arguments that start on two lines, or an argument that spans lines next to another one, also when every argument stands below the opening parenthesis, under LF and CRLF line endings
    /// </summary>
    /// <param name="statement">Statement whose outermost invocation holds the argument list, written with LF line endings</param>
    [TestMethod]
    [Requirement("RH5102-001")]
    [DataRow("Method(1, 2,\n       3);")]
    [DataRow("Method(\n    1, 2,\n    3);")]
    [DataRow("Method(1,\n       2, 3);")]
    [DataRow("Method(1, () =>\n{\n});")]
    [DataRow("Method(\n    1, () =>\n    {\n    });")]
    [DataRow("Method(\n    1, @\"a\nb\");")]
    [DataRow("Method(\n    1, Inner(2,\n             3));")]
    [DataRow("Method(\n    1, 2 /* a\n    b */ + 3);")]
    public void IsPartlyWrappedReturnsTrueForArgumentsSharingALineInAMultiLineList(string statement)
    {
        Assert.IsTrue(SyntaxNodeUtilities.IsPartlyWrapped(GetArgumentList(statement).Arguments));
        Assert.IsTrue(SyntaxNodeUtilities.IsPartlyWrapped(GetArgumentList(statement.Replace("\n", "\r\n")).Arguments));
    }

    /// <summary>
    /// Verifies that an argument list whose arguments all start on one line, none of them spanning lines, is not partly wrapped, wherever its parentheses stand and whatever comment or directive keeps the first argument off the line of the opening parenthesis, and that a multi-line argument list in which every argument starts its own line is not partly wrapped either, however many lines an argument spans, under LF and CRLF line endings
    /// </summary>
    /// <param name="statement">Statement whose outermost invocation holds the argument list, written with LF line endings</param>
    [TestMethod]
    [Requirement("RH5102-001")]
    [DataRow("Method(1, 2, 3);")]
    [DataRow("Method(\n    1, 2);")]
    [DataRow("Method(1, 2\n      );")]
    [DataRow("Method(\n    1, 2\n      );")]
    [DataRow("Method(1, 2 // c\n      );")]
    [DataRow("Method( // c\n    1, 2);")]
    [DataRow("Method(\n    /* c */ 1, 2);")]
    [DataRow("Method( /* a\n    b */ 1, 2);")]
    [DataRow("Method(\n#pragma warning disable CS0168\n    1, 2);")]
    [DataRow("Method(\n#region R\n    1, 2\n#endregion // R\n      );")]
    [DataRow("Method(1,\n       2);")]
    [DataRow("Method(\n    1,\n    2);")]
    [DataRow("Method(1,\n       () =>\n       {\n       });")]
    public void IsPartlyWrappedReturnsFalseForArgumentsThatAllStartOnOneLineOrEachOnTheirOwn(string statement)
    {
        Assert.IsFalse(SyntaxNodeUtilities.IsPartlyWrapped(GetArgumentList(statement).Arguments));
        Assert.IsFalse(SyntaxNodeUtilities.IsPartlyWrapped(GetArgumentList(statement.Replace("\n", "\r\n")).Arguments));
    }

    /// <summary>
    /// Verifies that a list with fewer than two elements is never partly wrapped, also when its only element spans lines
    /// </summary>
    /// <param name="statement">Statement whose outermost invocation holds the argument list</param>
    [TestMethod]
    [Requirement("RH5102-001")]
    [DataRow("Method();")]
    [DataRow("Method(1);")]
    [DataRow("Method(() =>\n{\n});")]
    [DataRow("Method(\n    1\n      );")]
    public void IsPartlyWrappedReturnsFalseForFewerThanTwoArguments(string statement)
    {
        Assert.IsFalse(SyntaxNodeUtilities.IsPartlyWrapped(GetArgumentList(statement).Arguments));
    }

    /// <summary>
    /// Verifies that a parameter list is partly wrapped when its parameters start on two lines, or when a parameter whose type spans lines shares its line with another one, also below the opening parenthesis, under LF and CRLF line endings
    /// </summary>
    /// <param name="declaration">Method declaration, written with LF line endings</param>
    [TestMethod]
    [Requirement("RH5109-001")]
    [DataRow("void M(int a, int b,\n       int c)\n{\n}")]
    [DataRow("void M(\n    int a, int b,\n    int c)\n{\n}")]
    [DataRow("void M(\n    int a, Dictionary<int,\n                      int> b)\n{\n}")]
    public void IsPartlyWrappedReturnsTrueForParametersSharingALineInAMultiLineList(string declaration)
    {
        Assert.IsTrue(SyntaxNodeUtilities.IsPartlyWrapped(GetParameterList(declaration).Parameters));
        Assert.IsTrue(SyntaxNodeUtilities.IsPartlyWrapped(GetParameterList(declaration.Replace("\n", "\r\n")).Parameters));
    }

    /// <summary>
    /// Verifies that a parameter list whose parameters all start on one line, none of them spanning lines, or in which every parameter starts its own line, is not partly wrapped, under LF and CRLF line endings
    /// </summary>
    /// <param name="declaration">Method declaration, written with LF line endings</param>
    [TestMethod]
    [Requirement("RH5109-001")]
    [DataRow("void M(int a, int b)\n{\n}")]
    [DataRow("void M(\n    int a, int b)\n{\n}")]
    [DataRow("void M(int a, int b\n      )\n{\n}")]
    [DataRow("void M(int a, int b // c\n      )\n{\n}")]
    [DataRow("void M( // c\n    int a, int b)\n{\n}")]
    [DataRow("void M(\n#pragma warning disable CS0168\n    int a, int b)\n{\n}")]
    [DataRow("void M(int a,\n       int b)\n{\n}")]
    [DataRow("void M(int a,\n       Dictionary<int,\n                  int> b)\n{\n}")]
    public void IsPartlyWrappedReturnsFalseForParametersThatAllStartOnOneLineOrEachOnTheirOwn(string declaration)
    {
        Assert.IsFalse(SyntaxNodeUtilities.IsPartlyWrapped(GetParameterList(declaration).Parameters));
        Assert.IsFalse(SyntaxNodeUtilities.IsPartlyWrapped(GetParameterList(declaration.Replace("\n", "\r\n")).Parameters));
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
    /// Verifies that the leading-and-interior predicate ignores a comment trailing the node, so a rewrite that
    /// reproduces everything after the node verbatim is not withheld by it
    /// </summary>
    [TestMethod]
    public void LeadingAndInteriorContainsCommentOrDirectiveIgnoresTrailingComment()
    {
        Assert.IsFalse(SyntaxNodeUtilities.LeadingAndInteriorContainsCommentOrDirective(GetArgumentList("""
                                                                                                            Method("first",
                                                                                                                   "second") /* note */;
                                                                                                        """)));
    }

    /// <summary>
    /// Verifies that the leading-and-interior predicate still reports a comment in the node's leading trivia, which
    /// is the region such a rewrite can still reach
    /// </summary>
    [TestMethod]
    public void LeadingAndInteriorContainsCommentOrDirectiveReportsLeadingComment()
    {
        Assert.IsTrue(SyntaxNodeUtilities.LeadingAndInteriorContainsCommentOrDirective(GetArgumentList("""
                                                                                                           Method
                                                                                                               /* note */ ("first",
                                                                                                                           "second");
                                                                                                       """)));
    }

    /// <summary>
    /// Verifies that the leading-and-interior predicate still reports a comment inside the node's own span
    /// </summary>
    [TestMethod]
    public void LeadingAndInteriorContainsCommentOrDirectiveReportsInteriorComment()
    {
        Assert.IsTrue(SyntaxNodeUtilities.LeadingAndInteriorContainsCommentOrDirective(GetArgumentList("""
                                                                                                           Method("first", /* note */
                                                                                                                  "second");
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
                                                                                                    #if true
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

    /// <summary>
    /// Verifies that a directive in front of the group refuses the fold. It lies outside the group's span so
    /// that a documentation comment does not block, but a fold would move it off its own line
    /// </summary>
    [TestMethod]
    public void GroupInteriorContainsCommentOrDirectiveReportsDirectiveBeforeTheGroup()
    {
        Assert.IsTrue(SyntaxNodeUtilities.GroupInteriorContainsCommentOrDirective(GetAttributeLists("""
                                                                                                    #if true
                                                                                                    [Serializable]
                                                                                                    [Obsolete]
                                                                                                    #endif
                                                                                                    internal class TestClass;
                                                                                                    """)));
    }

    /// <summary>
    /// Verifies that a directive following the last sibling refuses the fold, because the fold rewrites the
    /// trivia after the group and would move the directive onto the merged line
    /// </summary>
    [TestMethod]
    public void GroupInteriorContainsCommentOrDirectiveReportsDirectiveAfterTheLastSibling()
    {
        Assert.IsTrue(SyntaxNodeUtilities.GroupInteriorContainsCommentOrDirective(GetAttributeLists("""
                                                                                                    [Serializable]
                                                                                                    [Obsolete]
                                                                                                    #pragma warning disable CS0618
                                                                                                    internal class TestClass;
                                                                                                    """)));
    }

    /// <summary>
    /// Verifies that a property with no attribute lists at all is single line when written on one line
    /// </summary>
    [TestMethod]
    public void IsSingleLineExcludingAttributeListsReturnsTrueForUnattributedSingleLineProperty()
    {
        Assert.IsTrue(SyntaxNodeUtilities.IsSingleLineExcludingAttributeLists(GetPropertyDeclaration("public int Value { get; set; }")));
    }

    /// <summary>
    /// Verifies that a property-level attribute on its own line does not make an otherwise single-line
    /// declaration look multi-line, which is the defect this predicate exists to close
    /// </summary>
    [TestMethod]
    public void IsSingleLineExcludingAttributeListsReturnsTrueWhenOnlyThePropertyAttributeIsOnItsOwnLine()
    {
        Assert.IsTrue(SyntaxNodeUtilities.IsSingleLineExcludingAttributeLists(GetPropertyDeclaration("""
                                                                                                     [Example]
                                                                                                     public int Value { get; set; }
                                                                                                     """)));
    }

    /// <summary>
    /// Verifies that a property whose declaration proper genuinely spans multiple lines is still reported as
    /// multi-line, even with a property-level attribute on its own line above it
    /// </summary>
    [TestMethod]
    public void IsSingleLineExcludingAttributeListsReturnsFalseForGenuinelyMultiLineDeclaration()
    {
        Assert.IsFalse(SyntaxNodeUtilities.IsSingleLineExcludingAttributeLists(GetPropertyDeclaration("""
                                                                                                      [Example]
                                                                                                      public int Value
                                                                                                      {
                                                                                                          get;
                                                                                                          set;
                                                                                                      }
                                                                                                      """)));
    }

    /// <summary>
    /// Verifies that a missing property declaration is not single line instead of throwing
    /// </summary>
    [TestMethod]
    public void IsSingleLineExcludingAttributeListsReturnsFalseForMissingNode()
    {
        Assert.IsFalse(SyntaxNodeUtilities.IsSingleLineExcludingAttributeLists(null));
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Parses the source and returns all of its attribute lists
    /// </summary>
    /// <param name="source">Source text</param>
    /// <returns>The attribute lists</returns>
    private static List<AttributeListSyntax> GetAttributeLists(string source)
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
    /// Parses the source and returns the first property declaration found in a wrapping class
    /// </summary>
    /// <param name="propertyDeclaration">Property declaration source text</param>
    /// <returns>The property declaration</returns>
    private static PropertyDeclarationSyntax GetPropertyDeclaration(string propertyDeclaration)
    {
        var source = $$"""
                       internal class ExampleAttribute : System.Attribute;

                       internal class TestClass
                       {
                       {{propertyDeclaration}}
                       }
                       """;

        return CoreSyntaxTestHelper.ParseCompilationUnit(source)
                                   .DescendantNodes()
                                   .OfType<PropertyDeclarationSyntax>()
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

    /// <summary>
    /// Parses the method declaration inside a wrapping class and returns its parameter list
    /// </summary>
    /// <param name="declaration">Method declaration source text</param>
    /// <returns>The parameter list</returns>
    private static ParameterListSyntax GetParameterList(string declaration)
    {
        var source = $$"""
                       using System.Collections.Generic;

                       internal class TestClass
                       {
                       {{declaration}}
                       }
                       """;

        return CoreSyntaxTestHelper.ParseCompilationUnit(source)
                                   .DescendantNodes()
                                   .OfType<MethodDeclarationSyntax>()
                                   .First()
                                   .ParameterList;
    }

    #endregion // Methods
}