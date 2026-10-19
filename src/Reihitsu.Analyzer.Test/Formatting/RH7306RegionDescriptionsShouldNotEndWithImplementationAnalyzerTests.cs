using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Organization;
using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH7306RegionDescriptionsShouldNotEndWithImplementationAnalyzer"/>
/// </summary>
[TestClass]
public class RH7306RegionDescriptionsShouldNotEndWithImplementationAnalyzerTests : BatchCodeFixTestsBase<RH7306RegionDescriptionsShouldNotEndWithImplementationAnalyzer, RH7306RegionDescriptionsShouldNotEndWithImplementationCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that concise region descriptions do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForConciseDescriptions()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    #region Methods

                                    public void DoWork()
                                    {
                                    }

                                    #endregion // Methods
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that region descriptions ending with implementation are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7306-001")]
    public async Task VerifyRegionDescriptionsEndingWithImplementationAreDetected()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    {|#0:#region Methods implementation|}

                                    public void DoWork()
                                    {
                                    }

                                    {|#1:#endregion // Methods implementation|}
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     #region Methods

                                     public void DoWork()
                                     {
                                     }

                                     #endregion // Methods
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH7306RegionDescriptionsShouldNotEndWithImplementationAnalyzer.DiagnosticId, AnalyzerResources.RH7306MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that case-insensitive implementation suffixes are detected
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7306-001")]
    public async Task VerifyImplementationSuffixIsDetectedCaseInsensitively()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    {|#0:#region IDisposable IMPLEMENTATION|}

                                    public void Dispose()
                                    {
                                    }

                                    {|#1:#endregion // IDisposable IMPLEMENTATION|}
                                }
                                """;

        await Verify(testData, Diagnostics(RH7306RegionDescriptionsShouldNotEndWithImplementationAnalyzer.DiagnosticId, AnalyzerResources.RH7306MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that words merely containing implementation are not detected
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7306-002")]
    public async Task VerifyDescriptionsContainingImplementationAsPartOfAnotherWordAreIgnored()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    #region ImplementationDetails

                                    public void DoWork()
                                    {
                                    }

                                    #endregion // ImplementationDetails
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that region and end directives whose description ends with the word implementation, in any casing, are reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7306-001")]
    [Requirement("SHARED-REGDESC-002")]
    public async Task DescriptionsEndingWithTheWordImplementationAreReported()
    {
        const string testData = """
                                class C
                                {
                                    {|RH7306:#region IDisposable implementation|}

                                    int _a;

                                    #endregion

                                    {|RH7306:#region Implementation|}

                                    int _b;

                                    {|RH7306:#endregion // IComparable IMPLEMENTATION|}
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that implementation as part of a longer word or followed by other text is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7306-002")]
    public async Task WordInsideOrBeforeOtherTextIsNotReported()
    {
        const string testData = """
                                class C
                                {
                                    #region Reimplementation

                                    int _a;

                                    #endregion // Reimplementation

                                    #region IDisposable implementation details

                                    int _b;

                                    #endregion // IDisposable implementation details
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the code fix removes the trailing word from the reported directive and from its partner when that ends with the word as well
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7306-003")]
    public async Task CodeFixRemovesTheTrailingWordFromBothDirectives()
    {
        const string testData = """
                                class C
                                {
                                    {|RH7306:#region IDisposable implementation|}

                                    int _a;

                                    {|RH7306:#endregion // IDisposable implementation|}

                                    {|RH7306:#region IComparable implementation|}

                                    int _b;

                                    #endregion

                                    #region ICloneable

                                    int _c;

                                    {|RH7306:#endregion ICloneable Implementation|}
                                }
                                """;

        const string fixedData = """
                                 class C
                                 {
                                     #region IDisposable

                                     int _a;

                                     #endregion // IDisposable

                                     #region IComparable

                                     int _b;

                                     #endregion

                                     #region ICloneable

                                     int _c;

                                     #endregion ICloneable
                                 }
                                 """;

        await Verify(testData, fixedData);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    {|#0:#region Methods implementation|}

                                    public void DoWork()
                                    {
                                    }

                                    {|#1:#endregion // Methods implementation|}
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     #region Methods

                                     public void DoWork()
                                     {
                                     }

                                     #endregion // Methods
                                 }
                                 """;

        // Both diagnostics belong to the same region's #region and #endregion description; the fix registered
        // for either one strips the "implementation" suffix from both directive lines, so the two computed
        // fixes target the exact same span, the strongest form of interference in this batch, and the batch
        // fixer must still converge to correcting them once
        return new FixAllScenario(testData,
                                  fixedData,
                                  Diagnostics(RH7306RegionDescriptionsShouldNotEndWithImplementationAnalyzer.DiagnosticId, AnalyzerResources.RH7306MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}