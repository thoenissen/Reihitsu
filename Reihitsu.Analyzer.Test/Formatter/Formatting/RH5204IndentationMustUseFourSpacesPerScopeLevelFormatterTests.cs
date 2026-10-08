using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer"/>
/// </summary>
[TestClass]
public class RH5204IndentationMustUseFourSpacesPerScopeLevelFormatterTests : FormatterTestsBase<RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter normalizes indentation to four spaces per level
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string input = """
                             internal class Example
                             {
                               internal bool Value { get; }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal bool Value { get; }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              ExpectedDiagnostic(RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer.DiagnosticId, 3, 3, 3, 11, AnalyzerResources.RH5204MessageFormat));
    }

    /// <summary>
    /// Verifies that an opening brace followed by a delimited documentation comment on the same line is analyzer-clean and
    /// formatter-stable, because the comment does not start its line and so does not decide that line's indentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyOpeningBraceWithDelimitedDocumentationBehindItIsStable()
    {
        const string source = """
                              internal class Example
                              { /** Doc */
                                  internal bool Value { get; }
                              }
                              """;

        await VerifyFormatter(source);
    }

    /// <summary>
    /// Verifies that a switch label followed by a delimited documentation comment on the same line is analyzer-clean and
    /// formatter-stable
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCaseLabelWithDelimitedDocumentationBehindItIsStable()
    {
        const string source = """
                              internal class Example
                              {
                                  internal int Method(int value)
                                  {
                                      switch (value)
                                      {
                                          case 1: /** Doc */
                                              return 2;
                                      }

                                      return 0;
                                  }
                              }
                              """;

        await VerifyFormatter(source);
    }

    /// <summary>
    /// Verifies that the formatter leaves region directives inside a branch the compiler excluded exactly where the
    /// author wrote them. Together with the analyzer's matching carve-out this pins the parity that was missing while
    /// the analyzer re-indented what the formatter declined to touch
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterLeavesRegionDirectivesInsideInactiveBranchUntouched()
    {
        const string source = """
                              internal class Example
                              {
                              #if false
                              #region Disabled
                              #endregion // Disabled
                              #endif

                                  internal bool Value => true;
                              }
                              """;

        await VerifyFormatter(source);
    }

    #endregion // Tests
}