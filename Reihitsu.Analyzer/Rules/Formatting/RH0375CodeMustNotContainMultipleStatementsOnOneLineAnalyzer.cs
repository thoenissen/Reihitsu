using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0375: Code must not contain multiple statements on one line
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0375CodeMustNotContainMultipleStatementsOnOneLineAnalyzer : DiagnosticAnalyzerBase<RH0375CodeMustNotContainMultipleStatementsOnOneLineAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0375";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0375CodeMustNotContainMultipleStatementsOnOneLineAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0375Title), nameof(AnalyzerResources.RH0375MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzes a sequence of sibling statements
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="statements">Statements to inspect</param>
    private void AnalyzeStatements(SyntaxNodeAnalysisContext context, SyntaxList<StatementSyntax> statements)
    {
        StatementSyntax previousStatement = null;

        foreach (var currentStatement in statements)
        {
            if (currentStatement is EmptyStatementSyntax)
            {
                continue;
            }

            if (previousStatement != null
                && previousStatement.GetLocation().GetLineSpan().StartLinePosition.Line
                   == currentStatement.GetLocation().GetLineSpan().StartLinePosition.Line)
            {
                context.ReportDiagnostic(CreateDiagnostic(currentStatement.GetLocation()));
            }

            previousStatement = currentStatement;
        }
    }

    /// <summary>
    /// Analyzes a block node for multiple statements on one line
    /// </summary>
    /// <param name="context">Context</param>
    private void OnBlock(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is BlockSyntax block)
        {
            AnalyzeStatements(context, block.Statements);
        }
    }

    /// <summary>
    /// Analyzes a switch section node for multiple statements on one line
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSwitchSection(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is SwitchSectionSyntax switchSection)
        {
            AnalyzeStatements(context, switchSection.Statements);
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnBlock, SyntaxKind.Block);
        context.RegisterSyntaxNodeAction(OnSwitchSection, SyntaxKind.SwitchSection);
    }

    #endregion // DiagnosticAnalyzer
}