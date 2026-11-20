using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5419: Expression style get-only indexers should be single lined
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5419ExpressionStyleGetOnlyIndexersShouldBeSingleLinedAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5419";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5419ExpressionStyleGetOnlyIndexersShouldBeSingleLinedAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Layout)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Gets the line on which a token starts
    /// </summary>
    /// <param name="token">Token</param>
    /// <returns>The zero-based line number</returns>
    private static int GetStartLine(SyntaxToken token)
    {
        return token.GetLocation().GetLineSpan().StartLinePosition.Line;
    }

    /// <summary>
    /// Analyzing all <see cref="SyntaxKind.IndexerDeclaration"/> occurrences. An expression-bodied indexer is reported when
    /// the <c>[</c> and the <c>]</c> of its parameter list are on different lines, or when its <c>=&gt;</c> or the first
    /// token of its expression is not on the line of the <c>]</c> (RH5419-001). The line of <c>this</c> does not count, and
    /// the expression may continue on later lines
    /// </summary>
    /// <param name="context">Context</param>
    private void OnIndexerDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not IndexerDeclarationSyntax indexerDeclaration
            || indexerDeclaration.ExpressionBody is null)
        {
            return;
        }

        var closeBracketLine = GetStartLine(indexerDeclaration.ParameterList.CloseBracketToken);

        if (GetStartLine(indexerDeclaration.ParameterList.OpenBracketToken) != closeBracketLine
            || GetStartLine(indexerDeclaration.ExpressionBody.ArrowToken) != closeBracketLine
            || GetStartLine(indexerDeclaration.ExpressionBody.Expression.GetFirstToken()) != closeBracketLine)
        {
            context.ReportDiagnostic(CreateDiagnostic(indexerDeclaration.GetLocation()));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnIndexerDeclaration, SyntaxKind.IndexerDeclaration);
    }

    #endregion // DiagnosticAnalyzer
}