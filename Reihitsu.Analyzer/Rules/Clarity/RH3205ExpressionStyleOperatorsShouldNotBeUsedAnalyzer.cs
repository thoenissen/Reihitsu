using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;

namespace Reihitsu.Analyzer.Rules.Clarity;

/// <summary>
/// RH3205: Expression style operators should not be used
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH3205ExpressionStyleOperatorsShouldNotBeUsedAnalyzer : ExpressionBodyAnalyzerBase<OperatorDeclarationSyntax>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH3205";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH3205ExpressionStyleOperatorsShouldNotBeUsedAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH3205Title), nameof(AnalyzerResources.RH3205MessageFormat), SyntaxKind.OperatorDeclaration)
    {
    }

    #endregion // Constructor

    #region ExpressionBodyAnalyzerBase

    /// <inheritdoc/>
    protected override ArrowExpressionClauseSyntax GetExpressionBody(OperatorDeclarationSyntax node)
    {
        return node.ExpressionBody;
    }

    /// <inheritdoc/>
    protected override SyntaxToken GetSemicolonToken(OperatorDeclarationSyntax node)
    {
        return node.SemicolonToken;
    }

    #endregion // ExpressionBodyAnalyzerBase
}