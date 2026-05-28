using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;

namespace Reihitsu.Analyzer.CodeFixes.Base;

/// <summary>
/// Base code-fix provider for target-based attribute placement rules
/// </summary>
public abstract class TargetAttributePlacementCodeFixProviderBase : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Diagnostic ID handled by this provider
    /// </summary>
    protected abstract string DiagnosticId { get; }

    /// <summary>
    /// Target handled by this provider
    /// </summary>
    protected abstract AttributeTargets Target { get; }

    /// <summary>
    /// Default placement mode
    /// </summary>
    protected abstract TargetAttributePlacementMode DefaultPlacementMode { get; }

    /// <summary>
    /// Code-fix title
    /// </summary>
    protected abstract string CodeFixTitle { get; }

    /// <summary>
    /// Resolves the placement mode for a specific attribute list
    /// </summary>
    /// <param name="attributeList">Attribute list</param>
    /// <returns>Placement mode</returns>
    protected virtual TargetAttributePlacementMode ResolvePlacementMode(AttributeListSyntax attributeList)
    {
        return DefaultPlacementMode;
    }

    /// <summary>
    /// Determines whether the attribute list is in scope for this code-fix provider
    /// </summary>
    /// <param name="attributeList">Attribute list</param>
    /// <param name="target">Resolved target</param>
    /// <returns><see langword="true"/> when the attribute list should be processed</returns>
    protected virtual bool IsAttributeListInScope(AttributeListSyntax attributeList, AttributeTargets target)
    {
        return target == Target;
    }

    /// <summary>
    /// Applies the placement fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="attributeList">Attribute list</param>
    /// <param name="placementMode">Placement mode</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated document</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, AttributeListSyntax attributeList, TargetAttributePlacementMode placementMode, CancellationToken cancellationToken)
    {
        if (AttributeTargetUtilities.TryGetTokenAfterAttributeList(attributeList, out var tokenAfter) == false)
        {
            return document;
        }

        var closeBracket = attributeList.CloseBracketToken;
        var updatedCloseBracket = closeBracket;

        if (placementMode == TargetAttributePlacementMode.SeparateLine)
        {
            var trailingTrivia = SyntaxFactory.TriviaList(SyntaxFactory.EndOfLine(Environment.NewLine));
            var indentationTrivia = SyntaxTriviaUtilities.GetLineIndentationTrivia(attributeList.GetLeadingTrivia());

            if (attributeList.Parent is CompilationUnitSyntax)
            {
                trailingTrivia = trailingTrivia.Add(SyntaxFactory.EndOfLine(Environment.NewLine));
            }

            trailingTrivia = trailingTrivia.AddRange(indentationTrivia);
            updatedCloseBracket = closeBracket.WithTrailingTrivia(trailingTrivia);
        }
        else
        {
            updatedCloseBracket = closeBracket.WithTrailingTrivia(SyntaxFactory.Space);
        }

        var updatedTokenAfter = tokenAfter.WithLeadingTrivia(SyntaxFactory.TriviaList());
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        var updatedRoot = root.ReplaceTokens([closeBracket, tokenAfter], (original, _) => original == closeBracket ? updatedCloseBracket : updatedTokenAfter);

        return document.WithSyntaxRoot(updatedRoot);
    }

    /// <summary>
    /// Tries to get a fixable attribute list from a diagnostic
    /// </summary>
    /// <param name="root">Root</param>
    /// <param name="diagnostic">Diagnostic</param>
    /// <param name="attributeList">Resolved attribute list</param>
    /// <param name="placementMode">Resolved placement mode</param>
    /// <returns><see langword="true"/> when fix can be offered</returns>
    private bool TryGetFixableAttributeList(SyntaxNode root, Diagnostic diagnostic, out AttributeListSyntax attributeList, out TargetAttributePlacementMode placementMode)
    {
        var diagnosticNode = root.FindToken(diagnostic.Location.SourceSpan.Start).Parent;

        placementMode = default;
        attributeList = diagnosticNode?.AncestorsAndSelf()
                                      .OfType<AttributeListSyntax>()
                                      .FirstOrDefault();

        if (attributeList == null
            || AttributeTargetUtilities.TryResolveTarget(attributeList, out var target) == false
            || IsAttributeListInScope(attributeList, target) == false
            || AttributeTargetUtilities.TryGetTokenAfterAttributeList(attributeList, out _) == false
            || SyntaxNodeUtilities.HasCommentsOrDirectives(attributeList))
        {
            return false;
        }

        placementMode = ResolvePlacementMode(attributeList);

        return true;
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [DiagnosticId];

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    /// <inheritdoc/>
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return;
        }

        foreach (var diagnostic in context.Diagnostics)
        {
            if (TryGetFixableAttributeList(root, diagnostic, out var attributeList, out var placementMode))
            {
                context.RegisterCodeFix(CodeAction.Create(CodeFixTitle,
                                                          token => ApplyCodeFixAsync(context.Document, attributeList, placementMode, token),
                                                          GetType().Name),
                                        diagnostic);
            }
        }
    }

    #endregion // CodeFixProvider
}