using System.Threading;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline;

namespace Reihitsu.Formatter.Test.Regression.LineBreaks;

/// <summary>
/// Regression tests for issue #420: the collapse guards must treat a documentation comment like any other
/// comment. Guarding only on <c>//</c> and <c>/* */</c> let the auto-property collapse run over a
/// documentation comment and delete it, and let the bracketed-argument collapse reshape a list it could not
/// join
/// </summary>
[TestClass]
public class DocumentationCommentCollapseGuardTests
{
    #region Constants

    /// <summary>
    /// Auto-property whose accessor list carries a documentation comment
    /// </summary>
    private const string DocumentedAccessorTestData = """
                                                      internal class TestClass
                                                      {
                                                          public int Value
                                                          {
                                                              /// <summary>
                                                              /// Getter
                                                              /// </summary>
                                                              get;
                                                              set;
                                                          }
                                                      }
                                                      """;

    /// <summary>
    /// Two attribute lists separated by a documentation comment, which the merge must not consume
    /// </summary>
    private const string DocumentedAttributeMergeTestData = """
                                                            using System;

                                                            internal class TestClass
                                                            {
                                                                private static void Method([param: Obsolete]
                                                                                           /// <summary>Explains the parameter attribute.</summary>
                                                                                           [param: CLSCompliant(true)] int value)
                                                                {
                                                                }
                                                            }
                                                            """;

    /// <summary>
    /// Attribute list whose interior carries a multi-line documentation comment
    /// </summary>
    private const string DocumentedAttributeInteriorTestData = """
                                                               using System;

                                                               internal class TestClass
                                                               {
                                                                   [field: /** Marks the backing field. */ Obsolete]
                                                                   public int Value;
                                                               }
                                                               """;

    /// <summary>
    /// Indexer access whose bracketed argument list carries a documentation comment
    /// </summary>
    private const string DocumentedIndexerTestData = """
                                                     internal class TestClass
                                                     {
                                                         private static int Read(int[] data)
                                                         {
                                                             return data[
                                                                 /// <summary>
                                                                 /// index
                                                                 /// </summary>
                                                                 0];
                                                         }
                                                     }
                                                     """;

    #endregion // Constants

    #region Properties

    /// <summary>
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Formats the given source through the full pipeline using the requested end-of-line sequence
    /// </summary>
    /// <param name="input">The C# source text</param>
    /// <param name="endOfLine">The end-of-line sequence to use</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The formatted source text</returns>
    private static string Format(string input, string endOfLine, CancellationToken cancellationToken)
    {
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: cancellationToken);
        var context = new FormattingContext(endOfLine);
        var result = FormattingPipeline.Execute(tree.GetRoot(cancellationToken), context, cancellationToken);

