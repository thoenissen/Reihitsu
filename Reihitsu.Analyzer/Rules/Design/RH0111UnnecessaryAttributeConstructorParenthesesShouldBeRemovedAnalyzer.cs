using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Design;

/// <summary>
/// RH0111: Unnecessary attribute constructor parentheses should be removed.
/// </summary>
[Microsoft.CodeAnalysis.Diagnostics.DiagnosticAnalyzer(Microsoft.CodeAnalysis.LanguageNames.CSharp)]
public class RH0111UnnecessaryAttributeConstructorParenthesesShouldBeRemovedAnalyzer : EmptyParenthesesAnalyzerBase<RH0111UnnecessaryAttributeConstructorParenthesesShouldBeRemovedAnalyzer, AttributeSyntax>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0111";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0111UnnecessaryAttributeConstructorParenthesesShouldBeRemovedAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Design, nameof(AnalyzerResources.RH0111Title), nameof(AnalyzerResources.RH0111MessageFormat), SyntaxKind.Attribute)
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