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
    [Requirement("RH8306-002")]
    [Requirement("RH8306-003")]
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
    [Requirement("RH8306-003")]
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
    [Requirement("RH8306-003")]
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
    [Requirement("RH8306-002")]
    [Requirement("RH8306-004")]
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
    [Requirement("RH8306-002")]
    [Requirement("RH8306-004")]
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
    [Requirement("RH8306-002")]
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
    [Requirement("RH8306-002")]
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
    [Requirement("RH8306-002")]
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
    [Requirement("RH8306-002")]
    [Requirement("RH8306-004")]
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
    [Requirement("RH8306-002")]
    [Requirement("RH8306-004")]
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
    [Requirement("RH8306-002")]
    [Requirement("RH8306-004")]
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
    [Requirement("RH8306-003")]
    [Requirement("RH8306-004")]
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
    [Requirement("RH8306-003")]
    [Requirement("RH8306-004")]
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
    [Requirement("RH8306-003")]
    [Requirement("RH8306-004")]
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
    [Requirement("RH8306-001")]
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
    [Requirement("RH8306-001")]
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
    [Requirement("RH8306-001")]
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
    [Requirement("RH8306-001")]
    [Requirement("SHARED-DOC-008")]
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

    /// <summary>
    /// Verifies that the summary of every property is checked whatever its accessibility, also for an explicit
    /// interface implementation and with an inline reference counted as a word, while undocumented, inherited, and
    /// empty summaries and the summaries of fields are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8306-001")]
    [Requirement("RH8306-004")]
    public async Task VerifySummaryOfEveryPropertyIsChecked()
    {
        const string testData = """
                                interface I
                                {
                                    int Total { get; }
                                }

                                class C : I
                                {
                                    /// {|#0:<summary>
                                    /// Gets or sets the size.
                                    /// </summary>|}
                                    public int Size { get; set; }

                                    /// {|#1:<summary>Value <see cref="C"/>.</summary>|}
                                    private int Value { get; }

                                    /// <summary>
                                    /// Current size
                                    /// </summary>
                                    public int Length { get; set; }

                                    /// <inheritdoc/>
                                    public int Count { get; }

                                    /// <summary></summary>
                                    public int Width { get; }

                                    /// {|#2:<summary>Gets the total</summary>|}
                                    int I.Total => 0;

                                    /// <summary>
                                    /// Gets the name.
                                    /// </summary>
                                    public string Name;
                                }
                                """;

        await Verify(testData, Diagnostics(RH8306PropertySummaryMustUseNounPhraseInsteadOfSentenceAnalyzer.DiagnosticId, AnalyzerResources.RH8306MessageFormat, 3));
    }

    /// <summary>
    /// Verifies that summaries ending with a period, an exclamation mark, or a question mark are reported, except a
    /// question mark on a Boolean or nullable Boolean property
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8306-002")]
    public async Task VerifySummariesEndingLikeSentencesAreReported()
    {
        const string testData = """
                                class C
                                {
                                    /// {|#0:<summary>Current size.</summary>|}
                                    public int Size { get; set; }

                                    /// {|#1:<summary>Current size!</summary>|}
                                    public int Length { get; set; }

                                    /// {|#2:<summary>Is it open?</summary>|}
                                    public int Count { get; set; }

                                    /// <summary>Is it open?</summary>
                                    public bool IsOpen { get; set; }

                                    /// <summary>Is it closed?</summary>
                                    public bool? IsClosed { get; set; }
                                }
                                """;

        await Verify(testData, Diagnostics(RH8306PropertySummaryMustUseNounPhraseInsteadOfSentenceAnalyzer.DiagnosticId, AnalyzerResources.RH8306MessageFormat, 3));
    }

    /// <summary>
    /// Verifies that a narrating verb as the first word is reported in any casing and for any property type, while
    /// the verb further along and a first word that only starts with a verb are not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8306-003")]
    public async Task VerifySummariesStartingWithNarratingVerbsAreReported()
    {
        const string testData = """
                                class C
                                {
                                    /// {|#0:<summary>Gets or sets the size</summary>|}
                                    public int Size { get; set; }

                                    /// {|#1:<summary>returns the length</summary>|}
                                    public int Length { get; set; }

                                    /// {|#2:<summary>Indicates whether it is open?</summary>|}
                                    public bool IsOpen { get; set; }

                                    /// <summary>Name that represents the user</summary>
                                    public string Name { get; set; }

                                    /// <summary>Getset value</summary>
                                    public int Value { get; set; }
                                }
                                """;

        await Verify(testData, Diagnostics(RH8306PropertySummaryMustUseNounPhraseInsteadOfSentenceAnalyzer.DiagnosticId, AnalyzerResources.RH8306MessageFormat, 3));
    }

    /// <summary>
    /// Verifies that each narrating verb is reported as the first word of a property summary
    /// </summary>
    /// <param name="verb">Narrating verb that starts the summary</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("Gets")]
    [DataRow("Sets")]
    [DataRow("Returns")]
    [DataRow("Represents")]
    [DataRow("Indicates")]
    [DataRow("Specifies")]
    [DataRow("Determines")]
    [DataRow("Holds")]
    [DataRow("Stores")]
    [Requirement("RH8306-003")]
    public async Task VerifyEveryNarratingVerbIsReported(string verb)
    {
        var testData = $$"""
                         class C
                         {
                             /// {|#0:<summary>{{verb}} the size</summary>|}
                             public int Size { get; set; }
                         }
                         """;

        await Verify(testData, Diagnostics(RH8306PropertySummaryMustUseNounPhraseInsteadOfSentenceAnalyzer.DiagnosticId, AnalyzerResources.RH8306MessageFormat));
    }

    #endregion // Tests
}