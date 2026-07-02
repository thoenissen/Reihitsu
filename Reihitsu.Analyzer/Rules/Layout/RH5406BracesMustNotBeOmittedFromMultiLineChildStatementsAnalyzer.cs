using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5406: Braces must not be omitted from multi-line child statements
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5406BracesMustNotBeOmittedFromMultiLineChildStatementsAnalyzer : DiagnosticAnalyzerBase<RH5406BracesMustNotBeOmittedFromMultiLineChildStatementsAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5406";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5406BracesMustNotBeOmittedFromMultiLineChildStatementsAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Layout, nameof(AnalyzerResources.RH5406Title), nameof(AnalyzerResources.RH5406MessageFormat))
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

        foreach (var ifStatement in root.DescendantNodes().OfType<IfStatementSyntax>())
        {
            AnalyzeChildStatement(context, ifStatement.Statement);

            if (ifStatement.Else is { Statement: { } elseStatement }
                && elseStatement is IfStatementSyntax == false)
            {
                AnalyzeChildStatement(context, elseStatement);
            }
        }
    }

    /// <summary>
    /// Reports a diagnostic when the child statement spans multiple lines and is not enclosed in braces
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="statement">Child statement of the control-flow statement</param>
    private void AnalyzeChildStatement(SyntaxTreeAnalysisContext context, StatementSyntax statement)
    {
        if (statement is BlockSyntax)
        {
            return;
        }

        // Single-line brace-less child statements are reported by RH5405 to avoid double-reporting
        var lineSpan = statement.GetLocation().GetLineSpan();

        if (lineSpan.StartLinePosition.Line != lineSpan.EndLinePosition.Line)
        {
            context.ReportDiagnostic(CreateDiagnostic(statement.GetLocation()));
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