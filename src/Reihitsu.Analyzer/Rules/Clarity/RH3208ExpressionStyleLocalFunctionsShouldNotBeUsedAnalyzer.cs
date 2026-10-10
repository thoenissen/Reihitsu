using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;

namespace Reihitsu.Analyzer.Rules.Clarity;

/// <summary>
/// RH3208: Expression style local functions should not be used
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH3208ExpressionStyleLocalFunctionsShouldNotBeUsedAnalyzer : ExpressionBodyAnalyzerBase<LocalFunctionStatementSyntax>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH3208";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH3208ExpressionStyleLocalFunctionsShouldNotBeUsedAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH3208Title), nameof(AnalyzerResources.RH3208MessageFormat), SyntaxKind.LocalFunctionStatement)
    {
    }

    #endregion // Constructor

    #region ExpressionBodyAnalyzerBase

    /// <inheritdoc/>
    protected override ArrowExpressionClauseSyntax GetExpressionBody(LocalFunctionStatementSyntax node)
    {
        return node.ExpressionBody;
    }

    /// <inheritdoc/>
    protected override SyntaxToken GetSemicolonToken(LocalFunctionStatementSyntax node)
    {
        return node.SemicolonToken;
    }

    #endregion // ExpressionBodyAnalyzerBase
}