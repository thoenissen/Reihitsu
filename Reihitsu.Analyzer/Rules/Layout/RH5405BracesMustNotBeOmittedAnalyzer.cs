using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5405: Braces must not be omitted
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5405BracesMustNotBeOmittedAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5405";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5405BracesMustNotBeOmittedAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Layout, nameof(AnalyzerResources.RH5405Title), nameof(AnalyzerResources.RH5405MessageFormat))
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
            // The if-body is reported here. An else-body is reported when its statement is not itself an
            // if-statement, because a nested "else if" is handled by that inner if-statement instead.
            AnalyzeChildStatement(context, ifStatement.Statement);

            if (ifStatement.Else is { Statement: { } elseStatement }
                && elseStatement is IfStatementSyntax == false)
            {
                AnalyzeChildStatement(context, elseStatement);
            }
        }
    }

    /// <summary>
    /// Reports a diagnostic when the child statement is a single-line statement without braces
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="statement">Child statement of the control-flow statement</param>
    private void AnalyzeChildStatement(SyntaxTreeAnalysisContext context, StatementSyntax statement)
    {
        if (statement is BlockSyntax)
        {
            return;
        }

        // Multi-line brace-less child statements are reported by RH5406 to avoid double-reporting
        var lineSpan = statement.GetLocation().GetLineSpan();

        if (lineSpan.StartLinePosition.Line == lineSpan.EndLinePosition.Line)
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