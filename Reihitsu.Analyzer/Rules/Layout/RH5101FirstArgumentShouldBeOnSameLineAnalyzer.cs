using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5101: The first argument should be on the same line as the opening parenthesis
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5101FirstArgumentShouldBeOnSameLineAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5101";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5101FirstArgumentShouldBeOnSameLineAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Layout, nameof(AnalyzerResources.RH5101Title), nameof(AnalyzerResources.RH5101MessageFormat))
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