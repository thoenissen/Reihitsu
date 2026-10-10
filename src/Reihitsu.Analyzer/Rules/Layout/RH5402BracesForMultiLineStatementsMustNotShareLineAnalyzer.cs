using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5402: Braces for multi-line statements must not share line
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5402BracesForMultiLineStatementsMustNotShareLineAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5402";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5402BracesForMultiLineStatementsMustNotShareLineAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Layout, nameof(AnalyzerResources.RH5402Title), nameof(AnalyzerResources.RH5402MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzes a block
    /// </summary>
    /// <param name="context">Context</param>
    private void OnBlock(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not BlockSyntax block)
        {
            return;
        }

        if (StatementBlockParentPolicy.IsCovered(block) == false)
        {
            return;
        }

        var braceLine = block.OpenBraceToken.GetLocation().GetLineSpan().StartLinePosition.Line;
        var previousLine = block.OpenBraceToken.GetPreviousToken().GetLocation().GetLineSpan().EndLinePosition.Line;
        var closeLine = block.CloseBraceToken.GetLocation().GetLineSpan().StartLinePosition.Line;

        if (braceLine == previousLine && closeLine > braceLine)
        {
            context.ReportDiagnostic(CreateDiagnostic(block.OpenBraceToken.GetLocation()));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnBlock, SyntaxKind.Block);
    }

    #endregion // DiagnosticAnalyzer
}