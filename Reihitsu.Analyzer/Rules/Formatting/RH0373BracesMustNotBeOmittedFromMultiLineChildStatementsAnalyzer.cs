using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0373: Braces must not be omitted from multi-line child statements.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0373BracesMustNotBeOmittedFromMultiLineChildStatementsAnalyzer : DiagnosticAnalyzerBase<RH0373BracesMustNotBeOmittedFromMultiLineChildStatementsAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0373";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0373BracesMustNotBeOmittedFromMultiLineChildStatementsAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0373Title), nameof(AnalyzerResources.RH0373MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzes the syntax tree.
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        var root = context.Tree.GetRoot(context.CancellationToken);

        foreach (var statement in root.DescendantNodes().OfType<IfStatementSyntax>().Select(statement => statement.Statement))
        {
            if (statement is BlockSyntax)
            {
                continue;
            }

            var lineSpan = statement.GetLocation().GetLineSpan();

            if (lineSpan.StartLinePosition.Line != lineSpan.EndLinePosition.Line)
            {
                context.ReportDiagnostic(CreateDiagnostic(statement.GetLocation()));
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