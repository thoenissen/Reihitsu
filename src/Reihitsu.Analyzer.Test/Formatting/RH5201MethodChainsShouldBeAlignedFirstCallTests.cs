using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5201MethodChainsShouldBeAlignedAnalyzer"/> and <see cref="RH5201MethodChainsShouldBeAlignedCodeFixProvider"/>
/// on the first invoked link of a wrapped chain whose root line does not reach it, and on a link behind the closing
/// <c>*/</c> of a comment that starts its own line in the anchor column
/// </summary>
[TestClass]
public class RH5201MethodChainsShouldBeAlignedFirstCallTests : BatchCodeFixTestsBase<RH5201MethodChainsShouldBeAlignedAnalyzer, RH5201MethodChainsShouldBeAlignedCodeFixProvider>
{
    #region Constants

    /// <summary>
    /// Class around the single-chain bodies of this test class; <c>{0}</c> stands for the statements of <c>M</c>
    /// </summary>
    private const string ChainClassTemplate = """
                                              #nullable enable

                                              class C
                                              {
                                                  C Next { get; set; } = null!;
                                                  C? Opt { get; set; }
                                                  C Self() => this;

                                                  void M(C c)
                                                  {
                                              {0}
                                                  }
                                              }
                                              """;

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Places the given statements into the body of <c>M</c> of <see cref="ChainClassTemplate"/> and writes every line break
    /// as <paramref name="endOfLine"/>
    /// </summary>
    /// <param name="body">Statements of <c>M</c>, indented for the method body</param>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>The source text</returns>
    private static string InChainClass(string body, string endOfLine)
    {
        return ConvertEndOfLine(ChainClassTemplate.Replace("{0}", body), endOfLine);
    }

    /// <summary>
    /// Verifies the diagnostics marked in the source and that the fix, applied one diagnostic at a time and by Fix All,
    /// gives the expected text
    /// </summary>
    /// <param name="source">Source text with <c>{|#n:...|}</c> markup for every expected diagnostic</param>
    /// <param name="fixedSource">Expected fixed source text</param>
    /// <param name="diagnosticCount">Number of expected diagnostics</param>
    /// <param name="incrementalIterations">Number of single-diagnostic fix applications until no diagnostic is left</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task VerifyFix(string source, string fixedSource, int diagnosticCount, int incrementalIterations)
    {
        await Verify(source,
                     fixedSource,
                     config =>
                     {
                         config.NumberOfIncrementalIterations = incrementalIterations;
                         config.NumberOfFixAllIterations = 1;
                     },
                     Diagnostics(RH5201MethodChainsShouldBeAlignedAnalyzer.DiagnosticId, MessageFormat, diagnosticCount));
    }

    #endregion // Methods

    #region Tests

    /// <summary>
    /// Verifies the first example of RH5201-006: the first call behind a wrapped prefix link and behind a first link a
    /// comment holds below the root is reported, while the first call behind a plain line break in front of the first link,
    /// a first call that starts its own line, and a chain without a call are not
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5201-006")]
    [Requirement("RH5201-005")]
    [Requirement("SHARED-CHAIN-005")]
    public async Task VerifyFirstCallBehindWrappedPrefixOrHeldFirstLinkIsReportedAndFixed(string endOfLine)
    {
        const string testData = """
                                using System.Linq;

                                class C
                                {
                                    C Next { get; set; }
                                    int[] Items { get; set; }
                                    C Self() => this;

                                    void M(C c)
                                    {
                                        var a = c.Next
                                                 .Items{|#0:.|}Select(x => x);
                                        var b = c // keep
                                                .Next{|#1:.|}Self();
                                        var d = c.Next // keep
                                                 .Next{|#2:.|}Self();
                                        var e = c
                                                 .Next.Self();
                                        var f = c.Next
                                                 .Next
                                                 .Self();
                                        var h = c.Next
                                                 .Next.Next;
                                    }
                                }
                                """;
        const string resultData = """
                                  using System.Linq;

                                  class C
                                  {
                                      C Next { get; set; }
                                      int[] Items { get; set; }
                                      C Self() => this;

                                      void M(C c)
                                      {
                                          var a = c.Next
                                                   .Items
                                                   .Select(x => x);
                                          var b = c // keep
                                                  .Next
                                                  .Self();
                                          var d = c.Next // keep
                                                   .Next
                                                   .Self();
                                          var e = c
                                                   .Next.Self();
                                          var f = c.Next
                                                   .Next
                                                   .Self();
                                          var h = c.Next
                                                   .Next.Next;
                                      }
                                  }
                                  """;

        await VerifyFix(ConvertEndOfLine(testData, endOfLine), ConvertEndOfLine(resultData, endOfLine), 3, 3);
    }

