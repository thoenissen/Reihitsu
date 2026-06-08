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

        var updated = base.Visit(node);

        if (updated == null)
        {
            return null;
        }

        return FormatAttributeLists(updated);
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