using System.Collections.Generic;

using Microsoft.CodeAnalysis;

using Reihitsu.Core.Enumerations;

namespace Reihitsu.Core;

/// <summary>
/// A part of a member-access chain that belongs to the element in front of it: an argument list, an element access,
/// a conditional element access, or a null-forgiving operator that is not part of a link operator. An attached part is
/// never a chain link and never an alignment anchor
/// </summary>
public sealed class FluentChainAttachedPart
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="kind">The kind of the attached part</param>
    /// <param name="tokens">The leading tokens of the part, from its first token to its opening token</param>
    internal FluentChainAttachedPart(FluentChainAttachedPartKind kind, IReadOnlyList<SyntaxToken> tokens)
    {
        Kind = kind;
        Tokens = tokens;
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// The kind of the attached part
    /// </summary>
    public FluentChainAttachedPartKind Kind { get; }

    /// <summary>
    /// The leading tokens of the part, in source order: <c>(</c>; <c>[</c>; <c>?</c> and <c>[</c>; <c>!</c>, <c>?</c>
    /// and <c>[</c>; or the single <c>!</c>. The gaps between them belong to the part
    /// </summary>
    public IReadOnlyList<SyntaxToken> Tokens { get; }

    /// <summary>
    /// The first token of the part
    /// </summary>
    public SyntaxToken FirstToken => Tokens[0];

    #endregion // Properties
}