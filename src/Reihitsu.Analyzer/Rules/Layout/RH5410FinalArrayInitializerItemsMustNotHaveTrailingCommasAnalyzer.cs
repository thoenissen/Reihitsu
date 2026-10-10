using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5410: Final array initializer items must not have trailing commas
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5410FinalArrayInitializerItemsMustNotHaveTrailingCommasAnalyzer : TrailingCommaAnalyzerBase<InitializerExpressionSyntax>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5410";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5410FinalArrayInitializerItemsMustNotHaveTrailingCommasAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Layout, nameof(AnalyzerResources.RH5410Title), nameof(AnalyzerResources.RH5410MessageFormat), SyntaxKind.ArrayInitializerExpression)
    {
    }

    #endregion // Constructor

    #region TrailingCommaAnalyzerBase

    /// <inheritdoc/>
    protected override SyntaxNodeOrTokenList GetElementsWithSeparators(InitializerExpressionSyntax node)
    {
        return node.Expressions.GetWithSeparators();
    }

    /// <inheritdoc/>
    protected override SyntaxToken GetCloseBraceToken(InitializerExpressionSyntax node)
    {
        return node.CloseBraceToken;
    }

    #endregion // TrailingCommaAnalyzerBase
}