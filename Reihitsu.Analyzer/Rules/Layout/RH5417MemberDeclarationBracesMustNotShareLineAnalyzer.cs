using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5417: Member declaration braces must not share a line
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5417MemberDeclarationBracesMustNotShareLineAnalyzer : DiagnosticAnalyzerBase<RH5417MemberDeclarationBracesMustNotShareLineAnalyzer>
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
    public RH5417MemberDeclarationBracesMustNotShareLineAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Layout, nameof(AnalyzerResources.RH5417Title), nameof(AnalyzerResources.RH5417MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether the given block is the body of a member declaration or accessor whose braces are covered by this rule
    /// </summary>
    /// <param name="block">The block to inspect</param>
    /// <returns><see langword="true"/> if the block is a covered member body; otherwise, <see langword="false"/></returns>
    private static bool IsCoveredMemberBody(BlockSyntax block)
    {
        return block.Parent is MethodDeclarationSyntax
                            or ConstructorDeclarationSyntax
                            or DestructorDeclarationSyntax
                            or OperatorDeclarationSyntax
                            or ConversionOperatorDeclarationSyntax
                            or AccessorDeclarationSyntax;
    }

    /// <summary>
    /// Analyzes the syntax tree
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        var root = context.Tree.GetRoot(context.CancellationToken);

        foreach (var block in root.DescendantNodes().OfType<BlockSyntax>())
        {
            if (IsCoveredMemberBody(block) == false)
            {
                continue;
            }

            if (block.OpenBraceToken.IsMissing
                || block.CloseBraceToken.IsMissing)
            {
                continue;
            }

            var openBraceLine = block.OpenBraceToken.GetLocation().GetLineSpan().StartLinePosition.Line;
            var closeBraceLine = block.CloseBraceToken.GetLocation().GetLineSpan().StartLinePosition.Line;

            if (openBraceLine == closeBraceLine)
            {
                context.ReportDiagnostic(CreateDiagnostic(block.OpenBraceToken.GetLocation()));
            }
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxTreeAction(OnSyntaxTree);
    }

    #endregion // DiagnosticAnalyzer
}