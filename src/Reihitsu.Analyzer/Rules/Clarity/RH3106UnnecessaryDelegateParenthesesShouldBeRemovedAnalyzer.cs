using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Clarity;

/// <summary>
/// RH3106: Unnecessary delegate parentheses should be removed
/// </summary>
[Microsoft.CodeAnalysis.Diagnostics.DiagnosticAnalyzer(Microsoft.CodeAnalysis.LanguageNames.CSharp)]
public class RH3106UnnecessaryDelegateParenthesesShouldBeRemovedAnalyzer : EmptyParenthesesAnalyzerBase<AnonymousMethodExpressionSyntax>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH3106";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH3106UnnecessaryDelegateParenthesesShouldBeRemovedAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Clarity, nameof(AnalyzerResources.RH3106Title), nameof(AnalyzerResources.RH3106MessageFormat), SyntaxKind.AnonymousMethodExpression)
    {
    }

    #endregion // Constructor

    #region EmptyParenthesesAnalyzerBase

    /// <inheritdoc/>
    protected override bool HasUnnecessaryParentheses(AnonymousMethodExpressionSyntax node)
    {
        return node.ParameterList is { Parameters.Count: 0 };
    }

    /// <inheritdoc/>
    protected override Location GetDiagnosticLocation(AnonymousMethodExpressionSyntax node)
    {
        return node.ParameterList?.GetLocation() ?? node.GetLocation();
    }

    #endregion // EmptyParenthesesAnalyzerBase
}