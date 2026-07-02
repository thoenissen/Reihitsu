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
            var lists = AttributeTargetUtilities.GetAttributeLists(owner);
            var changed = false;

            foreach (var attributeList in lists)
            {
                if (SyntaxNodeUtilities.HasCommentsOrDirectives(attributeList))
                {
                    continue;
                }

                if (AttributeTargetUtilities.TryGetTokenAfterAttributeList(attributeList, out var tokenAfter) == false)
                {
                    continue;
                }

                var placementMode = ResolvePlacementMode(attributeList, keepAccessorListsSingleLine);
                var closeLine = attributeList.GetLocation().GetLineSpan().EndLinePosition.Line;
                var nextLine = tokenAfter.GetLocation().GetLineSpan().StartLinePosition.Line;
                var refreshedTokenAfter = TokenLocator.GetCurrentToken(owner, tokenAfter);

                if (placementMode == TargetAttributePlacementMode.SeparateLine
                    && closeLine == nextLine)
                {
                    owner = LineBreakTriviaUtilities.MoveTokenToNewLine(owner, refreshedTokenAfter, _context.EndOfLine);
                    changed = true;

                    break;
                }

                if (placementMode == TargetAttributePlacementMode.SingleLine
                    && closeLine != nextLine)
                {
                    owner = LineBreakTriviaUtilities.CollapseTokenToSameLine(owner, refreshedTokenAfter);
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
    /// Merges attribute lists that use the merged-list policy
    /// </summary>
    /// <param name="owner">Owner node</param>
    /// <param name="keepAccessorListsSingleLine">Whether the owner's attribute lists must stay single-line</param>
    /// <returns>Updated owner node</returns>
    private SyntaxNode MergeAttributeLists(SyntaxNode owner,
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

            owner = AttributeTargetUtilities.WithAttributeLists(owner, SyntaxFactory.List(updatedLists));
        }
    }

    #endregion // Methods
}