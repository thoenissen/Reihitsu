using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;

namespace Reihitsu.Formatter.Pipeline.LineBreaks;

/// <summary>
/// Applies line-break rules for target-based attribute lists
/// </summary>
internal sealed class AttributeTargetFormattingRewriter : LineBreakRewriter
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context">The formatting context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public AttributeTargetFormattingRewriter(FormattingContext context,
                                             CancellationToken cancellationToken)
        : base(context,
               cancellationToken)
    {
    }

    #endregion // Constructor

    #region CSharpSyntaxVisitor

    /// <inheritdoc/>
    public override SyntaxNode Visit(SyntaxNode node)
    {
        CancellationToken.ThrowIfCancellationRequested();

        var preserveSingleLineAccessorLayout = node is PropertyDeclarationSyntax propertyDeclaration
                                               && ShouldPreserveSingleLineAccessorLayout(propertyDeclaration);
        var updated = base.Visit(node);

        if (updated == null)
        {
            return null;
        }

        updated = FormatAttributeLists(updated);

        if (preserveSingleLineAccessorLayout && updated is PropertyDeclarationSyntax updatedPropertyDeclaration)
        {
            updated = NormalizeSingleLineAccessorProperties(updatedPropertyDeclaration);
        }

        return updated;
    }

    #endregion // CSharpSyntaxVisitor

    #region Methods

    /// <summary>
    /// Formats the attribute lists attached to an owner node
    /// </summary>
    /// <param name="owner">Owner node</param>
    /// <returns>Updated owner node</returns>
    private SyntaxNode FormatAttributeLists(SyntaxNode owner)
    {
        if (AttributeTargetUtilities.GetAttributeLists(owner).Count == 0)
        {
            return owner;
        }

        owner = SplitAttributeLists(owner);
        owner = MergeAttributeLists(owner);
        owner = ApplyPlacement(owner);

        return owner;
    }

    /// <summary>
    /// Determines whether the property accessor layout should remain single-line after formatting
    /// </summary>
    /// <param name="propertyDeclaration">Property declaration</param>
    /// <returns><see langword="true"/> if single-line accessor layout should be preserved; otherwise, <see langword="false"/></returns>
    private bool ShouldPreserveSingleLineAccessorLayout(PropertyDeclarationSyntax propertyDeclaration)
    {
        if (propertyDeclaration.AccessorList == null
            || SyntaxNodeUtilities.IsSingleLine(propertyDeclaration) == false
            || IsAutoPropertyAccessorList(propertyDeclaration.AccessorList) == false)
        {
            return false;
        }

        return propertyDeclaration.AccessorList.Accessors.Any(accessor => accessor.AttributeLists.Count > 0);
    }

    /// <summary>
    /// Keeps single-line auto-properties on one line when accessor attributes are reformatted
    /// </summary>
    /// <param name="owner">Owner node</param>
    /// <returns>Updated owner node</returns>
    private SyntaxNode NormalizeSingleLineAccessorProperties(SyntaxNode owner)
    {
        if (owner is not PropertyDeclarationSyntax propertyDeclaration
            || propertyDeclaration.AccessorList == null
            || IsAutoPropertyAccessorList(propertyDeclaration.AccessorList) == false)
        {
            return owner;
        }

        var updated = propertyDeclaration;
        updated = CollapseTokenToSameLine(updated, updated.AccessorList.OpenBraceToken);

        for (var accessorIndex = 0; accessorIndex < updated.AccessorList.Accessors.Count; accessorIndex++)
        {
            var accessor = updated.AccessorList.Accessors[accessorIndex];

            for (var attributeListIndex = 0; attributeListIndex < accessor.AttributeLists.Count; attributeListIndex++)
            {
                updated = CollapseTokenToSameLine(updated, accessor.AttributeLists[attributeListIndex].OpenBracketToken);
                accessor = updated.AccessorList.Accessors[accessorIndex];
            }

            updated = CollapseTokenToSameLine(updated, accessor.Keyword);
            accessor = updated.AccessorList.Accessors[accessorIndex];

            if (accessor.SemicolonToken.IsMissing == false)
            {
                updated = CollapseTokenToSameLine(updated, accessor.SemicolonToken);
            }
        }

        updated = CollapseTokenToSameLine(updated, updated.AccessorList.CloseBraceToken);

        return updated;
    }

    /// <summary>
    /// Applies the placement policy to all attribute lists on the owner node
    /// </summary>
    /// <param name="owner">Owner node</param>
    /// <returns>Updated owner node</returns>
    private SyntaxNode ApplyPlacement(SyntaxNode owner)
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

                var placementMode = AttributeTargetFormattingShared.ResolvePlacementMode(attributeList);
                var closeLine = attributeList.GetLocation().GetLineSpan().EndLinePosition.Line;
                var nextLine = tokenAfter.GetLocation().GetLineSpan().StartLinePosition.Line;
                var refreshedTokenAfter = GetCurrentToken(owner, tokenAfter);

                if (placementMode == TargetAttributePlacementMode.SeparateLine
                    && closeLine == nextLine)
                {
                    owner = MoveTokenToNewLine(owner, refreshedTokenAfter);
                    changed = true;

                    break;
                }

                if (placementMode == TargetAttributePlacementMode.SingleLine
                    && closeLine != nextLine)
                {
                    owner = CollapseTokenToSameLine(owner, refreshedTokenAfter);
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
    /// <returns>Updated owner node</returns>
    private SyntaxNode MergeAttributeLists(SyntaxNode owner)
    {
        while (true)
        {
            var lists = AttributeTargetUtilities.GetAttributeLists(owner);
            var matchingLists = lists.Where(list => AttributeTargetUtilities.TryResolveTarget(list, out var _)
                                                    && AttributeTargetFormattingShared.ResolveListShapeMode(list) == TargetAttributeListShapeMode.MergedList
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
    /// <returns>Updated owner node</returns>
    private SyntaxNode SplitAttributeLists(SyntaxNode owner)
    {
        while (true)
        {
            var lists = AttributeTargetUtilities.GetAttributeLists(owner);
            var listToSplit = lists.FirstOrDefault(list => AttributeTargetUtilities.TryResolveTarget(list, out var _)
                                                           && AttributeTargetFormattingShared.ResolveListShapeMode(list) == TargetAttributeListShapeMode.SplitLists
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
                        newList = newList.WithLeadingTrivia(SyntaxFactory.EndOfLine(Context.EndOfLine));
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