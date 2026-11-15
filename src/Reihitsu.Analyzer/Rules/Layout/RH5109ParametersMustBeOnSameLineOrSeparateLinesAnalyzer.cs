using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5109: Parameters must be on same line or separate lines
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5109ParametersMustBeOnSameLineOrSeparateLinesAnalyzer : DiagnosticAnalyzerBase
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

        // A conditional directive in a gap of the list lets another build configuration compile other parameters
        // (RH5109-004, SHARED-DIAG-008)
        if (parameterList.Parameters.Count <= 1
            || SyntaxTriviaUtilities.ContainsConditionalCompilationBoundaryInElementGaps(parameterList.Parameters.GetWithSeparators(), parameterList.CloseParenToken))
        {
            return;
        }

        // The parameters alone decide whether the list is multi-line: they start on different lines, or one of them
        // spans lines. A line break behind the opening or in front of the closing parenthesis does not (RH5109-001)
        var startLines = new HashSet<int>();
        var isMultiLine = false;
        var sharesLine = false;

        foreach (var parameter in parameterList.Parameters)
        {
            var parameterLineSpan = parameter.GetLocation().GetLineSpan();

            isMultiLine |= parameterLineSpan.StartLinePosition.Line != parameterLineSpan.EndLinePosition.Line;
            sharesLine |= startLines.Add(parameterLineSpan.StartLinePosition.Line) == false;
        }

        if ((isMultiLine || startLines.Count > 1)
            && sharesLine)
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