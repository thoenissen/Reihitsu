using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;
using Reihitsu.Core.Enumerations;
using Reihitsu.Formatter;

namespace Reihitsu.Analyzer.CodeFixes.Base;

/// <summary>
/// Base code-fix provider for target-based attribute list-shape rules
/// </summary>
public abstract class TargetAttributeListShapeCodeFixProviderBase : CodeFixProvider
{
    #region Properties

    /// <summary>
    /// Diagnostic ID handled by this provider
    /// </summary>
    protected abstract string DiagnosticId { get; }

    /// <summary>
    /// Target handled by this provider
    /// </summary>
    protected abstract AttributeTargets Target { get; }

    /// <summary>
    /// List-shape mode
    /// </summary>
    protected abstract TargetAttributeListShapeMode ListShapeMode { get; }

    /// <summary>
    /// Default placement mode for split output
    /// </summary>
    protected abstract TargetAttributePlacementMode DefaultPlacementMode { get; }

    /// <summary>
    /// Code-fix title
    /// </summary>
    protected abstract string CodeFixTitle { get; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Resolves placement mode for split output
    /// </summary>
    /// <param name="attributeList">Attribute list</param>
    /// <returns>Placement mode</returns>
    protected virtual TargetAttributePlacementMode ResolvePlacementMode(AttributeListSyntax attributeList)
    {
        return DefaultPlacementMode;
    }

    /// <summary>
    /// Resolves the effective list-shape mode for an attribute list
    /// </summary>
    /// <param name="attributeList">Attribute list</param>
    /// <returns>Effective list-shape mode</returns>
    protected virtual TargetAttributeListShapeMode ResolveListShapeMode(AttributeListSyntax attributeList)
    {
        return ListShapeMode;
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
    /// Applies split-list fix for a single attribute list
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="attributeList">Attribute list</param>
    /// <param name="placementMode">Placement mode</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated document</returns>
    private static async Task<Document> ApplySplitCodeFixAsync(Document document, AttributeListSyntax attributeList, TargetAttributePlacementMode placementMode, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var owner = attributeList.Parent;

        if (root == null || owner == null || SyntaxNodeUtilities.HasCommentsOrDirectives(attributeList))
        {
            return document;
        }

        var lists = AttributeTargetUtilities.GetAttributeLists(owner);
        var listIndex = IndexOfList(lists, attributeList);

        if (listIndex < 0 || attributeList.Attributes.Count <= 1)
        {
            return document;
        }

        var endOfLine = ReihitsuFormatterHelpers.DetectEndOfLine(root);
        var indentationTrivia = SyntaxTriviaUtilities.GetLineIndentationTrivia(attributeList.GetLeadingTrivia());
        var replacementLists = BuildSplitReplacementLists(attributeList, placementMode, endOfLine, indentationTrivia);

        var updatedLists = new List<AttributeListSyntax>(lists.Count - 1 + replacementLists.Count);

        for (var index = 0; index < lists.Count; index++)
        {
            if (index == listIndex)
            {
                updatedLists.AddRange(replacementLists);
            }
            else
            {
                updatedLists.Add(lists[index]);
            }
        }

        var updatedOwner = AttributeTargetUtilities.WithAttributeLists(owner, SyntaxFactory.List(updatedLists));
        var updatedRoot = root.ReplaceNode(owner, updatedOwner);

        return document.WithSyntaxRoot(updatedRoot);
    }

    /// <summary>
    /// Gets the index of the attribute list within the owner's attribute lists
    /// </summary>
    /// <param name="lists">Owner attribute lists</param>
    /// <param name="attributeList">Attribute list to locate</param>
    /// <returns>The index of the attribute list, or <c>-1</c> when it is not present</returns>
    private static int IndexOfList(IReadOnlyList<AttributeListSyntax> lists, AttributeListSyntax attributeList)
    {
        for (var index = 0; index < lists.Count; index++)
        {
            if (ReferenceEquals(lists[index], attributeList))
            {
                return index;
            }
        }

        return -1;
    }

    /// <summary>
    /// Builds the single-attribute lists that replace a multi-attribute list when splitting
    /// </summary>
    /// <param name="attributeList">Attribute list to split</param>
    /// <param name="placementMode">Placement mode</param>
    /// <param name="endOfLine">End-of-line sequence to insert between separate lines</param>
    /// <param name="indentationTrivia">Indentation applied to attributes moved onto their own line</param>
    /// <returns>The replacement attribute lists</returns>
    private static List<AttributeListSyntax> BuildSplitReplacementLists(AttributeListSyntax attributeList, TargetAttributePlacementMode placementMode, string endOfLine, SyntaxTriviaList indentationTrivia)
    {
        var replacementLists = new List<AttributeListSyntax>();

        for (var index = 0; index < attributeList.Attributes.Count; index++)
        {
            var attribute = attributeList.Attributes[index];
            var newList = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(attribute))
                                       .WithTarget(attributeList.Target);

            if (index == 0)
            {
                newList = newList.WithLeadingTrivia(attributeList.GetLeadingTrivia());
            }
            else if (placementMode == TargetAttributePlacementMode.SeparateLine)
            {
                newList = newList.WithLeadingTrivia(SyntaxFactory.TriviaList(SyntaxFactory.EndOfLine(endOfLine)).AddRange(indentationTrivia));
            }

            if (index == attributeList.Attributes.Count - 1)
            {
                newList = newList.WithTrailingTrivia(attributeList.GetTrailingTrivia());
            }

            replacementLists.Add(newList);
        }

        return replacementLists;
    }

    /// <summary>
    /// Applies merged-list fix for all matching sibling lists
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="attributeList">Attribute list from diagnostic span</param>
    /// <param name="listShapeMode">List-shape mode to merge</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated document</returns>
    private async Task<Document> ApplyMergeCodeFixAsync(Document document, AttributeListSyntax attributeList, TargetAttributeListShapeMode listShapeMode, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var owner = attributeList.Parent;

        if (root == null || owner == null)
        {
            return document;
        }

        var lists = AttributeTargetUtilities.GetAttributeLists(owner);
        var matchingLists = lists.Where(list => AttributeTargetUtilities.TryResolveTarget(list, out var target)
                                                && IsAttributeListInScope(list, target)
                                                && ResolveListShapeMode(list) == listShapeMode)
                                 .ToArray();

        if (matchingLists.Length <= 1 || Array.Exists(matchingLists, SyntaxNodeUtilities.HasCommentsOrDirectives))
        {
            return document;
        }

        var mergedAttributes = new List<AttributeSyntax>();
        var firstList = matchingLists[0];

        foreach (var list in matchingLists)
        {
            foreach (var attribute in list.Attributes)
            {
                mergedAttributes.Add(attribute.WithLeadingTrivia(SyntaxFactory.TriviaList())
                                              .WithTrailingTrivia(SyntaxFactory.TriviaList()));
            }
        }

        var mergedList = firstList.WithAttributes(SyntaxFactory.SeparatedList(mergedAttributes))
                                  .WithTrailingTrivia(SyntaxFactory.Space);

        var updatedLists = new List<AttributeListSyntax>();

        foreach (var list in lists)
        {
            if (ReferenceEquals(list, firstList))
            {
                updatedLists.Add(mergedList);
            }
            else if (matchingLists.Contains(list) == false)
            {
                updatedLists.Add(list);
            }
        }

        var updatedOwner = AttributeTargetUtilities.WithAttributeLists(owner, SyntaxFactory.List(updatedLists));
        var updatedRoot = root.ReplaceNode(owner, updatedOwner);

        return document.WithSyntaxRoot(updatedRoot);
    }

    /// <summary>
    /// Tries to get a fixable attribute list from a diagnostic
    /// </summary>
    /// <param name="root">Root</param>
    /// <param name="diagnostic">Diagnostic</param>
    /// <param name="attributeList">Resolved attribute list</param>
    /// <param name="listShapeMode">Resolved list-shape mode</param>
    /// <returns><see langword="true"/> when fix can be offered</returns>
    private bool TryGetFixableAttributeList(SyntaxNode root, Diagnostic diagnostic, out AttributeListSyntax attributeList, out TargetAttributeListShapeMode listShapeMode)
    {
        var diagnosticNode = root.FindToken(diagnostic.Location.SourceSpan.Start).Parent;
        listShapeMode = default;

        attributeList = diagnosticNode?.AncestorsAndSelf()
                                      .OfType<AttributeListSyntax>()
                                      .FirstOrDefault();

        if (attributeList == null
            || AttributeTargetUtilities.TryResolveTarget(attributeList, out var target) == false
            || IsAttributeListInScope(attributeList, target) == false)
        {
            return false;
        }

        listShapeMode = ResolveListShapeMode(attributeList);

        if (listShapeMode == TargetAttributeListShapeMode.SplitLists)
        {
            return attributeList.Attributes.Count > 1 && SyntaxNodeUtilities.HasCommentsOrDirectives(attributeList) == false;
        }

        var owner = attributeList.Parent;

        if (owner == null)
        {
            return false;
        }

        var expectedListShapeMode = listShapeMode;
        var matchingLists = AttributeTargetUtilities.GetAttributeLists(owner)
                                                    .Where(list => AttributeTargetUtilities.TryResolveTarget(list, out var siblingTarget)
                                                                   && IsAttributeListInScope(list, siblingTarget)
                                                                   && ResolveListShapeMode(list) == expectedListShapeMode)
                                                    .ToArray();

        return matchingLists.Length > 1
               && Array.Exists(matchingLists, SyntaxNodeUtilities.HasCommentsOrDirectives) == false;
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
            if (TryGetFixableAttributeList(root, diagnostic, out var attributeList, out var listShapeMode))
            {
                context.RegisterCodeFix(CodeAction.Create(CodeFixTitle,
                                                          token => listShapeMode == TargetAttributeListShapeMode.SplitLists
                                                                       ? ApplySplitCodeFixAsync(context.Document, attributeList, ResolvePlacementMode(attributeList), token)
                                                                       : ApplyMergeCodeFixAsync(context.Document, attributeList, listShapeMode, token),
                                                          GetType().Name),
                                        diagnostic);
            }
        }
    }

    #endregion // CodeFixProvider
}