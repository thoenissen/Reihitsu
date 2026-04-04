using Microsoft.CodeAnalysis;

namespace Reihitsu.Formatter.Rules.Regions;

/// <summary>
/// Represents a matched pair of <c>#region</c> and <c>#endregion</c> directive trivia.
/// </summary>
internal readonly struct RegionPair
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="region">The <c>#region</c> directive trivia.</param>
    /// <param name="endRegion">The matching <c>#endregion</c> directive trivia.</param>
    public RegionPair(SyntaxTrivia region, SyntaxTrivia endRegion)
    {
        Region = region;
        EndRegion = endRegion;
    }

    /// <summary>
    /// The <c>#region</c> directive trivia.
    /// </summary>
    public SyntaxTrivia Region { get; }

    /// <summary>
    /// The matching <c>#endregion</c> directive trivia.
    /// </summary>
    public SyntaxTrivia EndRegion { get; }
}