    /// <summary>
    /// Verifies the second example of RH5201-006: a line break inside the operator of a prefix link and a directive in front
    /// of the first link leave the root's line short of the first call, which is reported next to the split operator and,
    /// behind <c>#if</c>, in the active branch only; the fix lays out both chains
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5201-006")]
    [Requirement("RH5201-003")]
    [Requirement("RH5201-005")]
    [Requirement("SHARED-CHAIN-005")]
    public async Task VerifyFirstCallBehindSplitPrefixOperatorOrDirectiveHeldFirstLinkIsReportedAndFixed(string endOfLine)
    {
        const string testData = """
                                #nullable enable

                                class C
                                {
                                    C Next { get; set; } = null!;
                                    C? Opt { get; set; }
                                    C Self() => this;

                                    void M(C c)
                                    {
                                        var a = c.Opt{|#0:?|}
                                                 .Next{|#1:.|}Self();
                                        var b = c
                                #if X
                                                .Next.Self();
                                #else
                                                .Next{|#2:.|}Self();
                                #endif
                                    }
                                }
                                """;
        const string resultData = """
                                  #nullable enable

                                  class C
                                  {
                                      C Next { get; set; } = null!;
                                      C? Opt { get; set; }
                                      C Self() => this;

                                      void M(C c)
                                      {
                                          var a = c.Opt
                                                   ?.Next
                                                   .Self();
                                          var b = c
                                  #if X
                                                  .Next.Self();
                                  #else
                                                  .Next
                                                  .Self();
                                  #endif
                                      }
                                  }
                                  """;

        await VerifyFix(ConvertEndOfLine(testData, endOfLine), ConvertEndOfLine(resultData, endOfLine), 3, 2);
    }

    /// <summary>
    /// Verifies the third example of RH5201-006: a line break a comment keeps inside the operator of a prefix link does not
    /// count, so the first call behind it is not reported
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5201-006")]
    public async Task VerifyFirstCallBehindCommentHeldSplitOfPrefixOperatorIsNotReported(string endOfLine)
    {
        const string testData = """
                                #nullable enable

                                class C
                                {
                                    C Next { get; set; } = null!;
                                    C? Opt { get; set; }
                                    C Self() => this;

                                    void M(C c)
                                    {
                                        var g = c.Opt? // keep
                                                 .Next.Self();
                                    }
                                }
                                """;

        await Verify(ConvertEndOfLine(testData, endOfLine));
    }

    /// <summary>
    /// Verifies that a line break a comment keeps inside the operator of the chain's first link does not make the first
    /// call reported
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5201-006")]
    public async Task VerifyFirstCallBehindCommentHeldSplitOfFirstLinkOperatorIsNotReported(string endOfLine)
    {
        const string body = """
                                    var a = c? // keep
                                             .Next.Self();
                            """;

        await Verify(InChainClass(body, endOfLine));
    }

