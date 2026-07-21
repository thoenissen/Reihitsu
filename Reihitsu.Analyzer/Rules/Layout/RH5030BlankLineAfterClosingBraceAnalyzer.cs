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
    /// <param name="inSwitchSection">Whether the statements belong to a switch section</param>
    private void AnalyzeStatements(SyntaxNodeAnalysisContext context, SyntaxList<StatementSyntax> statements, bool inSwitchSection = false)
    {
        for (var statementIndex = 0; statementIndex < statements.Count - 1; statementIndex++)
        {
            var current = statements[statementIndex];
            var next = statements[statementIndex + 1];

            var lastToken = current.GetLastToken();

            if (lastToken.IsKind(SyntaxKind.CloseBraceToken) == false)
            {
                continue;
            }

            // Inside a switch section, no blank line is required before a break statement
            if (inSwitchSection && next is BreakStatementSyntax)
            {
                continue;
            }

            var nextFirstToken = next.GetFirstToken();

            if (TokenGapAnalysis.Between(lastToken, nextFirstToken).BlankLineCount > 0)
            {
                continue;
            }

            var lastLine = lastToken.GetLocation().GetLineSpan().EndLinePosition.Line;
            var nextLine = nextFirstToken.GetLocation().GetLineSpan().StartLinePosition.Line;

            // Skip pairs that are on the same line (e.g. inline blocks like if (x) { } Consume();)
            if (lastLine >= nextLine)
            {
                continue;
            }

            context.ReportDiagnostic(CreateDiagnostic(lastToken.GetLocation()));
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

        AnalyzeStatements(context, block.Statements, inSwitchSection: block.Parent is SwitchSectionSyntax);
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

        AnalyzeStatements(context, switchSection.Statements, inSwitchSection: true);
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