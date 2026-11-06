using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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
    /// behavior: two members of one context — both static or both instance — that each have a runtime initializer,
    /// or two instance fields of a type whose field order sets its layout. Members in code that an <c>#if</c> branch
    /// excludes count as well, because another build configuration compiles them
    /// </summary>
    /// <param name="typeDeclaration">Type declaration whose blocks are reordered</param>
    /// <returns><see langword="true"/> if the reorder changes initializer execution order or the layout</returns>
    public bool ChangesMemberOrder(TypeDeclarationSyntax typeDeclaration)
    {
        var setsLayout = OrderingDeclarationUtilities.FieldOrderSetsLayout(typeDeclaration);
        var members = typeDeclaration.Members.Select(member => (Position: member.SpanStart, Member: member))
                                             .Concat(GetMembersOfDisabledText(typeDeclaration))
                                             .ToList();

        bool HasStaticInitializer(MemberDeclarationSyntax member)
        {
            return OrderingDeclarationUtilities.HasInitializer(member) && OrderingDeclarationUtilities.IsStatic(member);
        }

        bool HasInstanceInitializer(MemberDeclarationSyntax member)
        {
            return OrderingDeclarationUtilities.HasInitializer(member) && OrderingDeclarationUtilities.IsStatic(member) == false;
        }

        bool IsLayoutField(MemberDeclarationSyntax member)
        {
            return setsLayout && OrderingDeclarationUtilities.IsLayoutField(member);
        }

        return ChangesOrder(members.Where(member => HasStaticInitializer(member.Member)).Select(member => member.Position))
               || ChangesOrder(members.Where(member => HasInstanceInitializer(member.Member)).Select(member => member.Position))
               || ChangesOrder(members.Where(member => IsLayoutField(member.Member)).Select(member => member.Position));
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
    /// Gets the members declared in the disabled text of the type's body, outside every member, with the position
    /// of that text
    /// </summary>
    /// <param name="typeDeclaration">Type declaration</param>
    /// <returns>The members of the disabled text</returns>
    private static IEnumerable<(int Position, MemberDeclarationSyntax Member)> GetMembersOfDisabledText(TypeDeclarationSyntax typeDeclaration)
    {
        var bodyStart = typeDeclaration.OpenBraceToken.Span.End;
        var bodyEnd = typeDeclaration.CloseBraceToken.SpanStart;
        var options = typeDeclaration.SyntaxTree.Options as CSharpParseOptions;

        foreach (var trivia in typeDeclaration.DescendantTrivia())
        {
            if (trivia.IsKind(SyntaxKind.DisabledTextTrivia) == false
                || trivia.SpanStart < bodyStart
                || trivia.SpanStart >= bodyEnd
                || typeDeclaration.Members.Any(member => member.Span.Contains(trivia.SpanStart)))
            {
                continue;
            }

            var parsedType = SyntaxFactory.ParseCompilationUnit($"class C\n{{\n{trivia.ToFullString()}\n}}", options: options)
                                          .Members
                                          .OfType<TypeDeclarationSyntax>()
                                          .FirstOrDefault();

            if (parsedType == null)
            {
                continue;
            }

            foreach (var member in parsedType.Members)
            {
                yield return (trivia.SpanStart, member);
            }
        }
    }

    /// <summary>
    /// Determines whether the reorder changes the relative order of the code at the given positions
    /// </summary>
    /// <param name="positions">Positions of the code</param>
    /// <returns><see langword="true"/> if the order changes</returns>
    private bool ChangesOrder(IEnumerable<int> positions)
    {
        var writtenOrder = positions.OrderBy(position => position).ToList();
        var newOrder = writtenOrder.OrderBy(position => GetNewRank(GetSegment(position)))
                                   .ThenBy(position => position);

        return writtenOrder.SequenceEqual(newOrder) == false;
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