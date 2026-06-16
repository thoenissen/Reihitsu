namespace Reihitsu.Analyzer.Test.SelfHosting;

/// <summary>
/// Parsed rule metadata from the analyzer package README
/// </summary>
/// <param name="DiagnosticId">Diagnostic ID</param>
/// <param name="Description">Rule description</param>
/// <param name="HasAnalyzer">Whether the analyzer package ships the analyzer</param>
/// <param name="HasCodeFix">Whether the analyzer package ships a code fix</param>
/// <param name="SupportsFormatter">Whether the formatter can correct the rule automatically</param>
internal sealed record PackageReadmeRuleMetadata(string DiagnosticId, string Description, bool HasAnalyzer, bool HasCodeFix, bool SupportsFormatter);