    /// <summary>
    /// Verifies the example of RH5201-002 for a comment that starts its own line: the links behind its closing <c>*/</c> in
    /// the anchor column are reported — a later call, a link of a chain without a call, and a prefix link together with
    /// the first call behind it — while a link behind the <c>*/</c> of a comment that starts behind code is the anchor; the
    /// fix starts every reported link on its own line in one application
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5201-002")]
    [Requirement("RH5201-006")]
    [Requirement("RH5201-005")]
    [Requirement("SHARED-COMMENT-012")]
    public async Task VerifyLinksBehindOwnLineCommentTailInAnchorColumnAreReportedAndFixed(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    C Next { get; set; }
                                    C Self() => this;

                                    void M(C c)
                                    {
                                        var a = c.Self()
                                                 .Self()

                                                 /* x
                                                 */ {|#0:.|}Self();
                                        var b = c.Next
                                                 .Next

                                                 /* x
                                                 */ {|#1:.|}Next;
                                        var d = c.Next

                                                 /* x
                                                 */ {|#2:.|}Next{|#3:.|}Self();
                                        var e = c.Next /* x
                                            */.Next
                                              .Self();
                                    }
                                }
                                """;
        const string resultData = """
                                  class C
                                  {
                                      C Next { get; set; }
                                      C Self() => this;

                                      void M(C c)
                                      {
                                          var a = c.Self()
                                                   .Self()

                                                   /* x
                                                   */
                                                   .Self();
                                          var b = c.Next
                                                   .Next

                                                   /* x
                                                   */
                                                   .Next;
                                          var d = c.Next

                                                   /* x
                                                   */
                                                   .Next
                                                   .Self();
                                          var e = c.Next /* x
                                              */.Next
                                                .Self();
                                      }
                                  }
                                  """;

        await VerifyFix(ConvertEndOfLine(testData, endOfLine), ConvertEndOfLine(resultData, endOfLine), 4, 3);
    }

    /// <summary>
    /// Verifies that the first call behind a first link that a block comment on the root's line holds below the root is
    /// reported, and that the fix starts it on its own line in the root's column
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5201-006")]
    [Requirement("RH5201-005")]
    public async Task VerifyFirstCallBehindFirstLinkHeldByBlockCommentOnRootLineIsReportedAndFixed(string endOfLine)
    {
        const string body = """
                                    var a = c /* x */
                                            .Next{|#0:.|}Self();
                            """;
        const string fixedBody = """
                                         var a = c /* x */
                                                 .Next
                                                 .Self();
                                 """;

        await VerifyFix(InChainClass(body, endOfLine), InChainClass(fixedBody, endOfLine), 1, 1);
    }

    /// <summary>
    /// Verifies that the first call behind a first link that a block comment in front of it holds below the root is
    /// reported, and that the fix starts it on its own line in the column the comment's line starts in
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5201-006")]
    [Requirement("RH5201-005")]
    public async Task VerifyFirstCallBehindFirstLinkHeldByBlockCommentInFrontOfItIsReportedAndFixed(string endOfLine)
    {
        const string body = """
                                    var a = c
                                            /* x */ .Next{|#0:.|}Self();
                            """;
        const string fixedBody = """
                                         var a = c
                                                 /* x */ .Next
                                                 .Self();
                                 """;

        await VerifyFix(InChainClass(body, endOfLine), InChainClass(fixedBody, endOfLine), 1, 1);
    }

    /// <summary>
    /// Verifies that the first call behind a prefix link that starts a line is reported also when a plain line break in
    /// front of the first link, which does not count by itself, stands in the same chain; the fix joins the first link to
    /// the root and starts the first call on its own line
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5201-006")]
    [Requirement("RH5201-005")]
    public async Task VerifyFirstCallBehindWrappedPrefixLinkBehindWrappedFirstLinkIsReportedAndFixed(string endOfLine)
    {
        const string body = """
                                    var a = c
                                             .Next
                                             .Next{|#0:.|}Self();
                            """;
        const string fixedBody = """
                                         var a = c.Next
                                                  .Next
                                                  .Self();
                                 """;

        await VerifyFix(InChainClass(body, endOfLine), InChainClass(fixedBody, endOfLine), 1, 1);
    }

    /// <summary>
    /// Verifies that the first call of a conditional-access chain behind a wrapped prefix link is reported on its
    /// <c>?</c>, and that the fix starts it on its own line
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5201-006")]
    [Requirement("RH5201-005")]
    public async Task VerifyConditionalFirstCallBehindWrappedConditionalPrefixLinkIsReportedAndFixed(string endOfLine)
    {
        const string body = """
                                    var a = c?.Opt
                                             ?.Opt{|#0:?|}.Self();
                            """;
        const string fixedBody = """
                                         var a = c?.Opt
                                                  ?.Opt
                                                  ?.Self();
                                 """;

        await VerifyFix(InChainClass(body, endOfLine), InChainClass(fixedBody, endOfLine), 1, 1);
    }

    /// <summary>
    /// Verifies that the first call behind a prefix link that a directive puts on its own line is reported on the
    /// <c>!</c> of its operator in the active branch, and that the fix starts it on its own line there only
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5201-006")]
    [Requirement("RH5201-005")]
    public async Task VerifyFirstCallBehindDirectiveWrappedPrefixLinkIsReportedAndFixedInTheActiveBranch(string endOfLine)
    {
        const string body = """
                                    var a = c.Next
                            #if X
                                             .Next.Self();
                            #else
                                             .Opt{|#0:!|}.Self();
                            #endif
                            """;
        const string fixedBody = """
                                         var a = c.Next
                                 #if X
                                                  .Next.Self();
                                 #else
                                                  .Opt
                                                  !.Self();
                                 #endif
                                 """;

        await VerifyFix(InChainClass(body, endOfLine), InChainClass(fixedBody, endOfLine), 1, 1);
    }

    /// <summary>
    /// Verifies that the first call behind a wrapped prefix link of a chain with a parenthesized root is reported, and that
    /// the fix starts it on its own line
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5201-006")]
    [Requirement("RH5201-005")]
    public async Task VerifyFirstCallBehindWrappedPrefixLinkOfParenthesizedRootIsReportedAndFixed(string endOfLine)
    {
        const string body = """
                                    var a = (c).Next
                                               .Next{|#0:.|}Self();
                            """;
        const string fixedBody = """
                                         var a = (c).Next
                                                    .Next
                                                    .Self();
                                 """;

        await VerifyFix(InChainClass(body, endOfLine), InChainClass(fixedBody, endOfLine), 1, 1);
    }

    /// <summary>
    /// Verifies that a line break between the <c>!</c> and the <c>.</c> of a prefix link is reported as a split operator and
    /// makes the first call behind it reported as well, and that one fix application clears both
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5201-006")]
    [Requirement("RH5201-003")]
    [Requirement("RH5201-005")]
    public async Task VerifyFirstCallBehindSplitNullForgivingPrefixOperatorIsReportedAndFixed(string endOfLine)
    {
        const string body = """
                                    var a = c.Opt{|#0:!|}
                                             .Next{|#1:.|}Self();
                            """;
        const string fixedBody = """
                                         var a = c.Opt
                                                  !.Next
                                                  .Self();
                                 """;

        await VerifyFix(InChainClass(body, endOfLine), InChainClass(fixedBody, endOfLine), 2, 1);
    }

    /// <summary>
    /// Verifies that a line break between the <c>!</c> and the <c>?.</c> of a prefix link is reported as a split operator
    /// and makes the first call behind it reported as well, and that one fix application clears both
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5201-006")]
    [Requirement("RH5201-003")]
    [Requirement("RH5201-005")]
    public async Task VerifyFirstCallBehindSplitNullForgivingConditionalPrefixOperatorIsReportedAndFixed(string endOfLine)
    {
        const string body = """
                                    var a = c.Next.Opt{|#0:!|}
                                                  ?.Next{|#1:.|}Self();
                            """;
        const string fixedBody = """
                                         var a = c.Next.Opt
                                                       !?.Next
                                                       .Self();
                                 """;

        await VerifyFix(InChainClass(body, endOfLine), InChainClass(fixedBody, endOfLine), 2, 1);
    }

    /// <summary>
    /// Verifies that a line break inside the operator of the chain's first link is reported as a split operator only, and
    /// that the fix joins the chain onto one line
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5201-006")]
    [Requirement("RH5201-003")]
    [Requirement("RH5201-005")]
    public async Task VerifyFirstCallBehindSplitFirstLinkOperatorIsNotReported(string endOfLine)
    {
        const string body = """
                                    var a = c{|#0:!|}
                                             .Next.Self();
                            """;
        const string fixedBody = """
                                         var a = c!.Next.Self();
                                 """;

        await VerifyFix(InChainClass(body, endOfLine), InChainClass(fixedBody, endOfLine), 1, 1);
    }

    /// <summary>
    /// Verifies that a prefix link behind the closing <c>*/</c> of a comment that starts behind code on the line in front
    /// does not start a line, so the first call on its line is not reported and is the anchor of the later call
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5201-006")]
    public async Task VerifyFirstCallBehindPrefixLinkBehindTailOfCommentStartingBehindCodeIsNotReported(string endOfLine)
    {
        const string body = """
                                    var a = c.Next /* x
                                        */.Next.Self()
                                               .Self();
                            """;

        await Verify(InChainClass(body, endOfLine));
    }

    /// <summary>
    /// Verifies that the links of a chain without a call behind a first link that a comment holds below the root are not
    /// reported for sharing a line
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5201-001")]
    public async Task VerifyCallLessChainBehindCommentHeldFirstLinkIsNotReported(string endOfLine)
    {
        const string body = """
                                    var a = c // keep
                                            .Next.Next;
                            """;

        await Verify(InChainClass(body, endOfLine));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testData = """
                                #nullable enable

                                class C
                                {
                                    C Next { get; set; } = null!;
                                    C? Opt { get; set; }
                                    C Self() => this;

                                    void M(C c)
                                    {
                                        var a = c.Next
                                                 .Next{|#0:.|}Self();
                                        var b = c.Opt{|#1:!|}
                                                 .Next{|#2:.|}Self();
                                    }
                                }
                                """;
        const string resultData = """
                                  #nullable enable

                                  class C
                                  {
                                      C Next { get; set; } = null!;
                                      C? Opt { get; set; }
                                      C Self() => this;

                                      void M(C c)
                                      {
                                          var a = c.Next
                                                   .Next
                                                   .Self();
                                          var b = c.Opt
                                                   !.Next
                                                   .Self();
                                      }
                                  }
                                  """;

        // Verifies that Fix All lays out two chains, one of them with two diagnostics, in one application
        return new FixAllScenario(testData,
                                  resultData,
                                  Diagnostics(RH5201MethodChainsShouldBeAlignedAnalyzer.DiagnosticId, MessageFormat, 3),
                                  static config => config.NumberOfFixAllIterations = 1);
    }

    #endregion // BatchCodeFixTestsBase
}