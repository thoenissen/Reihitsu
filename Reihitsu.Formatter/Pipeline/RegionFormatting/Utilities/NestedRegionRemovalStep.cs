using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Core;

namespace Reihitsu.Formatter.Pipeline.RegionFormatting.Utilities;

/// <summary>
/// The removal half of the region phase. It strips <c>#region</c> and <c>#endregion</c> directives
/// that sit inside an element body, except direct accessor-list pairs that act as layout barriers,
/// reparsing the changed text to rebuild the tree. Naming and endregion synchronization belong to
/// <see cref="RegionNamingRewriter"/>
/// </summary>
internal static class NestedRegionRemovalStep
{
    #region Methods

    /// <summary>
    /// Removes region directives placed within element bodies, except direct pairs inside the same
    /// accessor list. A region inside a branch the compiler skipped is left in place, because deleting
    /// its line would remove source the formatter otherwise leaves untouched
    /// </summary>
    /// <param name="root">The syntax root</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated root</returns>
    public static SyntaxNode Remove(SyntaxNode root, CancellationToken cancellationToken)
    {
        var syntaxTree = root.SyntaxTree;
        var sourceText = syntaxTree?.GetText(cancellationToken) ?? SourceText.From(root.ToFullString());
        var removalSpans = new List<TextSpan>();

        foreach (var trivia in root.DescendantTrivia(descendIntoTrivia: true))
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Removing a region the compiler skipped would delete a line of code the formatter is otherwise
            // leaving alone, and the surrounding disabled text keeps no record of what was there (issue #434)
            if (trivia.IsKind(SyntaxKind.RegionDirectiveTrivia) == false
                || SyntaxTriviaUtilities.IsInactiveDirective(trivia))
            {
                continue;
            }

            if (TryGetRemovableRegionPair(trivia, cancellationToken, out var endRegionTrivia) == false)
            {
                continue;
            }

            removalSpans.Add(GetLineRemovalSpan(sourceText, trivia));
            removalSpans.Add(GetLineRemovalSpan(sourceText, endRegionTrivia));
        }

        if (removalSpans.Count == 0)
        {
            return root;
        }

        var updatedText = sourceText;

        foreach (var removalSpan in removalSpans.OrderByDescending(static span => span.Start))
        {
            updatedText = updatedText.Replace(removalSpan, string.Empty);
        }

        // When the node is detached from a syntax tree, reparse the changed text instead of
        // dereferencing a null tree
        if (syntaxTree == null)
        {
            return CSharpSyntaxTree.ParseText(updatedText, cancellationToken: cancellationToken).GetRoot(cancellationToken);
        }

