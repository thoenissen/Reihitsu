using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5106: Closing parenthesis must be on line of last argument
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5106ClosingParenthesisMustBeOnLineOfLastArgumentAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5106";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5106ClosingParenthesisMustBeOnLineOfLastArgumentAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Layout, nameof(AnalyzerResources.RH5106Title), nameof(AnalyzerResources.RH5106MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Checks a parameter list and reports diagnostics when required
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="parameterList">Parameter list</param>
    private void AnalyzeParameterList(SyntaxNodeAnalysisContext context, ParameterListSyntax parameterList)
    {
        var closeParenLine = parameterList.CloseParenToken.GetLocation().GetLineSpan().StartLinePosition.Line;
        var expectedLine = parameterList.Parameters.Count > 0
                               ? parameterList.Parameters[parameterList.Parameters.Count - 1].GetLocation().GetLineSpan().EndLinePosition.Line
                               : parameterList.OpenParenToken.GetLocation().GetLineSpan().StartLinePosition.Line;

        if (closeParenLine == expectedLine)
        {
            return;
        }

        // The formatter refuses to pull the closing parenthesis onto the last argument's line when a comment or
        // directive sits in the gap, so flagging that shape would leave a permanent diagnostic.
        if (SyntaxTriviaUtilities.WouldJoinAcrossUnjoinableTrivia(parameterList.CloseParenToken.GetPreviousToken(), parameterList.CloseParenToken))
        {
            return;
        }

        context.ReportDiagnostic(CreateDiagnostic(parameterList.CloseParenToken.GetLocation()));
    }

    /// <summary>
    /// Analyzes a parameter list
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="errorVerdicts">Syntax error verdict per tree</param>
    private void OnParameterList(SyntaxNodeAnalysisContext context, SyntaxTreeErrorVerdictCache errorVerdicts)
    {
        if (context.Node is not ParameterListSyntax parameterList
            || ParameterListParentPolicy.IsClosingParenthesisCovered(parameterList) == false)
        {
            return;
        }

        // The rule withholds itself for a whole file with malformed syntax, so the verdict covers the entire tree
        // rather than the analyzed parameter list.
        if (errorVerdicts.ContainsError(context.Node.SyntaxTree, context.CancellationToken))
        {
            return;
        }

        AnalyzeParameterList(context, parameterList);
    }

    /// <summary>
    /// Starts the analysis of a compilation
    /// </summary>
    /// <param name="context">Context</param>
    private void OnCompilationStart(CompilationStartAnalysisContext context)
    {
        var errorVerdicts = new SyntaxTreeErrorVerdictCache();

        context.RegisterSyntaxNodeAction(nodeContext => OnParameterList(nodeContext, errorVerdicts), SyntaxKind.ParameterList);
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterCompilationStartAction(OnCompilationStart);
    }

    #endregion // DiagnosticAnalyzer
}