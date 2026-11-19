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
        : base(DiagnosticId, DiagnosticCategory.Layout)
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

        // The parameters are judged as the analyzed build compiles them; a conditional directive in a gap of the list
        // does not keep it from being reported, and the fix withholds itself there (RH5109-004, SHARED-DIAG-008)
        if (parameterList.Parameters.Count <= 1)
        {
            return;
        }

        // The parameters alone decide whether the list is multi-line; a line break behind the opening or in front of the
        // closing parenthesis does not (RH5109-001)
        if (SyntaxNodeUtilities.IsPartlyWrapped(parameterList.Parameters))
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