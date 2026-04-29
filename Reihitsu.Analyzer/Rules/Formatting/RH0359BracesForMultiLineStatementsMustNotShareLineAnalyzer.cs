using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0359: Braces for multi-line statements must not share line
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0359BracesForMultiLineStatementsMustNotShareLineAnalyzer : DiagnosticAnalyzerBase<RH0359BracesForMultiLineStatementsMustNotShareLineAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0359";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0359BracesForMultiLineStatementsMustNotShareLineAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0359Title), nameof(AnalyzerResources.RH0359MessageFormat))
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
                && block.Parent is not ForEachStatementSyntax
                && block.Parent is not UsingStatementSyntax
                && block.Parent is not LockStatementSyntax
                && block.Parent is not FixedStatementSyntax
                && block.Parent is not DoStatementSyntax)
            {
                continue;
            }

            var braceLine = block.OpenBraceToken.GetLocation().GetLineSpan().StartLinePosition.Line;
            var previousLine = block.OpenBraceToken.GetPreviousToken().GetLocation().GetLineSpan().EndLinePosition.Line;
            var closeLine = block.CloseBraceToken.GetLocation().GetLineSpan().StartLinePosition.Line;

            if (braceLine == previousLine && closeLine > braceLine)
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