using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Core;

/// <summary>
/// A link of a member-access chain: a link operator (<c>.</c>, <c>?.</c>, <c>!.</c> or <c>!?.</c>) and the member
/// name behind it. <c>?</c>, <c>!</c> and <c>.</c> are treated alike: a <c>!</c> or <c>?</c> directly in front of the
/// dot belongs to the operator, and the operator's first token represents the link
/// </summary>
public sealed class FluentChainLink
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="node">The member access or member binding that forms the link</param>
    /// <param name="operatorTokens">The operator tokens in source order, ending with the dot</param>
    /// <param name="name">The member name</param>
    /// <param name="isInvoked">Whether the member is invoked</param>
    internal FluentChainLink(ExpressionSyntax node, IReadOnlyList<SyntaxToken> operatorTokens, SimpleNameSyntax name, bool isInvoked)
    {
        Node = node;
        OperatorTokens = operatorTokens;
        Name = name;
        IsInvoked = isInvoked;
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// The member access or member binding that forms the link
    /// </summary>
    public ExpressionSyntax Node { get; }

    /// <summary>
    /// The operator tokens in source order: <c>.</c>; <c>?</c> and <c>.</c>; <c>!</c> and <c>.</c>; or <c>!</c>,
    /// <c>?</c> and <c>.</c>
    /// </summary>
    public IReadOnlyList<SyntaxToken> OperatorTokens { get; }

    /// <summary>
    /// The operator's first token, which represents the link: it starts the line of a wrapped link and is its alignment
    /// column
    /// </summary>
    public SyntaxToken OperatorToken => OperatorTokens[0];

    /// <summary>
    /// The operator's dot
    /// </summary>
    public SyntaxToken DotToken => OperatorTokens[OperatorTokens.Count - 1];

    /// <summary>
    /// The member name
    /// </summary>
    public SimpleNameSyntax Name { get; }

    /// <summary>
    /// Whether the member is invoked
    /// </summary>
    public bool IsInvoked { get; }

    /// <summary>
    /// Whether the operator starts a line, that is, whether a line break separates it from the end of the token in front
    /// of it. A link directly behind the closing delimiter of a multi-line raw string literal therefore does not start a
    /// line
    /// </summary>
    public bool StartsLine => SyntaxTokenPositionUtilities.IsFirstOnLine(OperatorToken);

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Determines whether the operator holds a line break between its own tokens (<c>?</c> ⏎ <c>.</c>, <c>!</c> ⏎
    /// <c>.</c>, <c>!</c> ⏎ <c>?.</c>)
    /// </summary>
    /// <param name="isJoinableOnly">Whether a line break that a comment, a preprocessor directive or disabled text keeps in place is ignored</param>
    /// <returns><see langword="true"/> if the operator holds a line break</returns>
    public bool HasInnerLineBreak(bool isJoinableOnly)
    {
        for (var tokenIndex = 1; tokenIndex < OperatorTokens.Count; tokenIndex++)
        {
            var previousToken = OperatorTokens[tokenIndex - 1];
            var token = OperatorTokens[tokenIndex];

            if (SyntaxTokenPositionUtilities.IsFirstOnLine(token)
                && (isJoinableOnly == false
                    || SyntaxTriviaUtilities.WouldJoinAcrossUnjoinableTrivia(previousToken, token) == false))
            {
                return true;
            }
        }

        return false;
    }

    #endregion // Methods
}