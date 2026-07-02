using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5109: Parameters must be on same line or separate lines
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5109ParametersMustBeOnSameLineOrSeparateLinesAnalyzer : DiagnosticAnalyzerBase<RH5109ParametersMustBeOnSameLineOrSeparateLinesAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5109";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5109ParametersMustBeOnSameLineOrSeparateLinesAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Layout, nameof(AnalyzerResources.RH5109Title), nameof(AnalyzerResources.RH5109MessageFormat))
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
        if (context.Node is not ParameterListSyntax parameterList)
        {
            return;
        }

        if (parameterList.Parameters.Count <= 1)
        {
            return;
        }

        var openParenLine = parameterList.OpenParenToken.GetLocation().GetLineSpan().StartLinePosition.Line;
        var closeParenLine = parameterList.CloseParenToken.GetLocation().GetLineSpan().StartLinePosition.Line;

        if (openParenLine == closeParenLine)
        {
            return;
        }

        var startLines = new HashSet<int>();

        foreach (var parameter in parameterList.Parameters)
        {
            var parameterStartLine = parameter.GetLocation().GetLineSpan().StartLinePosition.Line;

            if (startLines.Add(parameterStartLine) == false)
            {
                context.ReportDiagnostic(CreateDiagnostic(parameterList.OpenParenToken.GetLocation()));

                return;
            }
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