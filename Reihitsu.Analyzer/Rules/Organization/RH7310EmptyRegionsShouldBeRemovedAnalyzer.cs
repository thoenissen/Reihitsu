using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Organization;

/// <summary>
/// RH7310: Empty regions should be removed
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH7310EmptyRegionsShouldBeRemovedAnalyzer : DiagnosticAnalyzerBase<RH7310EmptyRegionsShouldBeRemovedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH7310";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH7310EmptyRegionsShouldBeRemovedAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Organization, nameof(AnalyzerResources.RH7310Title), nameof(AnalyzerResources.RH7310MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether the span between the region directives contains any code
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <param name="contentSpan">Span between the <c>#region</c> and <c>#endregion</c> directives</param>
    /// <returns><see langword="true"/> if the span contains at least one code token</returns>
    private static bool ContainsContent(SyntaxNode root, TextSpan contentSpan)
    {
        if (contentSpan.Length <= 0)
        {
            return false;
        }

        foreach (var token in root.DescendantTokens(contentSpan))
        {
            if (token.Span.Length > 0
                && contentSpan.Contains(token.Span))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether the span between the region directives contains a nested region directive
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <param name="contentSpan">Span between the <c>#region</c> and <c>#endregion</c> directives</param>
    /// <returns><see langword="true"/> if a nested <c>#region</c> directive is present</returns>
    private static bool ContainsNestedRegion(SyntaxNode root, TextSpan contentSpan)
    {
        foreach (var trivia in root.DescendantTrivia(contentSpan, descendIntoTrivia: true))
        {
            if (trivia.IsKind(SyntaxKind.RegionDirectiveTrivia)
                && contentSpan.Contains(trivia.SpanStart))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether the span between the region directives contains conditionally-excluded code or a
    /// non-region directive
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <param name="contentSpan">Span between the <c>#region</c> and <c>#endregion</c> directives</param>
    /// <returns><see langword="true"/> if disabled text or a non-region directive is present</returns>
    private static bool ContainsDisabledCodeOrDirective(SyntaxNode root, TextSpan contentSpan)
    {
        foreach (var trivia in root.DescendantTrivia(contentSpan, descendIntoTrivia: true))
        {
            if (contentSpan.Contains(trivia.SpanStart) == false)
            {
                continue;
            }

            if (trivia.IsKind(SyntaxKind.DisabledTextTrivia))
            {
                return true;
            }

            if (trivia.IsDirective
                && trivia.IsKind(SyntaxKind.RegionDirectiveTrivia) == false
                && trivia.IsKind(SyntaxKind.EndRegionDirectiveTrivia) == false)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Analyzes a <see cref="SyntaxKind.RegionDirectiveTrivia"/> occurrence
    /// </summary>
    /// <param name="context">Context</param>
    private void OnRegion(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not RegionDirectiveTriviaSyntax node)
        {
            return;
        }

        var regionTrivia = node.ParentTrivia;

        if (RegionDirectiveUtilities.IsWithinElementBody(regionTrivia))
        {
            return;
        }

        var root = context.Node.SyntaxTree.GetRoot(context.CancellationToken);

        if (RegionDirectiveUtilities.TryFindMatchingDirective(root, regionTrivia, out var endRegionTrivia) == false)
        {
            return;
        }

        var contentSpan = TextSpan.FromBounds(regionTrivia.Span.End, endRegionTrivia.Span.Start);

        if (ContainsContent(root, contentSpan)
            || ContainsNestedRegion(root, contentSpan)
            || ContainsDisabledCodeOrDirective(root, contentSpan))
        {
            return;
        }

        context.ReportDiagnostic(CreateDiagnostic(node.GetLocation()));
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnRegion, SyntaxKind.RegionDirectiveTrivia);
    }

    #endregion // DiagnosticAnalyzer
}