        return syntaxTree.WithChangedText(updatedText).GetRoot(cancellationToken);
    }

    /// <summary>
    /// Gets the span covering the whole line a directive sits on, including its line break
    /// </summary>
    /// <param name="sourceText">The source text</param>
    /// <param name="trivia">The directive trivia</param>
    /// <returns>The span to remove</returns>
    private static TextSpan GetLineRemovalSpan(SourceText sourceText, SyntaxTrivia trivia)
    {
        var line = sourceText.Lines.GetLineFromPosition(trivia.Span.Start);
        var removalEnd = line.EndIncludingLineBreak > line.End
                             ? line.EndIncludingLineBreak
                             : line.End;

        return TextSpan.FromBounds(line.Start, removalEnd);
    }

    /// <summary>
    /// Determines whether both directives in an active region pair sit directly inside the same
    /// accessor list
    /// </summary>
    /// <param name="regionTrivia">The opening region directive</param>
    /// <param name="endRegionTrivia">The closing region directive</param>
    /// <returns><see langword="true"/> when the pair must be preserved as accessor-list layout</returns>
    private static bool IsProtectedAccessorListPair(SyntaxTrivia regionTrivia, SyntaxTrivia endRegionTrivia)
    {
        if (SyntaxTriviaUtilities.IsInactiveDirective(regionTrivia)
            || SyntaxTriviaUtilities.IsInactiveDirective(endRegionTrivia))
        {
            return false;
        }

        if (GetNearestElementBody(regionTrivia) is not AccessorListSyntax accessorList
            || GetNearestElementBody(endRegionTrivia) is not AccessorListSyntax endAccessorList
            || accessorList.Span != endAccessorList.Span)
        {
            return false;
        }

        return accessorList.Span.Contains(regionTrivia.Span)
               && accessorList.Span.Contains(endRegionTrivia.Span);
    }

    /// <summary>
    /// Gets the nearest ancestor that makes a directive part of an element body
    /// </summary>
    /// <param name="directiveTrivia">The directive to classify</param>
    /// <returns>The nearest element-body ancestor, or <see langword="null"/> when none exists</returns>
    private static SyntaxNode GetNearestElementBody(SyntaxTrivia directiveTrivia)
    {
        var currentNode = directiveTrivia.Token.Parent;

        while (currentNode != null)
        {
            switch (currentNode)
            {
                case BlockSyntax:
                case AccessorListSyntax:
                case AnonymousFunctionExpressionSyntax:
                case LocalFunctionStatementSyntax:
                case StatementSyntax:
                    return currentNode;

                case TypeDeclarationSyntax:
                case NamespaceDeclarationSyntax:
                case FileScopedNamespaceDeclarationSyntax:
                case CompilationUnitSyntax:
                    return null;

                default:
                    currentNode = currentNode.Parent;
                    break;
            }
        }

        return null;
    }

    /// <summary>
    /// Determines whether a <c>#region</c> directive and its matching <c>#endregion</c> may both be removed
    /// </summary>
    /// <param name="regionTrivia">The <c>#region</c> directive trivia</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <param name="endRegionTrivia">The matching <c>#endregion</c> directive trivia when the pair is removable</param>
    /// <returns><see langword="true"/> if both halves of the pair may be removed; otherwise, <see langword="false"/></returns>
    /// <remarks>
    /// Except for a pair placed directly inside one accessor list, one qualifying half is enough,
    /// and then both are removed. A pair that straddles an element-body boundary — the
    /// <c>#region</c> inside a body and its <c>#endregion</c> outside, or the reverse — must not lose
    /// only its qualifying half, because the orphaned directive turns source that compiles into
    /// source that does not (CS1028). Removing the pair as a unit also matches
    /// <c>RH7303DoNotPlaceRegionsWithinElementsCodeFixProvider</c>, which deletes both halves once
    /// either one is reported, so the formatter and the code fix agree on the same input.
    /// </remarks>
    private static bool TryGetRemovableRegionPair(SyntaxTrivia regionTrivia,
                                                  CancellationToken cancellationToken,
                                                  out SyntaxTrivia endRegionTrivia)
    {
        endRegionTrivia = default;

        if (regionTrivia.GetStructure() is not RegionDirectiveTriviaSyntax regionDirective)
        {
            return false;
        }

        cancellationToken.ThrowIfCancellationRequested();

        var relatedDirectives = regionDirective.GetRelatedDirectives();

        if (relatedDirectives.Count != 2 || relatedDirectives[1] is not EndRegionDirectiveTriviaSyntax endRegionDirective)
        {
            return false;
        }

        var candidate = endRegionDirective.ParentTrivia;

        if (candidate.IsKind(SyntaxKind.EndRegionDirectiveTrivia) == false)
        {
            return false;
        }

        if (RegionDirectiveUtilities.IsWithinElementBody(regionTrivia) == false
            && RegionDirectiveUtilities.IsWithinElementBody(candidate) == false)
        {
            return false;
        }

        if (IsProtectedAccessorListPair(regionTrivia, candidate))
        {
            return false;
        }

        endRegionTrivia = candidate;

        return true;
    }

    #endregion // Methods
}