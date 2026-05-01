using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0389IndentationMustUseFourSpacesPerScopeLevelAnalyzer"/> and <see cref="RH0389IndentationMustUseFourSpacesPerScopeLevelCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0389IndentationMustUseFourSpacesPerScopeLevelAnalyzerTests : AnalyzerTestsBase<RH0389IndentationMustUseFourSpacesPerScopeLevelAnalyzer, RH0389IndentationMustUseFourSpacesPerScopeLevelCodeFixProvider>
{
    #region Members

    /// <summary>
    /// Verifies that clean indentation does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenIndentationIsConsistent()
    {
        const string testData = """
                                internal class Example
                                {
                                    #region Members

                                    /// <summary>
                                    /// Gets a value.
                                    /// </summary>
                                    internal bool Value
                                    {
                                        get
                                        {
                                            // Comment
                                            return true;
                                        }
                                    }

                                    #endregion // Members
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that lines within multi-line strings are ignored
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForMultiLineStrings()
    {
        const string testData = """
                                internal class Example
                                {
                                    internal void Method()
                                    {
                                        var text = @"first
                                  second";
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that member indentation is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMemberIndentationIsDetectedAndFixed()
    {
        const string testData = """
                                internal class Example
                                {
                                  {|#0:internal|} bool Value
                                    {
                                        get;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal bool Value
                                     {
                                         get;
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH0389IndentationMustUseFourSpacesPerScopeLevelAnalyzer.DiagnosticId, AnalyzerResources.RH0389MessageFormat));
    }

    /// <summary>
    /// Verifies that nested statement indentation is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNestedStatementIndentationIsDetectedAndFixed()
    {
        const string testData = """
                                internal class Example
                                {
                                    internal bool Method()
                                    {
                                         {|#0:return|} false;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal bool Method()
                                     {
                                         return false;
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH0389IndentationMustUseFourSpacesPerScopeLevelAnalyzer.DiagnosticId, AnalyzerResources.RH0389MessageFormat));
    }

    /// <summary>
    /// Verifies that accessor indentation is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyAccessorIndentationIsDetectedAndFixed()
    {
        const string testData = """
                                internal class Example
                                {
                                    internal bool Value
                                    {
                                      {|#0:get|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal bool Value
                                     {
                                         get;
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH0389IndentationMustUseFourSpacesPerScopeLevelAnalyzer.DiagnosticId, AnalyzerResources.RH0389MessageFormat));
    }

    /// <summary>
    /// Verifies that region indentation is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyRegionIndentationIsDetectedAndFixed()
    {
        const string testData = """
                                internal class Example
                                {
                                  {|#0:#region Members|}

                                    internal bool Value => true;

                                    #endregion // Members
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     #region Members

                                     internal bool Value => true;

                                     #endregion // Members
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH0389IndentationMustUseFourSpacesPerScopeLevelAnalyzer.DiagnosticId, AnalyzerResources.RH0389MessageFormat));
    }

    /// <summary>
    /// Verifies that comment indentation is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentIndentationIsDetectedAndFixed()
    {
        const string testData = """
                                internal class Example
                                {
                                    internal void Method()
                                    {
                                      {|#0:// Comment|}
                                        return;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal void Method()
                                     {
                                         // Comment
                                         return;
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH0389IndentationMustUseFourSpacesPerScopeLevelAnalyzer.DiagnosticId, AnalyzerResources.RH0389MessageFormat));
    }

    /// <summary>
    /// Verifies that multiple indentation issues are detected and fixed together
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMultipleIndentationIssuesAreDetectedAndFixed()
    {
        const string testData = """
                                internal class Example
                                {
                                  {|#0:#region Members|}

                                  {|#1:internal|} bool Method()
                                  {|#2:{|}
                                     {|#3:// Comment|}
                                     {|#4:return|} false;
                                  {|#5:}|}

                                    #endregion // Members
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     #region Members

                                     internal bool Method()
                                     {
                                         // Comment
                                         return false;
                                     }

                                     #endregion // Members
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH0389IndentationMustUseFourSpacesPerScopeLevelAnalyzer.DiagnosticId, AnalyzerResources.RH0389MessageFormat, 6));
    }

    #endregion // Members
}