using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Reihitsu.Tooling.Test.Helpers;

/// <summary>
/// An analyzer that throws while analyzing a syntax tree so Roslyn reports AD0001
/// </summary>
internal sealed class ThrowingFakeAnalyzer : DiagnosticAnalyzer
{
    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [new DiagnosticDescriptor(FakeDiagnostic.Id, "Title", "Message", "Testing", DiagnosticSeverity.Warning, isEnabledByDefault: true)];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxTreeAction(_ => throw new InvalidOperationException("Expected analyzer failure."));
    }

    #endregion // DiagnosticAnalyzer
}