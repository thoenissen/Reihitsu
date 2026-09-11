using System.Threading.Tasks;

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
    /// EXPERIMENT: split-region shape from issue #749 (literal example), LF. No expected diagnostics supplied so
    /// any actual diagnostic/compiler-diagnostic appears in the failure message for inspection
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task Issue749SplitRegionProbe()
    {
        const string testData = "public class Example\n" +
                                 "{\n" +
                                 "#if false\n" +
                                 "#region Disabled\n" +
                                 "#endif\n" +
                                 "    public bool Value => true;\n" +
                                 "#endregion\n" +
                                 "}\n";

        await Verify(testData);
    }

    /// <summary>
    /// EXPERIMENT: split-region shape from issue #749 (literal example), CRLF counterpart of <see cref="Issue749SplitRegionProbe"/>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task Issue749SplitRegionProbeCarriageReturnLineFeed()
    {
        var testData = NormalizeToCarriageReturnLineFeed("public class Example\n" +
                                                          "{\n" +
                                                          "#if false\n" +
                                                          "#region Disabled\n" +
                                                          "#endif\n" +
                                                          "    public bool Value => true;\n" +
                                                          "#endregion\n" +
                                                          "}\n");

        await Verify(testData);
    }

    /// <summary>
    /// EXPERIMENT: sibling shape from issue #749 question 3 - region opened under the taken <c>#if</c> branch and
    /// closed under the untaken <c>#else</c> branch, crossing a branch boundary within the same conditional group
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task Issue749SplitRegionAcrossElseProbe()
    {
        const string testData = "public class Example\n" +
                                 "{\n" +
                                 "#if DEBUG\n" +
                                 "#region Disabled\n" +
                                 "    public bool Value => true;\n" +
                                 "#else\n" +
                                 "#endregion\n" +
                                 "#endif\n" +
                                 "}\n";

        await Verify(testData);
    }

    /// <summary>
    /// EXPERIMENT: sibling shape from issue #749 question 3 - region opened under an untaken <c>#if false</c>
    /// branch and closed under the taken <c>#elif</c> branch, crossing a branch boundary within the same
    /// conditional group
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task Issue749SplitRegionAcrossElifProbe()
    {
        const string testData = "public class Example\n" +
                                 "{\n" +
                                 "#if false\n" +
                                 "#region Disabled\n" +
                                 "#elif true\n" +
                                 "    public bool Value => true;\n" +
                                 "#endregion\n" +
                                 "#endif\n" +
                                 "}\n";

        await Verify(testData);
    }

    /// <summary>
    /// EXPERIMENT: sibling shape from issue #749 question 3 - a region that nests properly around an entire,
    /// wholly-nested <c>#if false</c> block (region and endregion both sit in the active branch, no crossing),
    /// to check whether the disabled token in between still leaves <c>GetExpectedIndentation</c> with no live
    /// token to anchor on
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task Issue749NestedIfFalseInsideRegionProbe()
    {
        const string testData = "public class Example\n" +
                                 "{\n" +
                                 "#if true\n" +
                                 "#region Disabled\n" +
                                 "#if false\n" +
                                 "    public bool Value => true;\n" +
                                 "#endif\n" +
                                 "#endregion\n" +
                                 "#endif\n" +
                                 "}\n";

        await Verify(testData);
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