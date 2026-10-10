using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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
    /// Analyzes an if-statement
    /// </summary>
    /// <param name="context">Context</param>
    private void OnIfStatement(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not IfStatementSyntax statement)
        {
            return;
        }

        if (statement.Else == null)
        {
            return;
        }

        if (statement.Else.Statement is IfStatementSyntax)
        {
            return;
        }

        var ifHasBraces = statement.Statement is BlockSyntax;
        var elseHasBraces = statement.Else.Statement is BlockSyntax;

        if (ifHasBraces != elseHasBraces)
        {
            var target = elseHasBraces ? statement.Statement : statement.Else.Statement;

            context.ReportDiagnostic(CreateDiagnostic(target.GetLocation()));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnIfStatement, SyntaxKind.IfStatement);
    }

    #endregion // DiagnosticAnalyzer
}