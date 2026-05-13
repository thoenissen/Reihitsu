using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0395: Break statements should not be inside explicit switch case blocks
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0395BreakStatementsShouldNotBeInsideExplicitSwitchCaseBlocksAnalyzer : DiagnosticAnalyzerBase<RH0395BreakStatementsShouldNotBeInsideExplicitSwitchCaseBlocksAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0395";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0395BreakStatementsShouldNotBeInsideExplicitSwitchCaseBlocksAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0395Title), nameof(AnalyzerResources.RH0395MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzes break statements
    /// </summary>
    /// <param name="context">Context</param>
    private void OnBreakStatement(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not BreakStatementSyntax { Parent: BlockSyntax { Parent: SwitchSectionSyntax } } breakStatement)
        {
            return;
        }

        context.ReportDiagnostic(CreateDiagnostic(breakStatement.BreakKeyword.GetLocation()));
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnBreakStatement, SyntaxKind.BreakStatement);
    }

    #endregion // DiagnosticAnalyzer
}