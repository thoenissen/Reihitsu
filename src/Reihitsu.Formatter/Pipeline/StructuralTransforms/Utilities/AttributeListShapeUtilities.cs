using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;
using Reihitsu.Core.Enumerations;
using Reihitsu.Formatter.Pipeline.LineBreaks.Utilities;

namespace Reihitsu.Formatter.Pipeline.StructuralTransforms.Utilities;

/// <summary>
/// The one split and the one merge of the formatter's attribute lists. Both take the same predicate, which says whether a
/// list has the merged-list shape: the split splits every list that does not have it, and the merge merges the lists of
/// one target that have it. The structural transforms shape the lists of every owner except an accessor, passing
/// <see cref="UsesMergedListShape"/>; the line-break phase shapes the lists of accessors, whose shape also follows the
/// layout that phase gives their accessor list
/// </summary>
internal static class AttributeListShapeUtilities
{
    #region Methods

    /// <summary>
    /// Splits every attribute list of an owner that holds several attributes, does not have the merged-list shape, names a
    /// known target, and has no comment or directive between its brackets, into one list per attribute
    /// </summary>
    /// <param name="owner">Owner node</param>
    /// <param name="usesMergedListShape">Whether a list has the merged-list shape</param>
    /// <returns>Updated owner node</returns>
    internal static SyntaxNode SplitAttributeLists(SyntaxNode owner, Func<AttributeListSyntax, bool> usesMergedListShape)
    {
        while (true)
        {
            var lists = AttributeTargetUtilities.GetAttributeLists(owner);
            var listToSplit = lists.FirstOrDefault(list => CanSplit(list, usesMergedListShape));

            if (listToSplit == null)
            {
                return owner;
            }

            owner = AttributeTargetUtilities.WithAttributeLists(owner, SyntaxFactory.List(BuildSplitAttributeLists(lists, listToSplit)));
        }
    }

    /// <summary>
    /// Merges the attribute lists of an owner that have the merged-list shape and resolve to the same target, one target at
    /// a time, as long as a group can be merged without touching a comment or directive. Lists of different targets are
    /// never merged, because the merged list keeps the first list's specifier and would move the other attributes to that
    /// target
    /// </summary>
    /// <param name="owner">Owner node</param>
    /// <param name="usesMergedListShape">Whether a list has the merged-list shape</param>
    /// <param name="separators">The separators the merge writes</param>
    /// <returns>Updated owner node</returns>
    internal static SyntaxNode MergeAttributeLists(SyntaxNode owner, Func<AttributeListSyntax, bool> usesMergedListShape, AttributeListMergeSeparators separators)
    {
        while (true)
        {
            var group = FindMergeableGroup(AttributeTargetUtilities.GetAttributeLists(owner), usesMergedListShape);

            if (group.Length == 0)
            {
                return owner;
            }

            owner = AttributeTargetUtilities.MergeAttributeListGroup(owner, group, separators);
        }
    }

    /// <summary>
    /// Determines whether an attribute list has the merged-list shape of a parameter or type parameter, whatever its owner:
    /// a list on a parameter or type parameter, and a <c>param:</c> or <c>typevar:</c> list on any owner
    /// </summary>
    /// <param name="attributeList">Attribute list</param>
    /// <returns>
    /// <see langword="true"/> when the list is never split and is merged with the lists of its target that have the
    /// same shape; otherwise, <see langword="false"/>
    /// </returns>
    internal static bool UsesMergedListShape(AttributeListSyntax attributeList)
    {
        return AttributeTargetFormattingShared.ResolveListShapeMode(attributeList) == TargetAttributeListShapeMode.MergedList;
    }

    /// <summary>
    /// Determines whether an attribute list is split
    /// </summary>
    /// <param name="attributeList">Attribute list</param>
    /// <param name="usesMergedListShape">Whether a list has the merged-list shape</param>
    /// <returns><see langword="true"/> when the list is split; otherwise, <see langword="false"/></returns>
    private static bool CanSplit(AttributeListSyntax attributeList, Func<AttributeListSyntax, bool> usesMergedListShape)
    {
        return attributeList.Attributes.Count > 1
               && AttributeTargetUtilities.TryResolveTarget(attributeList, out var _)
               && usesMergedListShape(attributeList) == false
               && SyntaxNodeUtilities.InteriorContainsCommentOrDirective(attributeList) == false;
    }

    /// <summary>
    /// Finds the first group of attribute lists that have the merged-list shape, resolve to the same target, and can be
    /// merged
    /// </summary>
    /// <param name="lists">The owner's attribute lists</param>
    /// <param name="usesMergedListShape">Whether a list has the merged-list shape</param>
    /// <returns>The mergeable group, or an empty array when no group can be merged</returns>
    private static AttributeListSyntax[] FindMergeableGroup(IReadOnlyList<AttributeListSyntax> lists, Func<AttributeListSyntax, bool> usesMergedListShape)
    {
        var candidates = lists.Where(list => AttributeTargetUtilities.TryResolveTarget(list, out var _)
                                             && usesMergedListShape(list))
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
    /// the original opening bracket with its leading trivia, the last one the original closing bracket with its trivia, and
    /// every list keeps the original specifier. The whitespace and line breaks behind the original opening bracket — in
    /// front of the specifier, or of the first attribute when there is none — are dropped, and so is everything between two
    /// attributes: the comma, and the whitespace, line breaks, and blank lines in front of it and behind it. The brackets the
    /// split creates carry no trivia, so nothing stands between two created lists, and no created list holds anything
    /// between its opening bracket and its specifier or attribute
    /// </summary>
    /// <param name="lists">The owner's current attribute lists</param>
    /// <param name="listToSplit">The multi-attribute list to split</param>
    /// <returns>The rebuilt attribute lists</returns>
    private static List<AttributeListSyntax> BuildSplitAttributeLists(IReadOnlyList<AttributeListSyntax> lists, AttributeListSyntax listToSplit)
    {
        var updatedLists = new List<AttributeListSyntax>(lists.Count - 1 + listToSplit.Attributes.Count);
        var lastIndex = listToSplit.Attributes.Count - 1;
        var target = listToSplit.Target?.WithLeadingTrivia(SyntaxFactory.TriviaList());

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

                if (index > 0
                    || target == null)
                {
                    attribute = attribute.WithLeadingTrivia(SyntaxFactory.TriviaList());
                }

                if (index < lastIndex)
                {
                    attribute = attribute.WithTrailingTrivia(SyntaxFactory.TriviaList());
                }

                var openBracket = index == 0
                                      ? listToSplit.OpenBracketToken.WithTrailingTrivia(SyntaxFactory.TriviaList())
                                      : SyntaxFactory.Token(SyntaxFactory.TriviaList(), SyntaxKind.OpenBracketToken, SyntaxFactory.TriviaList());
                var closeBracket = index == lastIndex
                                       ? listToSplit.CloseBracketToken
                                       : SyntaxFactory.Token(SyntaxFactory.TriviaList(), SyntaxKind.CloseBracketToken, SyntaxFactory.TriviaList());

                updatedLists.Add(SyntaxFactory.AttributeList(openBracket, target, SyntaxFactory.SingletonSeparatedList(attribute), closeBracket));
            }
        }

        return updatedLists;
    }

    #endregion // Methods
}