using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5403: Statement must not be on a single line
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5403StatementMustNotBeOnSingleLineAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5403";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5403StatementMustNotBeOnSingleLineAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Layout, nameof(AnalyzerResources.RH5403Title), nameof(AnalyzerResources.RH5403MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzes the syntax tree
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        var root = context.Tree.GetRoot(context.CancellationToken);

        foreach (var block in root.DescendantNodes().OfType<BlockSyntax>())
        {
            if (block.Parent is not IfStatementSyntax
                && block.Parent is not ElseClauseSyntax
                && block.Parent is not WhileStatementSyntax
                && block.Parent is not ForStatementSyntax
                && block.Parent is not ForEachStatementSyntax)
            {
                continue;
            }

            if (block.Statements.Count > 0
                && block.OpenBraceToken.GetLocation().GetLineSpan().StartLinePosition.Line == block.CloseBraceToken.GetLocation().GetLineSpan().StartLinePosition.Line)
            {
                context.ReportDiagnostic(CreateDiagnostic(block.OpenBraceToken.GetLocation()));
            }
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxTreeAction(OnSyntaxTree);
    }

    #endregion // DiagnosticAnalyzer
}