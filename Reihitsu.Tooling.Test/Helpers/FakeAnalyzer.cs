using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Reihitsu.Tooling.Test.Helpers;

/// <summary>
/// An analyzer that reports <see cref="FakeDiagnostic.Id"/> and analyzes nothing. It deliberately carries no
/// <see cref="DiagnosticAnalyzerAttribute"/>: the resolver reads <see cref="SupportedDiagnostics"/> only, and the
/// attribute would make this test assembly look like a shipped compiler extension to the Roslyn analyzer rules
/// </summary>
internal sealed class FakeAnalyzer : DiagnosticAnalyzer
{
    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [new DiagnosticDescriptor(FakeDiagnostic.Id, "Title", "Message", "Testing", DiagnosticSeverity.Warning, isEnabledByDefault: true)];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
    }

    #endregion // DiagnosticAnalyzer
}