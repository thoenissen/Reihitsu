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
public class RH5405BracesMustNotBeOmittedAnalyzer : DiagnosticAnalyzerBase<RH5405BracesMustNotBeOmittedAnalyzer>
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

        foreach (var statement in root.DescendantNodes()
                                      .OfType<IfStatementSyntax>()
                                      .Select(statement => statement.Statement)
                                      .Where(statement => statement is BlockSyntax == false))
        {
            if (statement is BlockSyntax == false)
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