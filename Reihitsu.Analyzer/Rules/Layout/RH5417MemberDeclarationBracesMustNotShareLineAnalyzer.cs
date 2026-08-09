using System;
using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// Compatibility name for <see cref="RH5417FunctionAndAccessorBodyBracesMustNotShareLineAnalyzer"/>
/// </summary>
[Obsolete("Use RH5417FunctionAndAccessorBodyBracesMustNotShareLineAnalyzer instead.")]
public class RH5417MemberDeclarationBracesMustNotShareLineAnalyzer : DiagnosticAnalyzerBase
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

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5417MemberDeclarationBracesMustNotShareLineAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Layout, nameof(AnalyzerResources.RH5417Title), nameof(AnalyzerResources.RH5417MessageFormat))
    {
    }

    #endregion // Constructor

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