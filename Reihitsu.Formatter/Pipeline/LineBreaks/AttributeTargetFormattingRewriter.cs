using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;
using Reihitsu.Core.Enumerations;

namespace Reihitsu.Formatter.Pipeline.LineBreaks;

/// <summary>
/// Applies line-break rules for target-based attribute lists
/// </summary>
internal sealed class AttributeTargetFormattingRewriter : CSharpSyntaxRewriter
{
    #region Fields

    /// <summary>
    /// The formatting context
    /// </summary>
    private readonly FormattingContext _context;

    /// <summary>
    /// The cancellation token
    /// </summary>
    private readonly CancellationToken _cancellationToken;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context">The formatting context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public AttributeTargetFormattingRewriter(FormattingContext context,
                                             CancellationToken cancellationToken)
    {
        _context = context;
        _cancellationToken = cancellationToken;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether the owner is an accessor whose attribute lists must stay on the property's single line
    /// </summary>
    /// <param name="owner">Owner node</param>
    /// <returns><see langword="true"/> when the accessor belongs to a single-line property; otherwise, <see langword="false"/></returns>
    private static bool ShouldKeepAccessorListsSingleLine(SyntaxNode owner)
    {
        return owner is AccessorDeclarationSyntax accessorDeclaration
               && accessorDeclaration.Parent?.Parent is BasePropertyDeclarationSyntax basePropertyDeclaration
               && SyntaxNodeUtilities.IsSingleLine(basePropertyDeclaration);
    }

    /// <summary>
    /// Resolves the placement mode for an attribute list, honoring the single-line accessor context
    /// </summary>
    /// <param name="attributeList">Attribute list</param>
    /// <param name="keepAccessorListsSingleLine">Whether the owner's attribute lists must stay single-line</param>
    /// <returns>Effective placement mode</returns>
    private static TargetAttributePlacementMode ResolvePlacementMode(AttributeListSyntax attributeList,
                                                                     bool keepAccessorListsSingleLine)
    {
        return keepAccessorListsSingleLine
                   ? TargetAttributePlacementMode.SingleLine
                   : AttributeTargetFormattingShared.ResolvePlacementMode(attributeList);
    }

    /// <summary>
    /// Resolves the list-shape mode for an attribute list, honoring the single-line accessor context
    /// </summary>
    /// <param name="attributeList">Attribute list</param>
    /// <param name="keepAccessorListsSingleLine">Whether the owner's attribute lists must stay single-line</param>
    /// <returns>Effective list-shape mode</returns>
    private static TargetAttributeListShapeMode ResolveListShapeMode(AttributeListSyntax attributeList,
                                                                     bool keepAccessorListsSingleLine)
    {
        return keepAccessorListsSingleLine
                   ? TargetAttributeListShapeMode.MergedList
                   : AttributeTargetFormattingShared.ResolveListShapeMode(attributeList);
    }

    /// <summary>
    /// Merges attribute lists that use the merged-list policy
    /// </summary>
    /// <param name="owner">Owner node</param>
    /// <param name="keepAccessorListsSingleLine">Whether the owner's attribute lists must stay single-line</param>
    /// <returns>Updated owner node</returns>
    private static SyntaxNode MergeAttributeLists(SyntaxNode owner,
                                                  bool keepAccessorListsSingleLine)
    {
        while (true)
        {
            var lists = AttributeTargetUtilities.GetAttributeLists(owner);
            var matchingLists = lists.Where(list => AttributeTargetUtilities.TryResolveTarget(list, out var _)
                                                    && ResolveListShapeMode(list, keepAccessorListsSingleLine) == TargetAttributeListShapeMode.MergedList
                                                    && SyntaxNodeUtilities.HasCommentsOrDirectives(list) == false)
                                     .ToArray();

            if (matchingLists.Length <= 1)
            {
                return owner;
            }

            var mergedAttributes = new List<AttributeSyntax>();
            var firstList = matchingLists[0];

            foreach (var attributeList in matchingLists)
            {
                foreach (var attribute in attributeList.Attributes)
                {
                    mergedAttributes.Add(attribute.WithLeadingTrivia(SyntaxFactory.TriviaList())
                                                  .WithTrailingTrivia(SyntaxFactory.TriviaList()));
                }
            }

            var mergedList = firstList.WithAttributes(SyntaxFactory.SeparatedList(mergedAttributes))
                                      .WithTrailingTrivia(SyntaxFactory.Space);
            var updatedLists = new List<AttributeListSyntax>();

            foreach (var attributeList in lists)
            {
                if (ReferenceEquals(attributeList, firstList))
                {
                    updatedLists.Add(mergedList);
                }
                else if (matchingLists.Contains(attributeList) == false)
                {
                    updatedLists.Add(attributeList);
                }
            }

            owner = AttributeTargetUtilities.WithAttributeLists(owner, SyntaxFactory.List(updatedLists));
        }
    }

    /// <summary>
    /// Adopts a placement result, reporting a change only when the rewrite actually altered the owner.
    /// The line-break helpers return the original node unchanged when a rewrite is refused (for example,
    /// when a single-line join would move a token into a comment), so an unconditional "changed" would
    /// spin <see cref="ApplyPlacement"/> forever on that shape
    /// </summary>
    /// <param name="owner">Owner node, updated when the placement changed</param>
    /// <param name="updatedOwner">Candidate owner produced by the placement rewrite</param>
    /// <returns><see langword="true"/> when the owner was changed; otherwise, <see langword="false"/></returns>
    private static bool TryReportChange(ref SyntaxNode owner, SyntaxNode updatedOwner)
    {
        if (ReferenceEquals(updatedOwner, owner))
        {
            return false;
        }

        owner = updatedOwner;

        return true;
    }

    /// <summary>
    /// Formats the attribute lists attached to an owner node
    /// </summary>
    /// <param name="owner">Owner node</param>
    /// <param name="keepAccessorListsSingleLine">Whether the owner's attribute lists must stay single-line</param>
    /// <returns>Updated owner node</returns>
    private SyntaxNode FormatAttributeLists(SyntaxNode owner,
                                            bool keepAccessorListsSingleLine)
    {
        if (AttributeTargetUtilities.GetAttributeLists(owner).Count == 0)
        {
            return owner;
        }

        owner = SplitAttributeLists(owner, keepAccessorListsSingleLine);
        owner = MergeAttributeLists(owner, keepAccessorListsSingleLine);
        owner = ApplyPlacement(owner, keepAccessorListsSingleLine);

        return owner;
    }

    /// <summary>
    /// Applies the placement policy to all attribute lists on the owner node
    /// </summary>
    /// <param name="owner">Owner node</param>
    /// <param name="keepAccessorListsSingleLine">Whether the owner's attribute lists must stay single-line</param>
    /// <returns>Updated owner node</returns>
    private SyntaxNode ApplyPlacement(SyntaxNode owner,
                                      bool keepAccessorListsSingleLine)
    {
        while (true)
        {
            _cancellationToken.ThrowIfCancellationRequested();

            var changed = false;

            foreach (var attributeList in AttributeTargetUtilities.GetAttributeLists(owner))
            {
                if (TryApplyPlacementToList(ref owner, attributeList, keepAccessorListsSingleLine))
                {
                    changed = true;

                    break;
                }
            }

            if (changed == false)
            {
                return owner;
            }
        }
    }

    /// <summary>
    /// Applies the placement policy to a single attribute list, updating the owner in place
    /// </summary>
    /// <param name="owner">Owner node, updated when the placement changes</param>
    /// <param name="attributeList">Attribute list to place</param>
    /// <param name="keepAccessorListsSingleLine">Whether the owner's attribute lists must stay single-line</param>
    /// <returns><see langword="true"/> when the owner was changed; otherwise, <see langword="false"/></returns>
    private bool TryApplyPlacementToList(ref SyntaxNode owner, AttributeListSyntax attributeList, bool keepAccessorListsSingleLine)
    {
        if (SyntaxNodeUtilities.HasCommentsOrDirectives(attributeList)
            || AttributeTargetUtilities.TryGetTokenAfterAttributeList(attributeList, out var tokenAfter) == false)
        {
            return false;
        }

        var placementMode = ResolvePlacementMode(attributeList, keepAccessorListsSingleLine);
        var closeLine = attributeList.GetLocation().GetLineSpan().EndLinePosition.Line;
        var nextLine = tokenAfter.GetLocation().GetLineSpan().StartLinePosition.Line;
        var refreshedTokenAfter = TokenLocator.GetCurrentToken(owner, tokenAfter);

        if (placementMode == TargetAttributePlacementMode.SeparateLine
            && closeLine == nextLine)
        {
            var updatedOwner = LineBreakTriviaUtilities.MoveTokenToNewLine(owner, refreshedTokenAfter, _context.EndOfLine);

            return TryReportChange(ref owner, updatedOwner);
        }

        if (placementMode == TargetAttributePlacementMode.SingleLine
            && closeLine != nextLine)
        {
            var updatedOwner = LineBreakTriviaUtilities.CollapseTokenToSameLine(owner, refreshedTokenAfter);

            return TryReportChange(ref owner, updatedOwner);
        }

        return false;
    }

    /// <summary>
    /// Splits attribute lists that use the split-list policy
    /// </summary>
    /// <param name="owner">Owner node</param>
    /// <param name="keepAccessorListsSingleLine">Whether the owner's attribute lists must stay single-line</param>
    /// <returns>Updated owner node</returns>
    private SyntaxNode SplitAttributeLists(SyntaxNode owner,
                                           bool keepAccessorListsSingleLine)
    {
        while (true)
        {
            var lists = AttributeTargetUtilities.GetAttributeLists(owner);
            var listToSplit = lists.FirstOrDefault(list => AttributeTargetUtilities.TryResolveTarget(list, out var _)
                                                           && ResolveListShapeMode(list, keepAccessorListsSingleLine) == TargetAttributeListShapeMode.SplitLists
                                                           && list.Attributes.Count > 1
                                                           && SyntaxNodeUtilities.HasCommentsOrDirectives(list) == false);

            if (listToSplit == null)
            {
                return owner;
            }

            owner = AttributeTargetUtilities.WithAttributeLists(owner, SyntaxFactory.List(BuildSplitAttributeLists(lists, listToSplit)));
        }
    }

    /// <summary>
    /// Rebuilds the owner's attribute lists with the target list split into single-attribute lists
    /// </summary>
    /// <param name="lists">The owner's current attribute lists</param>
    /// <param name="listToSplit">The multi-attribute list to split</param>
    /// <returns>The rebuilt attribute lists</returns>
    private List<AttributeListSyntax> BuildSplitAttributeLists(IReadOnlyList<AttributeListSyntax> lists, AttributeListSyntax listToSplit)
    {
        var updatedLists = new List<AttributeListSyntax>(lists.Count - 1 + listToSplit.Attributes.Count);

        foreach (var attributeList in lists)
        {
            if (ReferenceEquals(attributeList, listToSplit) == false)
            {
                updatedLists.Add(attributeList);

                continue;
            }

            for (var index = 0; index < listToSplit.Attributes.Count; index++)
            {
                var attribute = listToSplit.Attributes[index];
                var newList = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(attribute))
                                           .WithTarget(listToSplit.Target);

                if (index == 0)
                {
                    newList = newList.WithLeadingTrivia(listToSplit.GetLeadingTrivia());
                }
                else
                {
                    newList = newList.WithLeadingTrivia(SyntaxFactory.EndOfLine(_context.EndOfLine));
                }

                if (index == listToSplit.Attributes.Count - 1)
                {
                    newList = newList.WithTrailingTrivia(listToSplit.GetTrailingTrivia());
                }

                updatedLists.Add(newList);
            }
        }

        return updatedLists;
    }

    #endregion // Methods

    #region CSharpSyntaxVisitor

    /// <inheritdoc/>
    public override SyntaxNode Visit(SyntaxNode node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        // Resolved from the original node while its parent chain is intact: the merge step rebuilds
        // the accessor and detaches it, which otherwise hides the single-line property context
        var keepAccessorListsSingleLine = ShouldKeepAccessorListsSingleLine(node);
        var updated = base.Visit(node);

        if (updated == null)
        {
            return null;
        }

        return FormatAttributeLists(updated, keepAccessorListsSingleLine);
    }

    #endregion // CSharpSyntaxVisitor
}