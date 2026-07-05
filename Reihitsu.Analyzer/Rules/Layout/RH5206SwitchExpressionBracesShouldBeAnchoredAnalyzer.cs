using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5206: Switch expression braces should be anchored
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5206SwitchExpressionBracesShouldBeAnchoredAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5206";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5206SwitchExpressionBracesShouldBeAnchoredAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Layout, nameof(AnalyzerResources.RH5206Title), nameof(AnalyzerResources.RH5206MessageFormat))
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
    /// Determines whether the token is the first token on its line
    /// </summary>
    /// <param name="token">Token</param>
    /// <returns><see langword="true"/> if no preceding token shares the token's line</returns>
    private static bool IsFirstTokenOnLine(SyntaxToken token)
    {
        var previousToken = token.GetPreviousToken();

        if (previousToken.IsKind(SyntaxKind.None))
        {
            return true;
        }

        var previousEndLine = previousToken.GetLocation().GetLineSpan().EndLinePosition.Line;
        var tokenStartLine = token.GetLocation().GetLineSpan().StartLinePosition.Line;

        return previousEndLine != tokenStartLine;
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

        // Only arms that start their own line are anchored by the formatter; arms that share a line with a
        // previous arm are never split, so checking their column would flag shapes the formatter leaves intact.
        if (IsTokenAligned(switchExpression.OpenBraceToken, anchorColumn) == false
            || IsTokenAligned(switchExpression.CloseBraceToken, anchorColumn) == false
            || switchExpression.Arms.Any(arm => IsFirstTokenOnLine(arm.GetFirstToken())
                                                && IsTokenAligned(arm.GetFirstToken(), armColumn) == false))
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