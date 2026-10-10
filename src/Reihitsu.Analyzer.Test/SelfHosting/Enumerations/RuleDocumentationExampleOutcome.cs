namespace Reihitsu.Analyzer.Test.SelfHosting.Enumerations;

/// <summary>
/// Outcome of verifying a single rule document's violation/correction example
/// </summary>
internal enum RuleDocumentationExampleOutcome
{
    /// <summary>
    /// All applicable criteria were satisfied
    /// </summary>
    Passed,

    /// <summary>
    /// No shipped analyzer reports the document's diagnostic ID
    /// </summary>
    NoMatchingAnalyzer,

    /// <summary>
    /// The metadata table advertises a code fix, but none is shipped for the diagnostic ID
    /// </summary>
    NoMatchingCodeFix,

    /// <summary>
    /// The violation or correction example does not parse as a standalone compilation unit
    /// </summary>
    ExampleDoesNotParse,

    /// <summary>
    /// The violation example does not report the document's diagnostic ID
    /// </summary>
    ViolationDoesNotReport,

    /// <summary>
    /// The correction example still reports the document's diagnostic ID
    /// </summary>
    CorrectionStillReports,

    /// <summary>
    /// Applying the code fix did not converge within the iteration budget
    /// </summary>
    CodeFixDoesNotConverge,

    /// <summary>
    /// Applying the code fix produced text that differs from the documented correction
    /// </summary>
    CodeFixOutputDiffers
}