using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Organization;
using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH7310EmptyRegionsShouldBeRemovedAnalyzer"/> and <see cref="RH7310EmptyRegionsShouldBeRemovedCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH7310EmptyRegionsShouldBeRemovedAnalyzerTests : BatchCodeFixTestsBase<RH7310EmptyRegionsShouldBeRemovedAnalyzer, RH7310EmptyRegionsShouldBeRemovedCodeFixProvider>
{
    #region Constants

    /// <summary>
    /// Input of the SHARED-FIX-007 example of regions removed between two members: the removal of <c>Fields</c> leaves
    /// one blank line, the removal of <c>Methods</c> leaves two
    /// </summary>
    private const string RegionsBetweenMembersTestCode = """
                                                         class C
                                                         {
                                                             int _a;

                                                             {|RH7310:#region Fields|}
                                                             #endregion // Fields
                                                             int _b;

                                                             {|RH7310:#region Methods|}

                                                             #endregion // Methods

                                                             int _c;
                                                         }
                                                         """;

    /// <summary>
    /// Result of the SHARED-FIX-007 example of regions removed between two members
    /// </summary>
    private const string RegionsBetweenMembersFixedCode = """
                                                          class C
                                                          {
                                                              int _a;

                                                              int _b;

                                                              int _c;
                                                          }
                                                          """;

    #endregion // Constants

    #region Tests

    /// <summary>
    /// Verifies that a region containing only blank lines is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7310-001")]
    [Requirement("RH7310-003")]
    public async Task VerifyEmptyRegionIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    {|#0:#region Methods|}

                                    #endregion // Methods
                                }
                                """;

        await Verify(testData, Diagnostics(RH7310EmptyRegionsShouldBeRemovedAnalyzer.DiagnosticId, AnalyzerResources.RH7310MessageFormat));
    }

    /// <summary>
    /// Verifies that a region containing only a single-line comment is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7310-002")]
    public async Task VerifyRegionContainingOnlyCommentIsNotReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    #region Properties

                                    // TODO: add properties later

                                    #endregion // Properties
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a region whose body is only rationale comments is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7310-002")]
    public async Task VerifyRegionContainingRationaleCommentsIsNotReported()
    {
        const string testData = """
                                public class C
                                {
                                    #region Notes

                                    // rationale for the design below
                                    // (kept for the next maintainer)

                                    #endregion // Notes

                                    public void M()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a region containing only a multi-line comment is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7310-002")]
    public async Task VerifyRegionContainingMultiLineCommentIsNotReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    #region Notes

                                    /* rationale for the design below */

                                    #endregion // Notes
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that two adjacent empty regions are both reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7310-001")]
    public async Task VerifyMultipleEmptyRegionsAreReported()
    {
        const string testData = """
                                public class Widget
                                {
                                    {|#0:#region Methods|}

                                    #endregion // Methods

                                    {|#1:#region Properties|}

                                    #endregion // Properties
                                }
                                """;

        await Verify(testData, Diagnostics(RH7310EmptyRegionsShouldBeRemovedAnalyzer.DiagnosticId, AnalyzerResources.RH7310MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that only the innermost region is reported when an empty region nests another empty region
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7310-002")]
    public async Task VerifyNestedEmptyRegionReportsInnermostOnly()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    #region Outer

                                    {|#0:#region Inner|}

                                    #endregion // Inner

                                    #endregion // Outer
                                }
                                """;

        await Verify(testData, Diagnostics(RH7310EmptyRegionsShouldBeRemovedAnalyzer.DiagnosticId, AnalyzerResources.RH7310MessageFormat));
    }

    /// <summary>
    /// Verifies that a region containing a field declaration is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7310-002")]
    public async Task VerifyRegionContainingFieldIsNotReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    #region Fields

                                    private int _value;

                                    #endregion // Fields
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a region containing a method declaration is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7310-002")]
    public async Task VerifyRegionContainingMethodIsNotReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    #region Methods

                                    public void Run()
                                    {
                                    }

                                    #endregion // Methods
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a region containing a nested type declaration is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7310-002")]
    public async Task VerifyRegionContainingNestedTypeIsNotReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    #region Types

                                    private class Nested
                                    {
                                    }

                                    #endregion // Types
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a region wrapping a non-empty nested region is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7310-002")]
    public async Task VerifyRegionWrappingNonEmptyNestedRegionIsNotReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    #region Outer

                                    #region Inner

                                    private int _value;

                                    #endregion // Inner

                                    #endregion // Outer
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a region wrapping only conditionally-excluded code is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7310-002")]
    public async Task VerifyRegionWrappingDisabledCodeIsNotReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    #region Methods
                                #if false
                                    public void Run()
                                    {
                                    }
                                #endif
                                    #endregion // Methods
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a region wrapping only a non-region directive is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7310-002")]
    public async Task VerifyRegionWrappingDirectiveIsNotReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    #region Nullable
                                #nullable enable
                                    #endregion // Nullable
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an empty region inside a method body is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7310-002")]
    public async Task VerifyEmptyRegionInsideMethodBodyIsNotReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private void Run()
                                    {
                                        #region Body

                                        #endregion // Body
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an empty region is removed by the code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyEmptyRegionIsRemoved()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    {|#0:#region Methods|}

                                    #endregion // Methods
                                }
                                """;

        const string resultData = """
                                  internal class TestClass
                                  {
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH7310EmptyRegionsShouldBeRemovedAnalyzer.DiagnosticId, AnalyzerResources.RH7310MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment-bearing region and its comments are preserved when an adjacent empty region is removed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentBearingRegionIsPreservedWhenRemovingEmptyRegion()
    {
        const string testData = """
                                public class Widget
                                {
                                    {|#0:#region Methods|}

                                    #endregion // Methods

                                    #region Properties

                                    // TODO: add properties later

                                    #endregion // Properties
                                }
                                """;

        const string resultData = """
                                  public class Widget
                                  {
                                      #region Properties

                                      // TODO: add properties later

                                      #endregion // Properties
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH7310EmptyRegionsShouldBeRemovedAnalyzer.DiagnosticId, AnalyzerResources.RH7310MessageFormat));
    }

    /// <summary>
    /// Verifies that only the empty region is removed while populated regions are preserved
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyPopulatedRegionsArePreservedWhenRemovingEmptyRegion()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    #region Fields

                                    private int _value;

                                    #endregion // Fields

                                    {|#0:#region Methods|}

                                    #endregion // Methods

                                    #region Properties

                                    public int Value => _value;

                                    #endregion // Properties
                                }
                                """;

        const string resultData = """
                                  internal class TestClass
                                  {
                                      #region Fields

                                      private int _value;

                                      #endregion // Fields

                                      #region Properties

                                      public int Value => _value;

                                      #endregion // Properties
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH7310EmptyRegionsShouldBeRemovedAnalyzer.DiagnosticId, AnalyzerResources.RH7310MessageFormat));
    }

    /// <summary>
    /// Verifies that nested empty regions collapse into an empty type after repeated application
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNestedEmptyRegionsAreRemoved()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    #region Outer

                                    {|#0:#region Inner|}

                                    #endregion // Inner

                                    #endregion // Outer
                                }
                                """;

        const string resultData = """
                                  internal class TestClass
                                  {
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     onConfigure: config =>
                                  {
                                      config.NumberOfIncrementalIterations = 2;
                                      config.NumberOfFixAllIterations = 2;
                                  },
                     Diagnostics(RH7310EmptyRegionsShouldBeRemovedAnalyzer.DiagnosticId, AnalyzerResources.RH7310MessageFormat));
    }

    /// <summary>
    /// Verifies that regions holding only whitespace and line breaks are reported on their region directive, also at the top level of a file
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7310-001")]
    [Requirement("RH7310-003")]
    [Requirement("SHARED-TYPEREGION-005")]
    public async Task RegionsWithNothingInsideAreReported()
    {
        const string testData = """
                                {|RH7310:#region Types|}

                                #endregion

                                class C
                                {
                                    int _a;

                                    {|RH7310:#region Methods|}

                                    #endregion // Methods

                                    {|RH7310:#region Events|}
                                    #endregion

                                    int _b;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that regions holding a comment, a nested region, a directive, or excluded code, and a region inside executable code, are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7310-002")]
    [Requirement("SHARED-TYPEREGION-005")]
    public async Task RegionsWithContentOrInsideExecutableCodeAreNotReported()
    {
        const string testData = """
                                class C
                                {
                                    #region Properties

                                    // Added later

                                    #endregion // Properties

                                    #region Outer

                                    {|RH7310:#region Inner|}

                                    #endregion // Inner

                                    #endregion // Outer

                                    #region Fields

                                #pragma warning disable CS0169

                                    #endregion // Fields

                                    #region Legacy

                                #if false
                                    void Old()
                                    {
                                    }
                                #endif

                                    #endregion // Legacy

                                    void M()
                                    {
                                        #region Body
                                        #endregion
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the code fix removes an empty region between two sibling members and the blank lines after it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7310-004")]
    [Requirement("SHARED-FIX-007")]
    public async Task CodeFixRemovesTheRegionAndTheBlankLinesAfterIt()
    {
        const string testData = """
                                class C
                                {
                                    int _a;

                                    {|RH7310:#region Methods|}

                                    #endregion // Methods

                                    int _b;
                                }
                                """;

        const string fixedData = """
                                 class C
                                 {
                                     int _a;

                                     int _b;
                                 }
                                 """;

        await Verify(testData, fixedData);
    }

    /// <summary>
    /// Verifies that the code fix removes the region and the blank lines after it with their carriage-return line-feed line breaks and leaves every other line unchanged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7310-004")]
    [Requirement("SHARED-FIX-008")]
    public async Task CodeFixRemovesTheRegionWithCarriageReturnLineFeed()
    {
        const string testData = """
                                class C
                                {
                                    int _a;

                                    #region Methods

                                    #endregion // Methods

                                    int _b;
                                }
                                """;

        const string fixedData = """
                                 class C
                                 {
                                     int _a;

                                     int _b;
                                 }
                                 """;

        var actual = await ApplyCodeFixAsync(NormalizeToCarriageReturnLineFeed(testData));

        Assert.AreEqual(NormalizeToCarriageReturnLineFeed(fixedData), actual);
    }

    /// <summary>
    /// Verifies that removing a region between two members keeps the single blank line the removal leaves, removes one
    /// of the two blank lines another removal leaves, and leaves code that formatting does not change
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-007")]
    public async Task CodeFixKeepsASingleBlankLineAndRemovesADoubledOneBetweenMembers()
    {
        await Verify(RegionsBetweenMembersTestCode, RegionsBetweenMembersFixedCode);

        AssertFormattingChangesNothing(RegionsBetweenMembersFixedCode);
    }

    /// <summary>
    /// Verifies that removing a region between two members keeps the single blank line the removal leaves and removes
    /// one of the two blank lines another removal leaves, with CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-007")]
    public async Task CodeFixKeepsASingleBlankLineAndRemovesADoubledOneBetweenMembersWithCarriageReturnLineFeed()
    {
        await Verify(NormalizeToCarriageReturnLineFeed(RegionsBetweenMembersTestCode), NormalizeToCarriageReturnLineFeed(RegionsBetweenMembersFixedCode));

        AssertFormattingChangesNothing(NormalizeToCarriageReturnLineFeed(RegionsBetweenMembersFixedCode));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testData = """
                                public class Widget
                                {
                                    {|#0:#region Methods|}

                                    #endregion // Methods

                                    {|#1:#region Properties|}

                                    #endregion // Properties
                                }
                                """;

        const string resultData = """
                                  public class Widget
                                  {
                                  }
                                  """;

        // Two adjacent empty region pairs, so removing the first shifts the span of the second. The document is
        // the one VerifyMultipleEmptyRegionsAreReported already asserts both diagnostics on; the expected result
        // follows the single-removal behaviour of VerifyEmptyRegionIsRemoved, which takes the separating blank
        // lines with the region pair
        return new FixAllScenario(testData,
                                  resultData,
                                  Diagnostics(RH7310EmptyRegionsShouldBeRemovedAnalyzer.DiagnosticId, AnalyzerResources.RH7310MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}