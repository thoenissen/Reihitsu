using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.LineBreaks;

/// <summary>
/// The collapse guards must treat a documentation comment like any other comment. Guarding only
/// on <c>//</c> and <c>/* */</c> let the auto-property collapse run over a documentation
/// comment and delete it, and let the bracketed-argument collapse reshape a list it could not
/// join
/// </summary>
[TestClass]
public class DocumentationCommentCollapseGuardTests : FormatterTestsBase
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
                                                                   public int Value { get; set; }
                                                               }
                                                               """;

    /// <summary>
    /// Attribute lists wrapped in an active preprocessor directive, which the merge must not fold
    /// </summary>
    private const string DirectiveWrappedAttributeTestData = """
                                                             using System.Runtime.InteropServices;

                                                             internal class TestClass
                                                             {
                                                                 private static void Method(
                                                             #if true
                                                                     [In] [Out]
                                                             #endif
                                                                     int value)
                                                                 {
                                                                 }
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

    #region Tests

    /// <summary>
    /// Verifies that the auto-property collapse does not delete a documentation comment sitting in the
    /// accessor list
    /// </summary>
    [TestMethod]
    public void DocumentedAccessorListKeepsItsDocumentationComment()
    {
        var actual = FormatWithPipeline(DocumentedAccessorTestData, "\n");

        Assert.Contains("/// Getter", actual, "The documentation comment must survive the auto-property collapse.");
    }

    /// <summary>
    /// Verifies that the auto-property carrying a documented accessor is not collapsed onto one line
    /// </summary>
    [TestMethod]
    public void DocumentedAccessorListIsNotCollapsedToSingleLine()
    {
        var actual = FormatWithPipeline(DocumentedAccessorTestData, "\n");

        Assert.DoesNotContain("public int Value { get; set; }", actual, "The accessor list must not be collapsed across a documentation comment.");
    }

    /// <summary>
    /// Verifies that the documentation comment also survives on CRLF input
    /// </summary>
    [TestMethod]
    public void DocumentedAccessorListKeepsItsDocumentationCommentWithCrlf()
    {
        var actual = FormatWithPipeline(DocumentedAccessorTestData, "\r\n");

        Assert.Contains("/// Getter", actual, "The documentation comment must survive the auto-property collapse on CRLF input.");
    }

    /// <summary>
    /// Verifies that formatting a documented accessor list twice produces the same output, and that no phase run alone
    /// changes the output (FMT-PIPE-027)
    /// </summary>
    [TestMethod]
    public void DocumentedAccessorListFormattingIsIdempotent()
    {
        AssertSecondPassIsStable(DocumentedAccessorTestData);
    }

    /// <summary>
    /// Verifies that a bracketed argument list carrying a documentation comment is not collapsed onto one
    /// line, so the analyzer that predicts the collapse does not report a diagnostic its fix cannot clear
    /// </summary>
    [TestMethod]
    public void DocumentedBracketedArgumentListIsNotCollapsedToSingleLine()
    {
        var actual = FormatWithPipeline(DocumentedIndexerTestData, "\n");

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

        var actual = FormatWithPipeline(input, "\n");

        Assert.DoesNotContain("data[0]", actual, "An ordinary comment must block the collapse too.");
    }

    /// <summary>
    /// Verifies that formatting a documented bracketed argument list twice produces the same output, and that no phase
    /// run alone changes the output (FMT-PIPE-027)
    /// </summary>
    [TestMethod]
    public void DocumentedBracketedArgumentListFormattingIsIdempotent()
    {
        AssertSecondPassIsStable(DocumentedIndexerTestData);
    }

    /// <summary>
    /// Verifies that merging two attribute lists does not consume a documentation comment sitting between
    /// them
    /// </summary>
    [TestMethod]
    public void DocumentedAttributeListsKeepTheirDocumentationComment()
    {
        var actual = FormatWithPipeline(DocumentedAttributeMergeTestData, "\n");

        Assert.Contains("Explains the parameter attribute.", actual, "The documentation comment must survive the attribute-list merge.");
    }

    /// <summary>
    /// Verifies that two attribute lists separated by a documentation comment are not merged
    /// </summary>
    [TestMethod]
    public void DocumentedAttributeListsAreNotMerged()
    {
        var actual = FormatWithPipeline(DocumentedAttributeMergeTestData, "\n");

        Assert.DoesNotContain("[param: Obsolete, CLSCompliant(true)]", actual, "Attribute lists must not be merged across a documentation comment.");
    }

    /// <summary>
    /// Verifies that the documentation comment also survives the merge on CRLF input
    /// </summary>
    [TestMethod]
    public void DocumentedAttributeListsKeepTheirDocumentationCommentWithCrlf()
    {
        var actual = FormatWithPipeline(DocumentedAttributeMergeTestData, "\r\n");

        Assert.Contains("Explains the parameter attribute.", actual, "The documentation comment must survive the attribute-list merge on CRLF input.");
    }

    /// <summary>
    /// Verifies that formatting attribute lists separated by a documentation comment is idempotent, and that no phase run
    /// alone changes the output (FMT-PIPE-027)
    /// </summary>
    [TestMethod]
    public void DocumentedAttributeListFormattingIsIdempotent()
    {
        AssertSecondPassIsStable(DocumentedAttributeMergeTestData);
    }

    /// <summary>
    /// Verifies that the interior documentation comment survives formatting
    /// </summary>
    [TestMethod]
    public void DocumentedAttributeInteriorKeepsItsDocumentationComment()
    {
        var actual = FormatWithPipeline(DocumentedAttributeInteriorTestData, "\n");

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

        var actual = FormatWithPipeline(testData, "\n");

        Assert.DoesNotContain("[Serializable, Obsolete]", actual, "Both attribute lists must be split, whether or not the member is documented.");
        Assert.Contains("A documented class.", actual, "The member documentation comment must survive.");
    }

    /// <summary>
    /// Verifies that attribute lists wrapped in a preprocessor directive are not merged. The fold rewrites the
    /// trivia around the group, which would pull the <c>#endif</c> onto the merged line and produce source that
    /// no longer compiles (CS1040 / CS1027)
    /// </summary>
    [TestMethod]
    public void DirectiveWrappedAttributeListsAreNotMerged()
    {
        var actual = FormatWithPipeline(DirectiveWrappedAttributeTestData, "\n");

        Assert.DoesNotContain("[In, Out]", actual, "Attribute lists wrapped in a directive must not be merged.");
        Assert.Contains("\n#endif", actual, "The #endif directive must keep its own line.");
    }

    /// <summary>
    /// Verifies that the directive-wrapped shape also survives CRLF input
    /// </summary>
    [TestMethod]
    public void DirectiveWrappedAttributeListsAreNotMergedWithCrlf()
    {
        var actual = FormatWithPipeline(DirectiveWrappedAttributeTestData, "\r\n");

        Assert.DoesNotContain("[In, Out]", actual, "Attribute lists wrapped in a directive must not be merged on CRLF input.");
        Assert.Contains("\r\n#endif", actual, "The #endif directive must keep its own line on CRLF input.");
    }

    /// <summary>
    /// Verifies that formatting the directive-wrapped shape twice produces the same output. Before the guard
    /// covered the group boundaries the first pass emitted source the second pass could no longer parse. No phase run alone
    /// changes the output (FMT-PIPE-027)
    /// </summary>
    [TestMethod]
    public void DirectiveWrappedAttributeListFormattingIsIdempotent()
    {
        AssertSecondPassIsStable(DirectiveWrappedAttributeTestData);
    }

    #endregion // Tests
}