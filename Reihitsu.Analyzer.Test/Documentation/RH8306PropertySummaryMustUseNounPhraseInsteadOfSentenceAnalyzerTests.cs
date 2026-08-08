using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Test methods for <see cref="RH8306PropertySummaryMustUseNounPhraseInsteadOfSentenceAnalyzer"/>
/// </summary>
[TestClass]
public class RH8306PropertySummaryMustUseNounPhraseInsteadOfSentenceAnalyzerTests : AnalyzerTestsBase<RH8306PropertySummaryMustUseNounPhraseInsteadOfSentenceAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that a property summary written as a noun phrase does not produce a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForNounPhrasePropertySummary()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <summary>
                                    /// Current project configuration
                                    /// </summary>
                                    public int Configuration { get; set; }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a single-word property summary does not produce a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForSingleWordPropertySummary()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <summary>
                                    /// Configuration
                                    /// </summary>
                                    public int Configuration { get; set; }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a property summary containing a verb that is not the first word does not produce a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticWhenVerbIsNotTheFirstWord()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <summary>
                                    /// Name that represents the user
                                    /// </summary>
                                    public string Name { get; set; }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a property summary ending with a period is detected
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForPropertySummaryEndingWithPeriod()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// {|#0:<summary>
                                    /// Current project configuration.
                                    /// </summary>|}
                                    public int Configuration { get; set; }
                                }
                                """;

        await Verify(testData, Diagnostics(RH8306PropertySummaryMustUseNounPhraseInsteadOfSentenceAnalyzer.DiagnosticId, AnalyzerResources.RH8306MessageFormat));
    }

    /// <summary>
    /// Verifies that a property summary ending with an exclamation mark is detected
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForPropertySummaryEndingWithExclamationMark()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// {|#0:<summary>
                                    /// Current project configuration!
                                    /// </summary>|}
                                    public int Configuration { get; set; }
                                }
                                """;

        await Verify(testData, Diagnostics(RH8306PropertySummaryMustUseNounPhraseInsteadOfSentenceAnalyzer.DiagnosticId, AnalyzerResources.RH8306MessageFormat));
    }

    /// <summary>
    /// Verifies that a Boolean property summary ending with a question mark does not produce a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForBooleanPropertySummaryEndingWithQuestionMark()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <summary>
                                    /// Is the value valid?
                                    /// </summary>
                                    public bool IsValid { get; set; }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a nullable Boolean property summary ending with a question mark does not produce a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForNullableBooleanPropertySummaryEndingWithQuestionMark()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <summary>
                                    /// Is the value valid?
                                    /// </summary>
                                    public bool? IsValid { get; set; }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a Nullable Boolean property summary ending with a question mark does not produce a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForNullableBooleanTypePropertySummaryEndingWithQuestionMark()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <summary>
                                    /// Is the value valid?
                                    /// </summary>
                                    public System.Nullable<bool> IsValid { get; set; }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a non-Boolean property summary ending with a question mark is detected
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForNonBooleanPropertySummaryEndingWithQuestionMark()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// {|#0:<summary>
                                    /// Current value?
                                    /// </summary>|}
                                    public int Value { get; set; }
                                }
                                """;

        await Verify(testData, Diagnostics(RH8306PropertySummaryMustUseNounPhraseInsteadOfSentenceAnalyzer.DiagnosticId, AnalyzerResources.RH8306MessageFormat));
    }

    /// <summary>
    /// Verifies that a Boolean property summary ending with a period is detected
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForBooleanPropertySummaryEndingWithPeriod()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// {|#0:<summary>
                                    /// Is the value valid.
                                    /// </summary>|}
                                    public bool IsValid { get; set; }
                                }
                                """;

        await Verify(testData, Diagnostics(RH8306PropertySummaryMustUseNounPhraseInsteadOfSentenceAnalyzer.DiagnosticId, AnalyzerResources.RH8306MessageFormat));
    }

    /// <summary>
    /// Verifies that a Boolean property summary ending with an exclamation mark is detected
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForBooleanPropertySummaryEndingWithExclamationMark()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// {|#0:<summary>
                                    /// Is the value valid!
                                    /// </summary>|}
                                    public bool IsValid { get; set; }
                                }
                                """;

        await Verify(testData, Diagnostics(RH8306PropertySummaryMustUseNounPhraseInsteadOfSentenceAnalyzer.DiagnosticId, AnalyzerResources.RH8306MessageFormat));
    }

    /// <summary>
    /// Verifies that a Boolean property summary starting with a sentence-leading verb is detected when it ends with a question mark
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForBooleanPropertySummaryStartingWithSentenceLeadingVerbAndEndingWithQuestionMark()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// {|#0:<summary>
                                    /// Indicates whether the value is valid?
                                    /// </summary>|}
                                    public bool IsValid { get; set; }
                                }
                                """;

        await Verify(testData, Diagnostics(RH8306PropertySummaryMustUseNounPhraseInsteadOfSentenceAnalyzer.DiagnosticId, AnalyzerResources.RH8306MessageFormat));
    }

    /// <summary>
    /// Verifies that a property summary starting with "Gets or sets" is detected
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForPropertySummaryStartingWithGetsOrSets()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// {|#0:<summary>
                                    /// Gets or sets the current project configuration
                                    /// </summary>|}
                                    public int Configuration { get; set; }
                                }
                                """;

        await Verify(testData, Diagnostics(RH8306PropertySummaryMustUseNounPhraseInsteadOfSentenceAnalyzer.DiagnosticId, AnalyzerResources.RH8306MessageFormat));
    }

    /// <summary>
    /// Verifies that a property summary starting with "Represents" is detected
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForPropertySummaryStartingWithRepresents()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// {|#0:<summary>
                                    /// Represents the current project configuration
                                    /// </summary>|}
                                    public int Configuration { get; set; }
                                }
                                """;

        await Verify(testData, Diagnostics(RH8306PropertySummaryMustUseNounPhraseInsteadOfSentenceAnalyzer.DiagnosticId, AnalyzerResources.RH8306MessageFormat));
    }

    /// <summary>
    /// Verifies that a method summary ending with a period does not produce a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForMethodSummaryEndingWithPeriod()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <summary>
                                    /// Validates the input.
                                    /// </summary>
                                    public void Method()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a parameter description ending with a period does not produce a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForParamEndingWithPeriod()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <summary>
                                    /// Method summary
                                    /// </summary>
                                    /// <param name="value">The value to validate.</param>
                                    public void Method(int value)
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a property using inheritdoc does not produce a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForPropertyUsingInheritdoc()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <inheritdoc/>
                                    public int Configuration { get; set; }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a property with an empty summary does not produce a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForPropertyWithEmptySummary()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <summary>
                                    ///
                                    /// </summary>
                                    public int Configuration { get; set; }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a sentence-like summary nested in remarks is ignored
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForSentenceSummaryNestedInRemarks()
    {
        const string testData = """
                                namespace TestNamespace
                                {
                                    internal class TestClass
                                    {
                                        /// <remarks><summary>Gets the value.</summary></remarks>
                                        internal string TestProperty { get; }
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when documentation mode is none
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenDocumentationModeIsNone()
    {
        const string source = """
                              internal class TestClass
                              {
                                  /// <summary>
                                  /// Gets or sets the current project configuration.
                                  /// </summary>
                                  public int Configuration { get; set; }
                              }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
    }

    #endregion // Tests
}