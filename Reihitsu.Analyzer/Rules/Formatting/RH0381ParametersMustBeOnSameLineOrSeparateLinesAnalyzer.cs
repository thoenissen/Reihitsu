using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0381: Parameters must be on same line or separate lines
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0381ParametersMustBeOnSameLineOrSeparateLinesAnalyzer : DiagnosticAnalyzerBase<RH0381ParametersMustBeOnSameLineOrSeparateLinesAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0381";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0381ParametersMustBeOnSameLineOrSeparateLinesAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0381Title), nameof(AnalyzerResources.RH0381MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzes a parameter list
    /// </summary>
    /// <param name="context">Context</param>
    private void OnParameterList(SyntaxNodeAnalysisContext context)
    {
        var parameterList = (ParameterListSyntax)context.Node;

        if (parameterList.Parameters.Count <= 1)
        {
            return;
        }

        var lineGroups = parameterList.Parameters.GroupBy(parameter => parameter.GetLocation().GetLineSpan().StartLinePosition.Line).ToList();

        if (lineGroups.Count > 1
            && lineGroups.Count < parameterList.Parameters.Count)
        {
            context.ReportDiagnostic(CreateDiagnostic(parameterList.OpenParenToken.GetLocation()));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnParameterList, SyntaxKind.ParameterList);
    }

    #endregion // DiagnosticAnalyzer
}