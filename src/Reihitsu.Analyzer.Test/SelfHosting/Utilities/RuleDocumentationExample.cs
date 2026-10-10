namespace Reihitsu.Analyzer.Test.SelfHosting.Utilities;

/// <summary>
/// A parsed rule document violation/correction example pair
/// </summary>
/// <param name="DocumentPath">Absolute path to the rule document</param>
/// <param name="DiagnosticId">Diagnostic ID the document describes</param>
/// <param name="HasCodeFix">Whether the document's metadata table advertises a code fix</param>
/// <param name="Violation">The <c>### Violation</c> example source</param>
/// <param name="Correction">The <c>### Correction</c> example source</param>
internal sealed record RuleDocumentationExample(string DocumentPath, string DiagnosticId, bool HasCodeFix, string Violation, string Correction);