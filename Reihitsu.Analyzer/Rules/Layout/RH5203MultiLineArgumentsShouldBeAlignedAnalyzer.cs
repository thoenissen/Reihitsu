using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5203: Multi-line arguments should be aligned
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5203MultiLineArgumentsShouldBeAlignedAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5203";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5203MultiLineArgumentsShouldBeAlignedAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Layout, nameof(AnalyzerResources.RH5203Title), nameof(AnalyzerResources.RH5203MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzing argument lists to ensure multi-line arguments are aligned to the column after the opening parenthesis
    /// </summary>
    /// <param name="context">Context</param>
    private void OnArgumentList(SyntaxNodeAnalysisContext context)
    {
        var argumentList = (ArgumentListSyntax)context.Node;

        if (argumentList.Arguments.Count < 2)
        {
            return;
        }

        var openParenLineSpan = argumentList.OpenParenToken.GetLocation().GetLineSpan();
        var openParenLine = openParenLineSpan.StartLinePosition.Line;
        var closeParenLine = argumentList.CloseParenToken.GetLocation().GetLineSpan().StartLinePosition.Line;

        if (openParenLine == closeParenLine)
        {
            return;
        }

        var expectedColumn = openParenLineSpan.StartLinePosition.Character + 1;

        foreach (var argument in argumentList.Arguments)
        {
            var argumentLineSpan = argument.GetLocation().GetLineSpan();
            var argumentStartLine = argumentLineSpan.StartLinePosition.Line;

            if (argumentStartLine == openParenLine)
            {
                continue;
            }

            if (argumentLineSpan.StartLinePosition.Character != expectedColumn)
            {
                context.ReportDiagnostic(CreateDiagnostic(argument.GetLocation()));
            }
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