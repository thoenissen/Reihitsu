using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Design;

/// <summary>
/// RH0110: Unnecessary delegate parentheses should be removed
/// </summary>
[Microsoft.CodeAnalysis.Diagnostics.DiagnosticAnalyzer(Microsoft.CodeAnalysis.LanguageNames.CSharp)]
public class RH0110UnnecessaryDelegateParenthesesShouldBeRemovedAnalyzer : EmptyParenthesesAnalyzerBase<RH0110UnnecessaryDelegateParenthesesShouldBeRemovedAnalyzer, AnonymousMethodExpressionSyntax>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0110";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0110UnnecessaryDelegateParenthesesShouldBeRemovedAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Design, nameof(AnalyzerResources.RH0110Title), nameof(AnalyzerResources.RH0110MessageFormat), SyntaxKind.AnonymousMethodExpression)
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