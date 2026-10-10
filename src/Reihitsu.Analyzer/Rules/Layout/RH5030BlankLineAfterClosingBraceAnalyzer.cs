using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5030: Require a blank line after a closing brace before the next statement
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5030BlankLineAfterClosingBraceAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5030";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5030BlankLineAfterClosingBraceAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Layout, nameof(AnalyzerResources.RH5030Title), nameof(AnalyzerResources.RH5030MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzes a list of statements and reports a diagnostic when a statement that follows a closing
    /// brace is not preceded by a blank line
    /// </summary>
    /// <param name="context">Analysis context</param>
    /// <param name="statements">Statements to analyze</param>
    private void AnalyzeStatements(SyntaxNodeAnalysisContext context, SyntaxList<StatementSyntax> statements)
    {
        for (var statementIndex = 0; statementIndex < statements.Count - 1; statementIndex++)
        {
            var current = statements[statementIndex];

            if (BlankLineSpacingPolicy.RequiresBlankLineAfterClosingBrace(current, statements[statementIndex + 1]))
            {
                context.ReportDiagnostic(CreateDiagnostic(current.GetLastToken().GetLocation()));
            }
        }
    }

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

        AnalyzeStatements(context, block.Statements);
    }

    /// <summary>
    /// Analyzes a switch section
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSwitchSection(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not SwitchSectionSyntax switchSection)
        {
            return;
        }

        AnalyzeStatements(context, switchSection.Statements);
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