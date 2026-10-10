using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH7301RegionsShouldMatchAnalyzer"/>
/// </summary>
[TestClass]
public class RH7301RegionsShouldMatchFormatterTests : FormatterTestsBase<RH7301RegionsShouldMatchAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter synchronizes mismatched region comments
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string input = """
                             internal class Example
                             {
                                 #region Fields
                                 private readonly int _value;
                                 #endregion // Properties
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     #region Fields

                                     private readonly int _value;

                                     #endregion // Fields
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              ExpectedDiagnostic(RH7301RegionsShouldMatchAnalyzer.DiagnosticId, 5, 5, 5, 29, AnalyzerResources.RH7301MessageFormat));
    }

    /// <summary>
    /// Verifies that an analyzer-clean pair with trailing whitespace on the opening description remains byte-stable
    /// under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterPreservesTrailingOpeningWhitespaceWhenCloseIsCanonical()
    {
        const string source = """
                              internal class Example
                              {
                                  #region Fields

                                  private readonly int _value;

                                  #endregion // Fields
                              }
                              """;
        var input = source.Replace("#region Fields", "#region Fields   ");

        await VerifyFormatter(input);
    }

    /// <summary>
    /// Verifies that a mismatched close is canonicalized from the normalized opening description without carrying
    /// its trailing whitespace
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterUsesNormalizedOpeningDescriptionForMismatchedClose()
    {
        const string source = """
                              internal class Example
                              {
                                  #region Fields

                                  private readonly int _value;

                                  {|#0:#endregion // Wrong|}
                              }
                              """;
        const string fixedSource = """
                                   internal class Example
                                   {
                                       #region Fields

                                       private readonly int _value;

                                       #endregion // Fields
                                   }
                                   """;
        var input = source.Replace("#region Fields", "#region Fields   ");
        var expected = fixedSource.Replace("#region Fields", "#region Fields   ");

        await VerifyFormatter(input,
                              expected,
                              Diagnostics(RH7301RegionsShouldMatchAnalyzer.DiagnosticId, AnalyzerResources.RH7301MessageFormat));
    }

    /// <summary>
    /// Verifies that a lowercase opening description with trailing whitespace and a bare close is capitalized,
    /// normalized, synchronized, and stable on a second formatter pass
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterCanonicalizesLowercaseTrailingOpeningAndBareClose()
    {
        const string source = """
                              internal class Example
                              {
                                  #region methods

                                  private readonly int _value;

                                  {|#0:#endregion|}
                              }
                              """;
        const string fixedSource = """
                                   internal class Example
                                   {
                                       #region Methods

                                       private readonly int _value;

                                       #endregion // Methods
                                   }
                                   """;
        var input = source.Replace("#region methods", "#region methods   ");

        await VerifyFormatter(input,
                              fixedSource,
                              Diagnostics(RH7301RegionsShouldMatchAnalyzer.DiagnosticId, AnalyzerResources.RH7301MessageFormat));
    }

    #endregion // Tests
}