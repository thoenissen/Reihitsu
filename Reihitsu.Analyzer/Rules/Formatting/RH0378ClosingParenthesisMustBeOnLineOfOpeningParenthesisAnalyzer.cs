using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0378: Closing parenthesis must be on line of opening parenthesis.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0378ClosingParenthesisMustBeOnLineOfOpeningParenthesisAnalyzer : DiagnosticAnalyzerBase<RH0378ClosingParenthesisMustBeOnLineOfOpeningParenthesisAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0378";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0378ClosingParenthesisMustBeOnLineOfOpeningParenthesisAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0378Title), nameof(AnalyzerResources.RH0378MessageFormat))
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

        foreach (var parameterList in root.DescendantNodes().OfType<MethodDeclarationSyntax>().Select(method => method.ParameterList))
        {
            if (parameterList.OpenParenToken.GetLocation().GetLineSpan().StartLinePosition.Line != parameterList.CloseParenToken.GetLocation().GetLineSpan().StartLinePosition.Line)
            {
                context.ReportDiagnostic(CreateDiagnostic(parameterList.CloseParenToken.GetLocation()));
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