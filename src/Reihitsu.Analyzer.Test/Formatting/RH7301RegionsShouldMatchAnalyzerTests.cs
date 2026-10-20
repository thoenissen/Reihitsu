using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Organization;
using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH7301RegionsShouldMatchAnalyzer"/> and <see cref="RH7301RegionsShouldMatchCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH7301RegionsShouldMatchAnalyzerTests : BatchCodeFixTestsBase<RH7301RegionsShouldMatchAnalyzer, RH7301RegionsShouldMatchCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying that mismatched region and endregion descriptions are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7301-001")]
    [Requirement("RH7301-003")]
    [Requirement("RH7301-004")]
    public async Task VerifyMismatchedRegionsAreDetectedAndFixed()
    {
        const string testData = """
                                using System;
                                using System.Collections.Generic;
                                using System.Text;

                                namespace Reihitsu.Analyzer.Test.Design.Resources
                                {
                                    internal class RH2001
                                    {
                                        #region Fields
                                        private bool _field;
                                        #endregion Fields
                                        #region Properties

                                        public bool Property { get { return _field; } }

                                        {|#0:#endregion // Fields|}

                                        #region Methods

                                        public bool GetValue() => _field;

                                        #endregion // Methods

                                        #region Outer 1

                                        #region Inner 1

                                        #endregion // Inner 1

                                        #endregion // Outer 1

                                        #region Outer 2

                                        #region Inner 2

                                        {|#1:#endregion|}

                                        #endregion // Outer 2

                                        #region Outer 3

                                        #region Inner 3

                                        #endregion // Inner 3

                                        {|#2:#endregion|}
                                    }
                                }
                                """;

        const string resultData = """
                                  using System;
                                  using System.Collections.Generic;
                                  using System.Text;

                                  namespace Reihitsu.Analyzer.Test.Design.Resources
                                  {
                                      internal class RH2001
                                      {
                                          #region Fields
                                          private bool _field;
                                          #endregion Fields
                                          #region Properties

                                          public bool Property { get { return _field; } }

                                          #endregion // Properties

                                          #region Methods

                                          public bool GetValue() => _field;

                                          #endregion // Methods

                                          #region Outer 1

                                          #region Inner 1

                                          #endregion // Inner 1

                                          #endregion // Outer 1

                                          #region Outer 2

                                          #region Inner 2

                                          #endregion // Inner 2

                                          #endregion // Outer 2

                                          #region Outer 3

                                          #region Inner 3

                                          #endregion // Inner 3

                                          #endregion // Outer 3
                                      }
                                  }
                                  """;

        await Verify(testData.Replace("\r\n", "\n"),
                     resultData.Replace("\r\n", "\n"),
                     Diagnostics(RH7301RegionsShouldMatchAnalyzer.DiagnosticId, AnalyzerResources.RH7301MessageFormat, 3));
    }

    /// <summary>
    /// Verifies that the synthesized endregion comment uses the document's detected CRLF end-of-line sequence
    /// instead of <see cref="System.Environment.NewLine"/>, so the fix does not introduce mixed line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7301-004")]
    public async Task VerifySynthesizedEndRegionCommentUsesDetectedCarriageReturnLineFeedEndOfLine()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    #region Methods

                                    public void Method()
                                    {
                                    }

                                    #endregion
                                }
                                """;

        var fixedSource = await ApplyCodeFixAsync(NormalizeToCarriageReturnLineFeed(testData));

        Assert.Contains(" // Methods\r\n", fixedSource);

        var sourceWithoutCarriageReturnLineFeeds = fixedSource.Replace("\r\n", string.Empty);

        Assert.IsFalse(sourceWithoutCarriageReturnLineFeeds.Contains('\r'));
        Assert.IsFalse(sourceWithoutCarriageReturnLineFeeds.Contains('\n'));
    }

    /// <summary>
    /// Verifies that the synthesized endregion comment preserves LF line endings without introducing CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7301-004")]
    public async Task VerifySynthesizedEndRegionCommentUsesDetectedLineFeedEndOfLine()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    #region Methods

                                    public void Method()
                                    {
                                    }

                                    #endregion
                                }
                                """;

        var fixedSource = await ApplyCodeFixAsync(testData.Replace("\r\n", "\n"));

        Assert.Contains(" // Methods\n", fixedSource);
        Assert.IsFalse(fixedSource.Contains('\r'));
    }

    /// <summary>
    /// Verifies that equivalent region descriptions with separator or trailing whitespace do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7301-002")]
    [Requirement("SHARED-REGDESC-001")]
    public async Task EquivalentRegionDescriptionsWithExtraWhitespaceDoNotProduceDiagnostics()
    {
        const string source = """
                              internal class TestClass
                              {
                                  #region  DoubleSeparator

                                  #endregion // DoubleSeparator

                                  #region TrailingSpace

                                  #endregion // TrailingSpace

                                  #region   OptionalComment

                                  #endregion   OptionalComment
                              }
                              """;
        var testData = source.Replace("#region TrailingSpace", "#region TrailingSpace ");

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that normalized-different region descriptions report exactly one diagnostic on the matching endregion
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7301-001")]
    [Requirement("RH7301-003")]
    public async Task DifferentNormalizedRegionDescriptionsReportAtMatchingEndRegion()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    #region Expected

                                    {|#0:#endregion // Actual|}
                                }
                                """;

        await Verify(testData, Diagnostics(RH7301RegionsShouldMatchAnalyzer.DiagnosticId, AnalyzerResources.RH7301MessageFormat));
    }

    /// <summary>
    /// Verifies that nameless regions do not register a code fix action
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7301-002")]
    public async Task NamelessRegionsDoNotRegisterCodeFixActions()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    #region

                                    #endregion
                                }
                                """;

        await Verify(testData);

        var actions = await GetCodeFixActionsAsync(testData,
                                                   RH7301RegionsShouldMatchAnalyzer.DiagnosticId,
                                                   root => root.DescendantTrivia(descendIntoTrivia: true)
                                                               .Select(trivia => trivia.GetStructure())
                                                               .OfType<EndRegionDirectiveTriviaSyntax>()
                                                               .Single()
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies that an empty start description and named end description report without offering an ineffective fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7301-001")]
    [Requirement("RH7301-005")]
    public async Task EmptyStartRegionDescriptionReportsWithoutCodeFixAction()
    {
        const string source = """
                              internal class TestClass
                              {
                                  #region

                                  #endregion // Named
                              }
                              """;
        var testData = source.Replace("#endregion // Named", "{|#0:#endregion // Named|}");

        await Verify(testData, Diagnostics(RH7301RegionsShouldMatchAnalyzer.DiagnosticId, AnalyzerResources.RH7301MessageFormat));

        var actions = await GetCodeFixActionsAsync(source,
                                                   RH7301RegionsShouldMatchAnalyzer.DiagnosticId,
                                                   root => root.DescendantTrivia(descendIntoTrivia: true)
                                                               .Select(trivia => trivia.GetStructure())
                                                               .OfType<EndRegionDirectiveTriviaSyntax>()
                                                               .Single()
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies that a named start description and empty end description are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7301-001")]
    [Requirement("RH7301-004")]
    public async Task NamedStartRegionAndBareEndRegionAreDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    #region Methods

                                    {|#0:#endregion|}
                                }
                                """;

        const string resultData = """
                                  internal class TestClass
                                  {
                                      #region Methods

                                      #endregion // Methods
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH7301RegionsShouldMatchAnalyzer.DiagnosticId, AnalyzerResources.RH7301MessageFormat));
    }

    /// <summary>
    /// Verifies that applying a region fix does not carry an extra start-region separator into the end-region comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7301-004")]
    [Requirement("SHARED-REGDESC-001")]
    public async Task RegionFixDoesNotCarryExtraStartSeparatorIntoEndRegionComment()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    #region  DoubleSeparator

                                    {|#0:#endregion // Wrong|}
                                }
                                """;

        const string resultData = """
                                  internal class TestClass
                                  {
                                      #region  DoubleSeparator

                                      #endregion // DoubleSeparator
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH7301RegionsShouldMatchAnalyzer.DiagnosticId, AnalyzerResources.RH7301MessageFormat));
    }

    /// <summary>
    /// Verifies that every end directive whose description differs from its region's is reported, also in an element body and when only the case differs
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7301-001")]
    [Requirement("RH7301-003")]
    public async Task EveryMismatchedEndRegionIsReported()
    {
        const string testData = """
                                class C
                                {
                                    #region Fields
                                    int _a;
                                    {|RH7301:#endregion // fields|}

                                    #region Methods
                                    void M()
                                    {
                                        #region Local
                                        var a = 1;
                                        {|RH7301:#endregion // Other|}
                                    }
                                    {|RH7301:#endregion|}
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that nested regions pair innermost first
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7301-001")]
    public async Task NestedRegionsPairInnermostFirst()
    {
        const string testData = """
                                class C
                                {
                                    #region Outer

                                    #region Inner
                                    int _a;
                                    {|RH7301:#endregion // Outer|}

                                    {|RH7301:#endregion // Inner|}
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a pair inside an excluded branch is checked too
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7301-001")]
    public async Task PairInAnExcludedBranchIsChecked()
    {
        const string testData = """
                                class C
                                {
                                #if false
                                    #region Fields

                                    {|RH7301:#endregion // Members|}
                                #endif
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that matching descriptions, in every form of the end directive and when both are empty, are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7301-002")]
    [Requirement("SHARED-REGDESC-001")]
    public async Task MatchingDescriptionsAreNotReported()
    {
        const string testData = """
                                class C
                                {
                                    #region Fields
                                    int _a;
                                    #endregion Fields

                                    #region Methods
                                    void M() { }
                                    #endregion //Methods

                                    #region
                                    int _b;
                                    #endregion
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the whitespace around a description and a leading double slash of the end directive are not part of the description
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-REGDESC-001")]
    public async Task WhitespaceAndLeadingDoubleSlashAreNotPartOfTheDescription()
    {
        const string testData = """
                                class C
                                {
                                    #region   Fields
                                    int _a;
                                    #endregion Fields

                                    #region Methods
                                    void M() { }
                                    #endregion //Methods

                                    #region Events
                                    #endregion   //   Events
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a block comment behind an end directive is part of its description
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-REGDESC-001")]
    public async Task BlockCommentBehindTheEndDirectiveIsPartOfTheDescription()
    {
        const string testData = """
                                class C
                                {
                                    #region Fields
                                    int _a;
                                    {|RH7301:#endregion /* Fields */|}
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the code fix rewrites the end directive to repeat its region's description
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7301-004")]
    public async Task CodeFixRepeatsTheRegionDescription()
    {
        const string testData = """
                                class C
                                {
                                    #region Fields
                                    int _a;
                                    {|RH7301:#endregion /* Fields */|}

                                    #region Methods
                                    void M() { }
                                    {|RH7301:#endregion|}
                                }
                                """;

        const string fixedData = """
                                 class C
                                 {
                                     #region Fields
                                     int _a;
                                     #endregion // Fields

                                     #region Methods
                                     void M() { }
                                     #endregion // Methods
                                 }
                                 """;

        await Verify(testData, fixedData);
    }

    /// <summary>
    /// Verifies that the code fix keeps the indentation and a carriage-return line-feed line break of the rewritten directive
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7301-004")]
    public async Task CodeFixKeepsIndentationAndCarriageReturnLineFeed()
    {
        const string testData = """
                                class C
                                {
                                    #region Fields
                                    int _a;
                                    #endregion /* Fields */
                                }
                                """;

        const string fixedData = """
                                 class C
                                 {
                                     #region Fields
                                     int _a;
                                     #endregion // Fields
                                 }
                                 """;

        var actual = await ApplyCodeFixAsync(NormalizeToCarriageReturnLineFeed(testData));

        Assert.AreEqual(NormalizeToCarriageReturnLineFeed(fixedData), actual);
    }

    /// <summary>
    /// Verifies that no code fix is offered when the region has no description
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7301-005")]
    public async Task NoCodeFixIsOfferedWhenTheRegionHasNoDescription()
    {
        const string testData = """
                                class C
                                {
                                    #region
                                    int _a;
                                    {|RH7301:#endregion // Stale|}
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    #region First

                                    {|#0:#endregion // WrongFirst|}

                                    #region Outer

                                    #region Inner

                                    {|#1:#endregion // WrongInner|}

                                    {|#2:#endregion // WrongOuter|}

                                #if false
                                    #region Inactive

                                    {|#3:#endregion // WrongInactive|}
                                #endif
                                }
                                """;

        const string resultData = """
                                  internal class TestClass
                                  {
                                      #region First

                                      #endregion // First

                                      #region Outer

                                      #region Inner

                                      #endregion // Inner

                                      #endregion // Outer

                                  #if false
                                      #region Inactive

                                      #endregion // Inactive
                                  #endif
                                  }
                                  """;

        // Verifies that nested, sequential and inactive-branch region pairs use LIFO matching and are independently corrected by Fix All without crossing pair boundaries
        return new FixAllScenario(testData,
                                  resultData,
                                  Diagnostics(RH7301RegionsShouldMatchAnalyzer.DiagnosticId, AnalyzerResources.RH7301MessageFormat, 4),
                                  static config => config.NumberOfFixAllIterations = 1);
    }

    #endregion // BatchCodeFixTestsBase
}