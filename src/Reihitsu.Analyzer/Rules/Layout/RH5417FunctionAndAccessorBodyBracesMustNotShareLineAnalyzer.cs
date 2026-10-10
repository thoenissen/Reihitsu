using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5417: Function and accessor body braces must not share a line
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5417FunctionAndAccessorBodyBracesMustNotShareLineAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5417";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5417FunctionAndAccessorBodyBracesMustNotShareLineAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Layout, nameof(AnalyzerResources.RH5417Title), nameof(AnalyzerResources.RH5417MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether the block is a directly owned function or accessor body covered by RH5417
    /// </summary>
    /// <param name="block">Block to inspect</param>
    /// <returns><see langword="true"/> when the direct parent is covered; otherwise, <see langword="false"/></returns>
    private static bool IsCoveredBody(BlockSyntax block)
    {
        return block.Parent is MethodDeclarationSyntax
                            or ConstructorDeclarationSyntax
                            or DestructorDeclarationSyntax
                            or OperatorDeclarationSyntax
                            or ConversionOperatorDeclarationSyntax
                            or AccessorDeclarationSyntax
                            or LocalFunctionStatementSyntax
                            or SimpleLambdaExpressionSyntax
                            or ParenthesizedLambdaExpressionSyntax
                            or AnonymousMethodExpressionSyntax;
    }

    /// <summary>
    /// Analyzes a block for a covered single-line function or accessor body
    /// </summary>
    /// <param name="context">Context</param>
    private void OnBlock(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not BlockSyntax block)
        {
            return;
        }

        if (IsCoveredBody(block) == false
            || block.OpenBraceToken.IsMissing
            || block.CloseBraceToken.IsMissing)
        {
            return;
        }

        var openBraceLine = block.OpenBraceToken.GetLocation().GetLineSpan().StartLinePosition.Line;
        var closeBraceLine = block.CloseBraceToken.GetLocation().GetLineSpan().StartLinePosition.Line;

        if (openBraceLine == closeBraceLine)
        {
            context.ReportDiagnostic(CreateDiagnostic(block.OpenBraceToken.GetLocation()));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnBlock, SyntaxKind.Block);
    }

    #endregion // DiagnosticAnalyzer
}