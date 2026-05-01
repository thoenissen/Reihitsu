using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0392: Statement lambda opening braces should be aligned
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0392StatementLambdaOpeningBraceShouldBeAlignedAnalyzer : DiagnosticAnalyzerBase<RH0392StatementLambdaOpeningBraceShouldBeAlignedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0392";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0392StatementLambdaOpeningBraceShouldBeAlignedAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0392Title), nameof(AnalyzerResources.RH0392MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Gets the anchor token for a parenthesized lambda expression
    /// </summary>
    /// <param name="lambda">Lambda expression</param>
    /// <returns>Anchor token</returns>
    private static SyntaxToken GetAnchorToken(ParenthesizedLambdaExpressionSyntax lambda)
    {
        if (lambda.AsyncKeyword.IsKind(SyntaxKind.AsyncKeyword))
        {
            return lambda.AsyncKeyword;
        }

        return lambda.ParameterList.OpenParenToken;
    }

    /// <summary>
    /// Gets the anchor token for a simple lambda expression
    /// </summary>
    /// <param name="lambda">Lambda expression</param>
    /// <returns>Anchor token</returns>
    private static SyntaxToken GetAnchorToken(SimpleLambdaExpressionSyntax lambda)
    {
        if (lambda.AsyncKeyword.IsKind(SyntaxKind.AsyncKeyword))
        {
            return lambda.AsyncKeyword;
        }

        return lambda.Parameter.Identifier;
    }

    /// <summary>
    /// Analyzes a statement lambda block
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="block">Block body</param>
    /// <param name="anchorToken">Anchor token</param>
    private void AnalyzeBlock(SyntaxNodeAnalysisContext context, BlockSyntax block, SyntaxToken anchorToken)
    {
        var openBraceToken = block.OpenBraceToken;
        var previousToken = openBraceToken.GetPreviousToken();

        if (previousToken.RawKind == 0)
        {
            return;
        }

        var openBraceLineSpan = openBraceToken.GetLocation().GetLineSpan();
        var previousLine = previousToken.GetLocation().GetLineSpan().EndLinePosition.Line;

        if (openBraceLineSpan.StartLinePosition.Line <= previousLine)
        {
            return;
        }

        var expectedColumn = anchorToken.GetLocation().GetLineSpan().StartLinePosition.Character;

        if (openBraceLineSpan.StartLinePosition.Character != expectedColumn)
        {
            context.ReportDiagnostic(CreateDiagnostic(openBraceToken.GetLocation()));
        }
    }

    /// <summary>
    /// Analyzing parenthesized lambda expressions
    /// </summary>
    /// <param name="context">Context</param>
    private void OnParenthesizedLambdaExpression(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ParenthesizedLambdaExpressionSyntax lambda
            || lambda.Block == null)
        {
            return;
        }

        AnalyzeBlock(context, lambda.Block, GetAnchorToken(lambda));
    }

    /// <summary>
    /// Analyzing simple lambda expressions
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSimpleLambdaExpression(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not SimpleLambdaExpressionSyntax lambda
            || lambda.Block == null)
        {
            return;
        }

        AnalyzeBlock(context, lambda.Block, GetAnchorToken(lambda));
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnSimpleLambdaExpression, SyntaxKind.SimpleLambdaExpression);
        context.RegisterSyntaxNodeAction(OnParenthesizedLambdaExpression, SyntaxKind.ParenthesizedLambdaExpression);
    }

    #endregion // DiagnosticAnalyzer
}