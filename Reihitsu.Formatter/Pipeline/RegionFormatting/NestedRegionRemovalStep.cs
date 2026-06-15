using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Core;

namespace Reihitsu.Formatter.Pipeline.RegionFormatting;

/// <summary>
/// The removal half of the region phase. It strips <c>#region</c> and <c>#endregion</c> directives
/// that sit inside an element body, reparsing the changed text to rebuild the tree. Naming and
/// endregion synchronization belong to <see cref="RegionNamingRewriter"/>
/// </summary>
internal static class NestedRegionRemovalStep
{
    #region Methods

    /// <summary>
    /// Removes region directives placed within element bodies
    /// </summary>
    /// <param name="root">The syntax root</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated root</returns>
    public static SyntaxNode Remove(SyntaxNode root, CancellationToken cancellationToken)
    {
        var sourceText = root.SyntaxTree?.GetText(cancellationToken) ?? SourceText.From(root.ToFullString());
        var removalSpans = new List<TextSpan>();

        foreach (var trivia in root.DescendantTrivia(descendIntoTrivia: true))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if ((trivia.IsKind(SyntaxKind.RegionDirectiveTrivia) || trivia.IsKind(SyntaxKind.EndRegionDirectiveTrivia))
                && RegionDirectiveUtilities.IsWithinElementBody(trivia))
            {
                var line = sourceText.Lines.GetLineFromPosition(trivia.Span.Start);
                var removalEnd = line.EndIncludingLineBreak > line.End
                                     ? line.EndIncludingLineBreak
                                     : line.End;

                removalSpans.Add(TextSpan.FromBounds(line.Start, removalEnd));
            }
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

        return root.SyntaxTree.WithChangedText(updatedText).GetRoot(cancellationToken);
    }

    #endregion // Methods
}