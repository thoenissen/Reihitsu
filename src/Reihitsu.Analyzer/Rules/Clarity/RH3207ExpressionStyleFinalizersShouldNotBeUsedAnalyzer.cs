using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;

namespace Reihitsu.Analyzer.Rules.Clarity;

/// <summary>
/// RH3207: Expression style finalizers should not be used
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH3207ExpressionStyleFinalizersShouldNotBeUsedAnalyzer : ExpressionBodyAnalyzerBase<DestructorDeclarationSyntax>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH3207";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH3207ExpressionStyleFinalizersShouldNotBeUsedAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH3207Title), nameof(AnalyzerResources.RH3207MessageFormat), SyntaxKind.DestructorDeclaration)
    {
    }

    #endregion // Constructor

    #region ExpressionBodyAnalyzerBase

    /// <inheritdoc/>
    protected override ArrowExpressionClauseSyntax GetExpressionBody(DestructorDeclarationSyntax node)
    {
        return node.ExpressionBody;
    }

    /// <inheritdoc/>
    protected override SyntaxToken GetSemicolonToken(DestructorDeclarationSyntax node)
    {
        return node.SemicolonToken;
    }

    #endregion // ExpressionBodyAnalyzerBase
}