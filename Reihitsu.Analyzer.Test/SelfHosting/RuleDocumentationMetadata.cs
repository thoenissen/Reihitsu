namespace Reihitsu.Analyzer.Test.SelfHosting;

/// <summary>
/// Parsed rule title metadata from rule documentation
/// </summary>
/// <param name="DiagnosticId">Diagnostic ID</param>
/// <param name="Title">Rule title</param>
internal sealed record RuleDocumentationMetadata(string DiagnosticId, string Title);