using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Organization;

/// <summary>
/// RH7501: Break statements should not be inside explicit switch case blocks
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH7501BreakStatementsShouldNotBeInsideExplicitSwitchCaseBlocksAnalyzer : DiagnosticAnalyzerBase<RH7501BreakStatementsShouldNotBeInsideExplicitSwitchCaseBlocksAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH7501";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH7501BreakStatementsShouldNotBeInsideExplicitSwitchCaseBlocksAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Organization, nameof(AnalyzerResources.RH7501Title), nameof(AnalyzerResources.RH7501MessageFormat))
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