        return result.ToFullString();
    }

    /// <summary>
    /// Normalizes every line break in the given text to CRLF regardless of the source line endings
    /// </summary>
    /// <param name="text">The text to normalize</param>
    /// <returns>The text using CRLF line endings</returns>
    private static string ToCrlf(string text)
    {
        return text.Replace("\r\n", "\n").Replace("\n", "\r\n");
    }

    #endregion // Methods

    #region Tests

    /// <summary>
    /// Verifies that the auto-property collapse does not delete a documentation comment sitting in the
    /// accessor list
    /// </summary>
    [TestMethod]
    public void DocumentedAccessorListKeepsItsDocumentationComment()
    {
        var actual = Format(DocumentedAccessorTestData, "\n", TestContext.CancellationToken);

        Assert.Contains("/// Getter", actual, "The documentation comment must survive the auto-property collapse.");
    }

    /// <summary>
    /// Verifies that the auto-property carrying a documented accessor is not collapsed onto one line
    /// </summary>
    [TestMethod]
    public void DocumentedAccessorListIsNotCollapsedToSingleLine()
    {
        var actual = Format(DocumentedAccessorTestData, "\n", TestContext.CancellationToken);

        Assert.DoesNotContain("public int Value { get; set; }", actual, "The accessor list must not be collapsed across a documentation comment.");
    }

    /// <summary>
    /// Verifies that the documentation comment also survives on CRLF input
    /// </summary>
    [TestMethod]
    public void DocumentedAccessorListKeepsItsDocumentationCommentWithCrlf()
    {
        var actual = Format(ToCrlf(DocumentedAccessorTestData), "\r\n", TestContext.CancellationToken);

        Assert.Contains("/// Getter", actual, "The documentation comment must survive the auto-property collapse on CRLF input.");
    }

    /// <summary>
    /// Verifies that formatting a documented accessor list twice produces the same output
    /// </summary>
    [TestMethod]
    public void DocumentedAccessorListFormattingIsIdempotent()
    {
        var first = Format(DocumentedAccessorTestData, "\n", TestContext.CancellationToken);
        var second = Format(first, "\n", TestContext.CancellationToken);

        Assert.AreEqual(first, second, "The second formatting pass must be a no-op.");
    }

    /// <summary>
    /// Verifies that a bracketed argument list carrying a documentation comment is not collapsed onto one
    /// line, so the analyzer that predicts the collapse does not report a diagnostic its fix cannot clear
    /// </summary>
    [TestMethod]
    public void DocumentedBracketedArgumentListIsNotCollapsedToSingleLine()
    {
        var actual = Format(DocumentedIndexerTestData, "\n", TestContext.CancellationToken);

        Assert.Contains("/// index", actual, "The documentation comment must survive.");
        Assert.DoesNotContain("data[0]", actual, "The bracketed argument list must not be collapsed across a documentation comment.");
    }

    /// <summary>
    /// Verifies that an ordinary comment blocks the same collapse, which is the behaviour the documentation
    /// comment is brought into line with
    /// </summary>
    [TestMethod]
    public void OrdinaryCommentedBracketedArgumentListIsNotCollapsedToSingleLine()
    {
        var input = DocumentedIndexerTestData.Replace("/// <summary>", "// index")
                                             .Replace("/// index", string.Empty)
                                             .Replace("/// </summary>", string.Empty);

        var actual = Format(input, "\n", TestContext.CancellationToken);

        Assert.DoesNotContain("data[0]", actual, "An ordinary comment must block the collapse too.");
    }

    /// <summary>
    /// Verifies that formatting a documented bracketed argument list twice produces the same output
    /// </summary>
    [TestMethod]
    public void DocumentedBracketedArgumentListFormattingIsIdempotent()
    {
        var first = Format(DocumentedIndexerTestData, "\n", TestContext.CancellationToken);
        var second = Format(first, "\n", TestContext.CancellationToken);

        Assert.AreEqual(first, second, "The second formatting pass must be a no-op.");
    }

    /// <summary>
    /// Verifies that merging two attribute lists does not consume a documentation comment sitting between
    /// them
    /// </summary>
    [TestMethod]
    public void DocumentedAttributeListsKeepTheirDocumentationComment()
    {
        var actual = Format(DocumentedAttributeMergeTestData, "\n", TestContext.CancellationToken);

        Assert.Contains("Explains the parameter attribute.", actual, "The documentation comment must survive the attribute-list merge.");
    }

    /// <summary>
    /// Verifies that two attribute lists separated by a documentation comment are not merged
    /// </summary>
    [TestMethod]
    public void DocumentedAttributeListsAreNotMerged()
    {
        var actual = Format(DocumentedAttributeMergeTestData, "\n", TestContext.CancellationToken);

        Assert.DoesNotContain("[param: Obsolete, CLSCompliant(true)]", actual, "Attribute lists must not be merged across a documentation comment.");
    }

    /// <summary>
    /// Verifies that the documentation comment also survives the merge on CRLF input
    /// </summary>
    [TestMethod]
    public void DocumentedAttributeListsKeepTheirDocumentationCommentWithCrlf()
    {
        var actual = Format(ToCrlf(DocumentedAttributeMergeTestData), "\r\n", TestContext.CancellationToken);

        Assert.Contains("Explains the parameter attribute.", actual, "The documentation comment must survive the attribute-list merge on CRLF input.");
    }

    /// <summary>
    /// Verifies that formatting attribute lists separated by a documentation comment is idempotent
    /// </summary>
    [TestMethod]
    public void DocumentedAttributeListFormattingIsIdempotent()
    {
        var first = Format(DocumentedAttributeMergeTestData, "\n", TestContext.CancellationToken);
        var second = Format(first, "\n", TestContext.CancellationToken);

        Assert.AreEqual(first, second, "The second formatting pass must be a no-op.");
    }

    /// <summary>
    /// Verifies that the interior documentation comment survives formatting
    /// </summary>
    [TestMethod]
    public void DocumentedAttributeInteriorKeepsItsDocumentationComment()
    {
        var actual = Format(DocumentedAttributeInteriorTestData, "\n", TestContext.CancellationToken);

        Assert.Contains("Marks the backing field.", actual, "The interior documentation comment must survive.");
    }

    /// <summary>
    /// Verifies that a member's own documentation comment does not gate the reshape of its attribute list.
    /// The comment sits in the leading trivia of the list's first token, so a full-span guard would treat a
    /// documented member differently from an identical undocumented one
    /// </summary>
    [TestMethod]
    public void DocumentedMemberAttributeListIsSplitLikeAnUndocumentedOne()
    {
        const string testData = """
                                using System;

                                /// <summary>
                                /// A documented class.
                                /// </summary>
                                [Serializable, Obsolete]
                                public class DocumentedClass
                                {
                                }

                                [Serializable, Obsolete]
                                public class UndocumentedClass
                                {
                                }
                                """;

        var actual = Format(testData, "\n", TestContext.CancellationToken);

        Assert.DoesNotContain("[Serializable, Obsolete]", actual, "Both attribute lists must be split, whether or not the member is documented.");
        Assert.Contains("A documented class.", actual, "The member documentation comment must survive.");
    }

    #endregion // Tests
}