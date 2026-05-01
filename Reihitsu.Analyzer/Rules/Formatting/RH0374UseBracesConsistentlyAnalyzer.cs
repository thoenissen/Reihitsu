using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0374: Use braces consistently
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0374UseBracesConsistentlyAnalyzer : DiagnosticAnalyzerBase<RH0374UseBracesConsistentlyAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0374";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0374UseBracesConsistentlyAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0374Title), nameof(AnalyzerResources.RH0374MessageFormat))
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