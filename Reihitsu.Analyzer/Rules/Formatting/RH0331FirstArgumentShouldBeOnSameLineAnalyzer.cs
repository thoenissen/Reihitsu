using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0331: The first argument should be on the same line as the opening parenthesis
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0331FirstArgumentShouldBeOnSameLineAnalyzer : DiagnosticAnalyzerBase<RH0331FirstArgumentShouldBeOnSameLineAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0331";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0331FirstArgumentShouldBeOnSameLineAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0331Title), nameof(AnalyzerResources.RH0331MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzing argument lists to ensure the first argument is on the same line as the opening parenthesis
    /// </summary>
    /// <param name="context">Context</param>
    private void OnArgumentList(SyntaxNodeAnalysisContext context)
    {
        var argumentList = (ArgumentListSyntax)context.Node;

        if (argumentList.Arguments.Count == 0)
        {
            return;
        }

        var openParenLine = argumentList.OpenParenToken.GetLocation().GetLineSpan().StartLinePosition.Line;
        var firstArgumentLine = argumentList.Arguments[0].GetLocation().GetLineSpan().StartLinePosition.Line;

        if (firstArgumentLine != openParenLine)
        {
            context.ReportDiagnostic(CreateDiagnostic(argumentList.Arguments[0].GetLocation()));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnArgumentList, SyntaxKind.ArgumentList);
    }

    #endregion // DiagnosticAnalyzer
}