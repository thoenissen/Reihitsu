using System;
using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// Compatibility name for <see cref="RH5020CommentsShouldBePrecededByABlankLineAnalyzer"/>
/// </summary>
[Obsolete("Use RH5020CommentsShouldBePrecededByABlankLineAnalyzer instead.")]
public class RH5020SingleLineCommentsShouldBePrecededByABlankLineAnalyzer : DiagnosticAnalyzerBase
{
    #region Fields

    /// <summary>
    /// Analyzer implementation
    /// </summary>
    private readonly RH5020CommentsShouldBePrecededByABlankLineAnalyzer _implementation = new();

    #endregion // Fields

    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = RH5020CommentsShouldBePrecededByABlankLineAnalyzer.DiagnosticId;

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5020SingleLineCommentsShouldBePrecededByABlankLineAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Layout, nameof(AnalyzerResources.RH5020Title), nameof(AnalyzerResources.RH5020MessageFormat))
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