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
/// RH5105: Opening parenthesis must be on declaration line
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5105OpeningParenthesisMustBeOnDeclarationLineAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5105";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5105OpeningParenthesisMustBeOnDeclarationLineAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Layout, nameof(AnalyzerResources.RH5105Title), nameof(AnalyzerResources.RH5105MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Reports a diagnostic when the opening parenthesis of a parameter list is not on the same line as the preceding declaration token
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="parameterList">Parameter list to inspect</param>
    private void CheckParameterList(SyntaxNodeAnalysisContext context, ParameterListSyntax parameterList)
    {
        var openParenToken = parameterList.OpenParenToken;
        var previousToken = openParenToken.GetPreviousToken();

        if (previousToken.GetLocation().GetLineSpan().StartLinePosition.Line == openParenToken.GetLocation().GetLineSpan().StartLinePosition.Line)
        {
            return;
        }

        // The formatter refuses to pull the opening parenthesis onto the declaration line when a comment or
        // directive sits in the gap, so flagging that shape would leave a permanent diagnostic.
        if (SyntaxTriviaUtilities.WouldJoinAcrossUnjoinableTrivia(previousToken, openParenToken))
        {
            return;
        }

        context.ReportDiagnostic(CreateDiagnostic(openParenToken.GetLocation()));
    }

    /// <summary>
    /// Analyzes a parameter list
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="errorVerdicts">Syntax error verdict per tree</param>
    private void OnParameterList(SyntaxNodeAnalysisContext context, SyntaxTreeErrorVerdictCache errorVerdicts)
    {
        if (context.Node is not ParameterListSyntax parameterList
            || ParameterListParentPolicy.IsOpeningParenthesisCovered(parameterList) == false)
        {
            return;
        }

        // The rule withholds itself for a whole file with malformed syntax, so the verdict covers the entire tree
        // rather than the analyzed parameter list.
        if (errorVerdicts.ContainsError(context.Node.SyntaxTree, context.CancellationToken))
        {
            return;
        }

        CheckParameterList(context, parameterList);
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