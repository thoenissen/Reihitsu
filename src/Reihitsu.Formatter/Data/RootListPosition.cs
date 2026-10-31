using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;
using Reihitsu.Formatter.Pipeline.BlankLines.Rewriter;
using Reihitsu.Formatter.Pipeline.LineBreaks.Utilities;

namespace Reihitsu.Formatter.Data;

/// <summary>
/// The facts about the position of a formatting root inside the statement list or switch-section list of its document,
/// which the list-level blank-line decisions read. Those decisions run in the visitor of the list's parent, and that parent
/// lies outside a formatting run rooted at the list element itself. The facts are plain values (FMT-PIPE-024), captured
/// before the pipeline runs from the document as it stands: the kind of the statement in front of the root and of that
/// statement's last token, whether the switch section in front of the root ends in a <see langword="break"/> statement, and
/// whether the root is the terminal <see langword="break"/> directly inside its switch section
/// </summary>
internal readonly struct RootListPosition
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="previousStatementKind">The kind of the statement that precedes the root statement in its block or switch section</param>
    /// <param name="previousStatementLastTokenKind">The kind of the last token of that statement</param>
    /// <param name="isBehindSectionEndingInBreak">Whether the switch section that precedes the root switch section ends in a <see langword="break"/> statement</param>
    /// <param name="isTerminalDirectSwitchSectionBreak">Whether the root is the terminal <see langword="break"/> directly inside its switch section</param>
    private RootListPosition(SyntaxKind previousStatementKind, SyntaxKind previousStatementLastTokenKind, bool isBehindSectionEndingInBreak, bool isTerminalDirectSwitchSectionBreak)
    {
        PreviousStatementKind = previousStatementKind;
        PreviousStatementLastTokenKind = previousStatementLastTokenKind;
        IsBehindSectionEndingInBreak = isBehindSectionEndingInBreak;
        IsTerminalDirectSwitchSectionBreak = isTerminalDirectSwitchSectionBreak;
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// The kind of the statement that precedes the root statement in its block or switch section in the document, or
    /// <see cref="SyntaxKind.None"/> when the root is no statement of such a list, is the first one, or does not start its line
    /// </summary>
    public SyntaxKind PreviousStatementKind { get; }

    /// <summary>
    /// The kind of the last token of the statement that precedes the root statement, or <see cref="SyntaxKind.None"/> when
    /// there is no such statement
    /// </summary>
    public SyntaxKind PreviousStatementLastTokenKind { get; }

    /// <summary>
    /// Whether a statement precedes the root statement in its block or switch section
    /// </summary>
    public bool HasPreviousStatement => PreviousStatementKind != SyntaxKind.None;

    /// <summary>
    /// Whether the root is a switch section that starts its line and the switch section in front of it in its switch statement
    /// ends in a <see langword="break"/> statement
    /// </summary>
    public bool IsBehindSectionEndingInBreak { get; }

    /// <summary>
    /// Whether the root is the terminal <see langword="break"/> directly inside its switch section, evaluated in the document
    /// because a detached root no longer reaches its parent
    /// </summary>
    public bool IsTerminalDirectSwitchSectionBreak { get; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Captures the position of the specified formatting root inside the list that contains it in its document. A root
    /// whose first token does not start its line gets no position, because its leading trivia is restored from the document.
    /// Neither does a block statement unless an own-line comment is the last content above its opening brace; see
    /// <see cref="KeepsBlankLineAboveBlockStatement"/>
    /// </summary>
    /// <param name="root">The formatting root, still attached to its document</param>
    /// <returns>The position of the root</returns>
    public static RootListPosition From(SyntaxNode root)
    {
        var firstToken = root.GetFirstToken();

        if (SyntaxTriviaUtilities.AreSeparatedByEndOfLine(firstToken.GetPreviousToken(), firstToken) == false)
        {
            return default;
        }

        switch (root)
        {
            case BlockSyntax block when KeepsBlankLineAboveBlockStatement(block) == false:
                {
                    return default;
                }

            case StatementSyntax statement when statement.Parent is BlockSyntax block:
                {
                    return ForStatement(GetPrevious(block.Statements, statement), false);
                }

            case StatementSyntax statement when statement.Parent is SwitchSectionSyntax section:
                {
                    return ForStatement(GetPrevious(section.Statements, statement),
                                        BlankLineSpacingPolicy.IsTerminalDirectSwitchSectionBreak(statement));
                }

            case SwitchSectionSyntax section when section.Parent is SwitchStatementSyntax switchStatement:
                {
                    var previousSection = GetPrevious(switchStatement.Sections, section);

                    return new RootListPosition(SyntaxKind.None,
                                                SyntaxKind.None,
                                                previousSection != null && BlankLineBreakSpacingRewriter.EndsInBreak(previousSection),
                                                false);
                }

            default:
                {
                    return default;
                }
        }
    }

    /// <summary>
    /// Determines whether a block statement gets a position, because a blank line that the list-level rules insert above it
    /// is kept by document-level formatting in a way this position reproduces. The line-break phase leaves no blank line
    /// directly above the opening brace of a block statement. When the last content of the gap is an own-line comment or
    /// directive that owns its own placement (<see cref="TokenGapNormalizer.HasOwnLinePlacementOwner"/>), it keeps everything
    /// up to that content. The list-level rules insert their blank line at the start of the gap or right after its last
    /// directive, so under such an owner the blank line survives exactly when the owner is an own-line comment rather than a
    /// directive. Every other gap is left without a position on purpose: when its last content is glued to the brace, a
    /// documentation comment, or a region directive, the line-break phase decides the gap itself in document-level
    /// formatting, which a position for the list-level rules cannot reproduce
    /// </summary>
    /// <param name="block">The block statement</param>
    /// <returns><see langword="true"/> if a blank line inserted above the block statement survives the line-break phase</returns>
    private static bool KeepsBlankLineAboveBlockStatement(BlockSyntax block)
    {
        var leadingTrivia = block.OpenBraceToken.LeadingTrivia;

        if (TokenGapNormalizer.HasOwnLinePlacementOwner(leadingTrivia) == false)
        {
            return false;
        }

        return leadingTrivia[SyntaxTriviaUtilities.FindLastSignificantTriviaIndex(leadingTrivia)].IsDirective == false;
    }

    /// <summary>
    /// Creates the position of a root statement from the statement in front of it
    /// </summary>
    /// <param name="previousStatement">The statement in front of the root statement, or <see langword="null"/> when there is none</param>
    /// <param name="isTerminalDirectSwitchSectionBreak">Whether the root is the terminal <see langword="break"/> directly inside its switch section</param>
    /// <returns>The position</returns>
    private static RootListPosition ForStatement(StatementSyntax previousStatement, bool isTerminalDirectSwitchSectionBreak)
    {
        return previousStatement == null
                   ? new RootListPosition(SyntaxKind.None, SyntaxKind.None, false, isTerminalDirectSwitchSectionBreak)
                   : new RootListPosition(previousStatement.Kind(), previousStatement.GetLastToken().Kind(), false, isTerminalDirectSwitchSectionBreak);
    }

    /// <summary>
    /// Gets the element that precedes the specified element in its list
    /// </summary>
    /// <typeparam name="TNode">The element type</typeparam>
    /// <param name="list">The list that contains the element</param>
    /// <param name="element">The element</param>
    /// <returns>The preceding element, or <see langword="null"/> when the element is the first one</returns>
    private static TNode GetPrevious<TNode>(SyntaxList<TNode> list, TNode element)
        where TNode : SyntaxNode
    {
        var index = list.IndexOf(element);

        return index > 0
                   ? list[index - 1]
                   : null;
    }

    #endregion // Methods
}