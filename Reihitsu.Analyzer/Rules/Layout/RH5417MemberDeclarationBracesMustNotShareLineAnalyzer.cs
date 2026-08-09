using System;
using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// Compatibility name for <see cref="RH5417FunctionAndAccessorBodyBracesMustNotShareLineAnalyzer"/>
/// </summary>
[Obsolete("Use RH5417FunctionAndAccessorBodyBracesMustNotShareLineAnalyzer instead.")]
public class RH5417MemberDeclarationBracesMustNotShareLineAnalyzer : DiagnosticAnalyzer
{
    #region Fields

    /// <summary>
    /// Analyzer implementation
    /// </summary>
    private readonly RH5417FunctionAndAccessorBodyBracesMustNotShareLineAnalyzer _implementation = new();

    #endregion // Fields

    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = RH5417FunctionAndAccessorBodyBracesMustNotShareLineAnalyzer.DiagnosticId;

    #endregion // Constants

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => _implementation.SupportedDiagnostics;

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        _implementation.Initialize(context);
    }

    #endregion // DiagnosticAnalyzer
}