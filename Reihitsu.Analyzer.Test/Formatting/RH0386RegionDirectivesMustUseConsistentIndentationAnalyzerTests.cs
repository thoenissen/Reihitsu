using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0386RegionDirectivesMustUseConsistentIndentationAnalyzer"/> and <see cref="RH0386RegionDirectivesMustUseConsistentIndentationCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0386RegionDirectivesMustUseConsistentIndentationAnalyzerTests : AnalyzerTestsBase<RH0386RegionDirectivesMustUseConsistentIndentationAnalyzer, RH0386RegionDirectivesMustUseConsistentIndentationCodeFixProvider>
{
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
                                            #region Members

                                            void DoWork();

                                            #endregion // Members
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
                     Diagnostics(RH0386RegionDirectivesMustUseConsistentIndentationAnalyzer.DiagnosticId, AnalyzerResources.RH0386MessageFormat, 2));
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
                     Diagnostics(RH0386RegionDirectivesMustUseConsistentIndentationAnalyzer.DiagnosticId, AnalyzerResources.RH0386MessageFormat, 2));
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
                     Diagnostics(RH0386RegionDirectivesMustUseConsistentIndentationAnalyzer.DiagnosticId, AnalyzerResources.RH0386MessageFormat, 2));
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
                     Diagnostics(RH0386RegionDirectivesMustUseConsistentIndentationAnalyzer.DiagnosticId, AnalyzerResources.RH0386MessageFormat, 2));
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
                     Diagnostics(RH0386RegionDirectivesMustUseConsistentIndentationAnalyzer.DiagnosticId, AnalyzerResources.RH0386MessageFormat, 2));
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
}