using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;
using Reihitsu.Core.Enumerations;
using Reihitsu.Formatter.Pipeline.LineBreaks.Utilities;

namespace Reihitsu.Formatter.Pipeline.StructuralTransforms.Utilities;

/// <summary>
/// The one split and the one merge of the formatter's attribute lists. The structural transforms shape the lists of
/// declarations, parameters, and type parameters with them, and the line-break phase shapes the lists of accessors, whose
/// shape follows the layout that phase gives their accessor list
/// </summary>
internal static class AttributeListShapeUtilities
{
    #region Methods

    /// <summary>
    /// Splits every attribute list of an owner that holds several attributes, follows the one-attribute-per-list shape, names
    /// a known target, and has no comment or directive between its brackets, into one list per attribute
    /// </summary>
    /// <param name="owner">Owner node</param>
    /// <returns>Updated owner node</returns>
    internal static SyntaxNode SplitAttributeLists(SyntaxNode owner)
    {
        while (true)
        {
            var lists = AttributeTargetUtilities.GetAttributeLists(owner);
            var listToSplit = lists.FirstOrDefault(CanSplit);

            if (listToSplit == null)
            {
                return owner;
            }

            owner = AttributeTargetUtilities.WithAttributeLists(owner, SyntaxFactory.List(BuildSplitAttributeLists(lists, listToSplit)));
        }
    }

    /// <summary>
    /// Merges the attribute lists of an owner that the caller accepts as candidates and that resolve to the same target,
    /// one target at a time, as long as a group can be merged without touching a comment or directive. Lists of different
    /// targets are never merged, because the merged list keeps the first list's specifier and would move the other
    /// attributes to that target
    /// </summary>
    /// <param name="owner">Owner node</param>
    /// <param name="isCandidate">Whether a list that names a known target takes part in a merge</param>
    /// <param name="separators">The separators the merge writes</param>
    /// <returns>Updated owner node</returns>
    internal static SyntaxNode MergeAttributeLists(SyntaxNode owner, Func<AttributeListSyntax, bool> isCandidate, AttributeListMergeSeparators separators)
    {
        while (true)
        {
            var group = FindMergeableGroup(AttributeTargetUtilities.GetAttributeLists(owner), isCandidate);

            if (group.Length == 0)
            {
                return owner;
            }

            owner = AttributeTargetUtilities.MergeAttributeListGroup(owner, group, separators);
        }
    }

    /// <summary>
    /// Determines whether an attribute list follows the merged-list shape of a parameter or type parameter
    /// </summary>
    /// <param name="attributeList">Attribute list</param>
    /// <returns><see langword="true"/> when the list's lists of one target are merged; otherwise, <see langword="false"/></returns>
    internal static bool UsesMergedListShape(AttributeListSyntax attributeList)
    {
        return AttributeTargetFormattingShared.ResolveListShapeMode(attributeList) == TargetAttributeListShapeMode.MergedList;
    }

    /// <summary>
    /// Determines whether an attribute list is split
    /// </summary>
    /// <param name="attributeList">Attribute list</param>
    /// <returns><see langword="true"/> when the list is split; otherwise, <see langword="false"/></returns>
    private static bool CanSplit(AttributeListSyntax attributeList)
    {
        return attributeList.Attributes.Count > 1
               && AttributeTargetUtilities.TryResolveTarget(attributeList, out var _)
               && UsesMergedListShape(attributeList) == false
               && SyntaxNodeUtilities.InteriorContainsCommentOrDirective(attributeList) == false;
    }

    /// <summary>
    /// Finds the first group of candidate attribute lists that resolve to the same target and can be merged
    /// </summary>
    /// <param name="lists">The owner's attribute lists</param>
    /// <param name="isCandidate">Whether a list that names a known target takes part in a merge</param>
    /// <returns>The mergeable group, or an empty array when no group can be merged</returns>
    private static AttributeListSyntax[] FindMergeableGroup(IReadOnlyList<AttributeListSyntax> lists, Func<AttributeListSyntax, bool> isCandidate)
    {
        var candidates = lists.Where(list => AttributeTargetUtilities.TryResolveTarget(list, out var _)
                                             && isCandidate(list))
                              .ToArray();

        foreach (var candidate in candidates)
        {
            var group = candidates.Where(list => AttributeTargetUtilities.HaveSameTarget(candidate, list))
                                  .ToArray();

            if (AttributeTargetUtilities.CanMergeAttributeListGroup(group))
            {
                return group;
            }
        }

        return [];
    }

    /// <summary>
    /// Rebuilds the owner's attribute lists with one list split into single-attribute lists. The first created list keeps
    /// the original opening bracket with its trivia, the last one the original closing bracket with its trivia, and every
    /// list keeps the original specifier. Everything between two attributes — the comma, and the whitespace, line breaks,
    /// and blank lines in front of it and behind it — is dropped, and the brackets the split creates carry no trivia, so
    /// nothing stands between two created lists
    /// </summary>
    /// <param name="lists">The owner's current attribute lists</param>
    /// <param name="listToSplit">The multi-attribute list to split</param>
    /// <returns>The rebuilt attribute lists</returns>
    private static List<AttributeListSyntax> BuildSplitAttributeLists(IReadOnlyList<AttributeListSyntax> lists, AttributeListSyntax listToSplit)
    {
        var updatedLists = new List<AttributeListSyntax>(lists.Count - 1 + listToSplit.Attributes.Count);
        var lastIndex = listToSplit.Attributes.Count - 1;

        foreach (var attributeList in lists)
        {
            if (ReferenceEquals(attributeList, listToSplit) == false)
            {
                updatedLists.Add(attributeList);

                continue;
            }

            for (var index = 0; index <= lastIndex; index++)
            {
                var attribute = listToSplit.Attributes[index];

                if (index > 0)
                {
                    attribute = attribute.WithLeadingTrivia(SyntaxFactory.TriviaList());
                }

                if (index < lastIndex)
                {
                    attribute = attribute.WithTrailingTrivia(SyntaxFactory.TriviaList());
                }

                var openBracket = index == 0
                                      ? listToSplit.OpenBracketToken
                                      : SyntaxFactory.Token(SyntaxFactory.TriviaList(), SyntaxKind.OpenBracketToken, SyntaxFactory.TriviaList());
                var closeBracket = index == lastIndex
                                       ? listToSplit.CloseBracketToken
                                       : SyntaxFactory.Token(SyntaxFactory.TriviaList(), SyntaxKind.CloseBracketToken, SyntaxFactory.TriviaList());

                updatedLists.Add(SyntaxFactory.AttributeList(openBracket, listToSplit.Target, SyntaxFactory.SingletonSeparatedList(attribute), closeBracket));
            }
        }

        return updatedLists;
    }

    #endregion // Methods
}