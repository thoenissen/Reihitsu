using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;

namespace Reihitsu.Analyzer.Rules.Clarity;

/// <summary>
/// RH3206: Expression style conversion operators should not be used
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH3206ExpressionStyleConversionOperatorsShouldNotBeUsedAnalyzer : ExpressionBodyAnalyzerBase<ConversionOperatorDeclarationSyntax>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH3206";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH3206ExpressionStyleConversionOperatorsShouldNotBeUsedAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH3206Title), nameof(AnalyzerResources.RH3206MessageFormat), SyntaxKind.ConversionOperatorDeclaration)
    {
    }

    #endregion // Constructor

    #region ExpressionBodyAnalyzerBase

    /// <inheritdoc/>
    protected override ArrowExpressionClauseSyntax GetExpressionBody(ConversionOperatorDeclarationSyntax node)
    {
        return node.ExpressionBody;
    }

    /// <inheritdoc/>
    protected override SyntaxToken GetSemicolonToken(ConversionOperatorDeclarationSyntax node)
    {
        return node.SemicolonToken;
    }

    #endregion // ExpressionBodyAnalyzerBase
}