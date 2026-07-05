using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5407: Use braces consistently
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5407UseBracesConsistentlyAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5407";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5407UseBracesConsistentlyAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Layout, nameof(AnalyzerResources.RH5407Title), nameof(AnalyzerResources.RH5407MessageFormat))
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

        foreach (var statement in root.DescendantNodes().OfType<IfStatementSyntax>())
        {
            if (statement.Else == null)
            {
                continue;
            }

            if (statement.Else.Statement is IfStatementSyntax)
            {
                continue;
            }

            var ifHasBraces = statement.Statement is BlockSyntax;
            var elseHasBraces = statement.Else.Statement is BlockSyntax;

            if (ifHasBraces != elseHasBraces)
            {
                var target = elseHasBraces ? statement.Statement : statement.Else.Statement;

                context.ReportDiagnostic(CreateDiagnostic(target.GetLocation()));
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