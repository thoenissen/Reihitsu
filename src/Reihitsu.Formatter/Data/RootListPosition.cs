using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;
using Reihitsu.Formatter.Pipeline.LineBreaks.Utilities;
using Reihitsu.Formatter.Utilities;

namespace Reihitsu.Formatter.Data;

/// <summary>
/// The facts about the position of a formatting root inside the statement list or switch-section list of its document,
/// which the list-level blank-line decisions read. Those decisions run in the visitor of the list's parent, and that parent
/// lies outside a formatting run rooted at the list element itself. These facts are captured before the pipeline runs, from
/// the document as it stands, so that a phase replacing the root does not change them. Only kinds and last tokens of the
/// captured nodes may be read, never their positions
/// </summary>
internal readonly struct RootListPosition
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="previousStatement">The statement that precedes the root statement in its block or switch section</param>
    /// <param name="previousSection">The switch section that precedes the root switch section in its switch statement</param>
    /// <param name="isTerminalDirectSwitchSectionBreak">Whether the root is the terminal <see langword="break"/> directly inside its switch section</param>
    private RootListPosition(StatementSyntax previousStatement, SwitchSectionSyntax previousSection, bool isTerminalDirectSwitchSectionBreak)
    {
        PreviousStatement = previousStatement;
        PreviousSection = previousSection;
        IsTerminalDirectSwitchSectionBreak = isTerminalDirectSwitchSectionBreak;
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// The statement that precedes the root statement in its block or switch section in the document, or
    /// <see langword="null"/> when the root is no statement of such a list, is the first one, or does not start its line
    /// </summary>
    public StatementSyntax PreviousStatement { get; }

    /// <summary>
    /// The switch section that precedes the root switch section in its switch statement in the document, or
    /// <see langword="null"/> when the root is no switch section, is the first one, or does not start its line
    /// </summary>
    public SwitchSectionSyntax PreviousSection { get; }

    /// <summary>
    /// Whether the root is the terminal <see langword="break"/> directly inside its switch section, evaluated in the document
    /// because a replaced root no longer reaches its parent
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
        if (ReihitsuFormatterHelpers.StartsOnNewLineIncludingDocumentation(root.GetFirstToken()) == false)
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
                    return new RootListPosition(GetPrevious(block.Statements, statement), null, false);
                }

            case StatementSyntax statement when statement.Parent is SwitchSectionSyntax section:
                {
                    return new RootListPosition(GetPrevious(section.Statements, statement),
                                                null,
                                                BlankLineSpacingPolicy.IsTerminalDirectSwitchSectionBreak(statement));
                }

            case SwitchSectionSyntax section when section.Parent is SwitchStatementSyntax switchStatement:
                {
                    return new RootListPosition(null, GetPrevious(switchStatement.Sections, section), false);
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