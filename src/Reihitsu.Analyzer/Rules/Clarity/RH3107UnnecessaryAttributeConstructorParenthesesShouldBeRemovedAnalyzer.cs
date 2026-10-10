using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Clarity;

/// <summary>
/// RH3107: Unnecessary attribute constructor parentheses should be removed
/// </summary>
[Microsoft.CodeAnalysis.Diagnostics.DiagnosticAnalyzer(Microsoft.CodeAnalysis.LanguageNames.CSharp)]
public class RH3107UnnecessaryAttributeConstructorParenthesesShouldBeRemovedAnalyzer : EmptyParenthesesAnalyzerBase<AttributeSyntax>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH3107";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH3107UnnecessaryAttributeConstructorParenthesesShouldBeRemovedAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Clarity, nameof(AnalyzerResources.RH3107Title), nameof(AnalyzerResources.RH3107MessageFormat), SyntaxKind.Attribute)
    {
    }

    #endregion // Constructor

    #region EmptyParenthesesAnalyzerBase

    /// <inheritdoc/>
    protected override bool HasUnnecessaryParentheses(AttributeSyntax node)
    {
        return node.ArgumentList is { Arguments.Count: 0 };
    }

    /// <inheritdoc/>
    protected override Location GetDiagnosticLocation(AttributeSyntax node)
    {
        return node.ArgumentList?.GetLocation() ?? node.GetLocation();
    }

    #endregion // EmptyParenthesesAnalyzerBase
}