using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0801: Switch expression braces should be anchored
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0801SwitchExpressionBracesShouldBeAnchoredAnalyzer : DiagnosticAnalyzerBase<RH0801SwitchExpressionBracesShouldBeAnchoredAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0801";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0801SwitchExpressionBracesShouldBeAnchoredAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0801Title), nameof(AnalyzerResources.RH0801MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether the token is aligned to the expected column
    /// </summary>
    /// <param name="token">Token</param>
    /// <param name="expectedColumn">Expected column</param>
    /// <returns><see langword="true"/> if the token matches the formatter contract</returns>
    private static bool IsTokenAligned(SyntaxToken token, int expectedColumn)
    {
        var linePosition = token.GetLocation().GetLineSpan().StartLinePosition;

        return linePosition.Character == expectedColumn;
    }

    /// <summary>
    /// Analyzing all <see cref="SwitchExpressionSyntax"/> occurrences
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSwitchExpression(SyntaxNodeAnalysisContext context)
    {
        var switchExpression = (SwitchExpressionSyntax)context.Node;
        var anchorColumn = switchExpression.GoverningExpression.GetFirstToken()
                                                               .GetLocation()
                                                               .GetLineSpan()
                                                               .StartLinePosition
                                                               .Character;
        var armColumn = anchorColumn + 4;

        if (IsTokenAligned(switchExpression.OpenBraceToken, anchorColumn) == false
            || IsTokenAligned(switchExpression.CloseBraceToken, anchorColumn) == false
            || switchExpression.Arms.Any(obj => IsTokenAligned(obj.GetFirstToken(), armColumn) == false))
        {
            context.ReportDiagnostic(CreateDiagnostic(switchExpression.GetLocation()));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnSwitchExpression, Microsoft.CodeAnalysis.CSharp.SyntaxKind.SwitchExpression);
    }

    #endregion // DiagnosticAnalyzer
}