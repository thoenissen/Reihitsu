using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Organization;
using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH7304RegionDirectivesMustUseConsistentIndentationAnalyzer"/> and <see cref="RH7304RegionDirectivesMustUseConsistentIndentationCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH7304RegionDirectivesMustUseConsistentIndentationAnalyzerTests : BatchCodeFixTestsBase<RH7304RegionDirectivesMustUseConsistentIndentationAnalyzer, RH7304RegionDirectivesMustUseConsistentIndentationCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that correctly indented region directives do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenRegionDirectivesMatchContainingScope()
    {
        const string testData = """
                                namespace Sample
                                {
                                    #region Types

                                    internal class Example
                                    {
                                        #region Fields

                                        private string _name;

                                        #endregion // Fields

                                        internal interface INested
                                        {
                                            #region Tests
                                            void DoWork();

                                            #endregion // Tests
                                        }

                                        internal record NestedRecord
                                        {
                                            #region Properties

                                            internal int Value { get; init; }

                                            #endregion // Properties
                                        }
                                    }

                                    internal struct ExampleStruct
                                    {
                                        #region Methods

                                        internal void DoWork()
                                        {
                                        }

                                        #endregion // Methods
                                    }

                                    #endregion // Types
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that misindented type-level region directives are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMisindentedTypeLevelRegionDirectivesAreDetectedAndFixed()
    {
        const string testData = """
                                internal class Example
                                {
                                {|#0:#region Fields|}

                                    private string _name;

                                {|#1:#endregion // Fields|}
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     #region Fields

                                     private string _name;

                                     #endregion // Fields
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH7304RegionDirectivesMustUseConsistentIndentationAnalyzer.DiagnosticId, AnalyzerResources.RH7304MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that misindented nested type region directives are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMisindentedNestedTypeRegionDirectivesAreDetectedAndFixed()
    {
        const string testData = """
                                internal class Outer
                                {
                                    internal class Inner
                                    {
                                    {|#0:#region Fields|}

                                        private string _name;

                                    {|#1:#endregion // Fields|}
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class Outer
                                 {
                                     internal class Inner
                                     {
                                         #region Fields

                                         private string _name;

                                         #endregion // Fields
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH7304RegionDirectivesMustUseConsistentIndentationAnalyzer.DiagnosticId, AnalyzerResources.RH7304MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that misindented nested region directives are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMisindentedNestedRegionDirectivesAreDetectedAndFixed()
    {
        const string testData = """
                                internal class Example
                                {
                                    #region Outer

                                {|#0:#region Inner|}

                                    private string _name;

                                {|#1:#endregion // Inner|}

                                    #endregion // Outer
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     #region Outer

                                     #region Inner

                                     private string _name;

                                     #endregion // Inner

                                     #endregion // Outer
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH7304RegionDirectivesMustUseConsistentIndentationAnalyzer.DiagnosticId, AnalyzerResources.RH7304MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that the code fix only changes directive indentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixOnlyChangesDirectiveIndentation()
    {
        const string testData = """
                                internal class Example
                                {
                                {|#0:#region fields|}

                                    private string _name;

                                {|#1:#endregion // fields|}
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     #region fields

                                     private string _name;

                                     #endregion // fields
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH7304RegionDirectivesMustUseConsistentIndentationAnalyzer.DiagnosticId, AnalyzerResources.RH7304MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that indentation is inferred from the containing code instead of a fixed indentation width
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyIndentationIsInferredFromContainingCode()
    {
        const string testData = """
                                internal class Example
                                {
                                {|#0:#region Fields|}

                                  private string _name;

                                {|#1:#endregion // Fields|}
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                   #region Fields

                                   private string _name;

                                   #endregion // Fields
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH7304RegionDirectivesMustUseConsistentIndentationAnalyzer.DiagnosticId, AnalyzerResources.RH7304MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that region directives within element bodies are ignored by this rule
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyRegionDirectivesWithinElementBodiesAreIgnored()
    {
        const string testData = """
                                internal class Example
                                {
                                    internal void DoWork()
                                    {
                                #region Inner

                                #endregion // Inner
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a region opening inside an excluded element body cannot consume the first included endregion
    /// after that body during pairing
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyExcludedElementBodyDirectiveCannotPairWithContainingTypeDirective()
    {
        const string testData = """
                                internal class Example
                                {
                                {|#0:#region Outer|}

                                    internal void DoWork()
                                    {
                                #region Body
                                    }

                                {|#1:#endregion // Body|}
                                #endregion // Outer
                                }
                                """;

        await Verify(testData,
                     Diagnostics(RH7304RegionDirectivesMustUseConsistentIndentationAnalyzer.DiagnosticId, AnalyzerResources.RH7304MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that a region directive split across an <c>#if</c>/<c>#endif</c> boundary is rejected by the
    /// compiler's own conditional-compilation nesting rule before this rule ever sees a live token to report on
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyRegionSplitAcrossEndIfBoundaryFailsToCompileAndIsIgnored()
    {
        const string testData = """
                                public class Example
                                {
                                #if false
                                #region Disabled
                                #endif
                                    public bool Value => true;
                                #endregion
                                }
                                """;

        await Verify(testData, test => test.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// CRLF counterpart of <see cref="VerifyRegionSplitAcrossEndIfBoundaryFailsToCompileAndIsIgnored"/>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyRegionSplitAcrossEndIfBoundaryFailsToCompileAndIsIgnoredCarriageReturnLineFeed()
    {
        var testData = NormalizeToCarriageReturnLineFeed("""
                                                         public class Example
                                                         {
                                                         #if false
                                                         #region Disabled
                                                         #endif
                                                             public bool Value => true;
                                                         #endregion
                                                         }
                                                         """);

        await Verify(testData, test => test.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifies that a region directive split across an <c>#if</c>/<c>#else</c> boundary still fails to compile
    /// (the compiler's own conditional-compilation nesting rule rejects it), but that this rule and RH5204
    /// report identically on the error-recovered pair rather than diverging - confirmed by running the
    /// equivalent input through <c>RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer</c> directly, which
    /// reports at the same two spans despite already applying the <c>IsInactiveDirective</c> guard
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyRegionSplitAcrossElseBoundaryStillReportsInParityWithRH5204()
    {
        const string testData = """
                                public class Example
                                {
                                #if true
                                {|#0:#region Split|}

                                    public bool Value => true;

                                #else
                                {|#1:#endregion|}
                                #endif
                                }
                                """;

        await Verify(testData,
                     test => test.CompilerDiagnostics = CompilerDiagnostics.None,
                     Diagnostics(RH7304RegionDirectivesMustUseConsistentIndentationAnalyzer.DiagnosticId, AnalyzerResources.RH7304MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that a region directive split across an <c>#if false</c>/<c>#elif</c> boundary is likewise
    /// rejected by the compiler before this rule can report on it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyRegionSplitAcrossElifBoundaryFailsToCompileAndIsIgnored()
    {
        const string testData = """
                                public class Example
                                {
                                #if false
                                #region Disabled
                                #elif true
                                    public bool Value => true;
                                #endregion
                                #endif
                                }
                                """;

        await Verify(testData, test => test.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifies that a region wholly containing a nested <c>#if false</c> block leaves no live token for the
    /// pair to anchor on, so the directive is ignored just like the formatter and RH5204 ignore it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyRegionWhollyContainingSkippedBranchStaysSilent()
    {
        const string testData = """
                                public class Example
                                {
                                #if true
                                #region Disabled
                                #if false
                                    public bool Value => true;
                                #endif
                                #endregion
                                #endif
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an inactive region pair stays silent even while a differently-indented active region pair
    /// in the same file is still reported, so the compiler's own skip of disabled text - not a policy this rule
    /// applies - is what keeps inactive directives out of its report
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyInactiveRegionPairStaysSilentBesideReportedActivePair()
    {
        const string testData = """
                                public class Example
                                {
                                #if true
                                {|#0:#region Active|}

                                    public bool Value => true;

                                {|#1:#endregion // Active|}
                                #else
                                #region Inactive

                                    public bool OtherValue => true;

                                #endregion // Inactive
                                #endif
                                }
                                """;

        await Verify(testData,
                     Diagnostics(RH7304RegionDirectivesMustUseConsistentIndentationAnalyzer.DiagnosticId, AnalyzerResources.RH7304MessageFormat, 2));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testData = """
                                internal class Example
                                {
                                {|#0:#region Fields|}

                                    private string _name;

                                {|#1:#endregion // Fields|}
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     #region Fields

                                     private string _name;

                                     #endregion // Fields
                                 }
                                 """;

        // The #region and #endregion directives of the same region pair are the rule's minimal reporting unit;
        // each fix only re-indents its own directive line, so the batch fixer converges in one pass
        return new FixAllScenario(testData,
                                  fixedData,
                                  Diagnostics(RH7304RegionDirectivesMustUseConsistentIndentationAnalyzer.DiagnosticId, AnalyzerResources.RH7304MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}