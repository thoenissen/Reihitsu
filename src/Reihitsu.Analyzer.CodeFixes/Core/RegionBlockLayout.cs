using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Core;

namespace Reihitsu.Analyzer.CodeFixes.Core;

/// <summary>
/// The layout of a type body whose top-level region blocks are put into a new order, while the text between two
/// blocks, and in front of the first and behind the last, keeps its place. The body consists of segments in written
/// order: a gap, a block, a gap, … , a block, a gap. A reorder changes which block stands at each block position;
/// the gaps stay where they are
/// </summary>
internal sealed class RegionBlockLayout
{
    #region Fields

    /// <summary>
    /// Spans of the blocks in written order
    /// </summary>
    private readonly IReadOnlyList<TextSpan> _blockSpans;

    /// <summary>
    /// For each block position, the index of the block that the reorder puts there
    /// </summary>
    private readonly IReadOnlyList<int> _order;

    /// <summary>
    /// For each block, the block position the reorder puts it at
    /// </summary>
    private readonly int[] _positionOfBlock;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="blockSpans">Spans of the blocks in written order</param>
    /// <param name="order">For each block position, the index of the block that the reorder puts there</param>
    public RegionBlockLayout(IReadOnlyList<TextSpan> blockSpans, IReadOnlyList<int> order)
    {
        _blockSpans = blockSpans;
        _order = order;
        _positionOfBlock = new int[order.Count];

        for (var position = 0; position < order.Count; position++)
        {
            _positionOfBlock[order[position]] = position;
        }
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether the reorder puts two members of the type into the other order where their order is
    /// behavior, as <see cref="OrderingDeclarationUtilities.ChangesMemberOrder(TypeDeclarationSyntax, INamedTypeSymbol, System.Func{int, int})"/>
    /// decides it for the segments of this layout
    /// </summary>
    /// <param name="typeDeclaration">Type declaration whose blocks are reordered</param>
    /// <param name="typeSymbol">Symbol of the type, which decides whether the order of its instance fields sets its layout</param>
    /// <returns><see langword="true"/> if the reorder changes initializer execution order or the layout</returns>
    public bool ChangesMemberOrder(TypeDeclarationSyntax typeDeclaration, INamedTypeSymbol typeSymbol)
    {
        return OrderingDeclarationUtilities.ChangesMemberOrder(typeDeclaration, typeSymbol, position => GetNewRank(GetSegment(position)));
    }

    /// <summary>
    /// Determines whether the text from one position up to another stays the same sequence of segments after the
    /// reorder, so that the code at the first position still stands directly in front of the code at the second
    /// </summary>
    /// <param name="start">First position</param>
    /// <param name="end">Second position, behind the first</param>
    /// <returns><see langword="true"/> if the segments from the first to the second position stay together in order</returns>
    public bool KeepsSequenceBetween(int start, int end)
    {
        var firstSegment = GetSegment(start);
        var lastSegment = GetSegment(end);

        if (firstSegment == lastSegment)
        {
            return true;
        }

        var segmentCount = (2 * _blockSpans.Count) + 1;
        var newFirstRank = GetNewRank(firstSegment);

        for (var offset = 0; offset <= lastSegment - firstSegment; offset++)
        {
            var newRank = newFirstRank + offset;

            if (newRank >= segmentCount
                || GetSegmentAtNewRank(newRank) != firstSegment + offset)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Gets the segment of the position in written order: <c>2 * i + 1</c> for block <c>i</c>, and <c>2 * g</c> for
    /// the gap in front of block <c>g</c>, or behind the last block
    /// </summary>
    /// <param name="position">Position</param>
    /// <returns>The segment</returns>
    private int GetSegment(int position)
    {
        var gap = 0;

        for (var index = 0; index < _blockSpans.Count; index++)
        {
            if (_blockSpans[index].Contains(position))
            {
                return (2 * index) + 1;
            }

            if (_blockSpans[index].End <= position)
            {
                gap = index + 1;
            }
        }

        return 2 * gap;
    }

    /// <summary>
    /// Gets the rank of the segment after the reorder
    /// </summary>
    /// <param name="segment">Segment in written order</param>
    /// <returns>The rank after the reorder</returns>
    private int GetNewRank(int segment)
    {
        return segment % 2 == 0
                   ? segment
                   : (2 * _positionOfBlock[segment / 2]) + 1;
    }

    /// <summary>
    /// Gets the segment that stands at the rank after the reorder
    /// </summary>
    /// <param name="rank">Rank after the reorder</param>
    /// <returns>The segment in written order</returns>
    private int GetSegmentAtNewRank(int rank)
    {
        return rank % 2 == 0
                   ? rank
                   : (2 * _order[rank / 2]) + 1;
    }

    #endregion // Methods
}