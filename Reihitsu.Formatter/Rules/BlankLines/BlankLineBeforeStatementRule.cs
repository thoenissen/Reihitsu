using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Rules.BlankLines;

/// <summary>
/// Ensures that certain statements are preceded by a blank line,
/// unless the statement is the first in a block.
/// Consolidates rules RH0303–RH0321.
/// </summary>
internal sealed class BlankLineBeforeStatementRule : FormattingRuleBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context">The formatting context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public BlankLineBeforeStatementRule(FormattingContext context, CancellationToken cancellationToken)
        : base(context, cancellationToken)
    {
    }

    #endregion // Constructor

    #region IFormattingRule

    /// <inheritdoc/>
    public override FormattingPhase Phase => FormattingPhase.BlankLineManagement;

    #endregion // IFormattingRule

    #region FormattingRuleBase

    /// <inheritdoc/>
    public override SyntaxNode VisitTryStatement(TryStatementSyntax node)
    {
        var visited = (TryStatementSyntax)base.VisitTryStatement(node);

        return EnsureBlankLineBefore(node, node.TryKeyword, visited);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitIfStatement(IfStatementSyntax node)
    {
        var visited = (IfStatementSyntax)base.VisitIfStatement(node);

        var previousToken = node.IfKeyword.GetPreviousToken();

        if (previousToken.IsKind(SyntaxKind.ElseKeyword))
        {
            return visited;
        }

        return EnsureBlankLineBefore(node, node.IfKeyword, visited);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitWhileStatement(WhileStatementSyntax node)
    {
        var visited = (WhileStatementSyntax)base.VisitWhileStatement(node);

        return EnsureBlankLineBefore(node, node.WhileKeyword, visited);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitDoStatement(DoStatementSyntax node)
    {
        var visited = (DoStatementSyntax)base.VisitDoStatement(node);

        return EnsureBlankLineBefore(node, node.DoKeyword, visited);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitUsingStatement(UsingStatementSyntax node)
    {
        var visited = (UsingStatementSyntax)base.VisitUsingStatement(node);

        return EnsureBlankLineBefore(node, node.UsingKeyword, visited);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitForEachStatement(ForEachStatementSyntax node)
    {
        var visited = (ForEachStatementSyntax)base.VisitForEachStatement(node);

        return EnsureBlankLineBefore(node, node.ForEachKeyword, visited);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitForStatement(ForStatementSyntax node)
    {
        var visited = (ForStatementSyntax)base.VisitForStatement(node);

        return EnsureBlankLineBefore(node, node.ForKeyword, visited);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitReturnStatement(ReturnStatementSyntax node)
    {
        var visited = (ReturnStatementSyntax)base.VisitReturnStatement(node);

        return EnsureBlankLineBefore(node, node.ReturnKeyword, visited);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitGotoStatement(GotoStatementSyntax node)
    {
        var visited = (GotoStatementSyntax)base.VisitGotoStatement(node);

        return EnsureBlankLineBefore(node, node.GotoKeyword, visited);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitBreakStatement(BreakStatementSyntax node)
    {
        var visited = (BreakStatementSyntax)base.VisitBreakStatement(node);

        if (IsInsideSwitchSection(node))
        {
            return visited;
        }

        return EnsureBlankLineBefore(node, node.BreakKeyword, visited);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitContinueStatement(ContinueStatementSyntax node)
    {
        var visited = (ContinueStatementSyntax)base.VisitContinueStatement(node);

        return EnsureBlankLineBefore(node, node.ContinueKeyword, visited);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitThrowStatement(ThrowStatementSyntax node)
    {
        var visited = (ThrowStatementSyntax)base.VisitThrowStatement(node);

        return EnsureBlankLineBefore(node, node.ThrowKeyword, visited);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitSwitchStatement(SwitchStatementSyntax node)
    {
        var visited = (SwitchStatementSyntax)base.VisitSwitchStatement(node);

        return EnsureBlankLineBefore(node, node.SwitchKeyword, visited);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitCheckedStatement(CheckedStatementSyntax node)
    {
        var visited = (CheckedStatementSyntax)base.VisitCheckedStatement(node);

        return EnsureBlankLineBefore(node, node.Keyword, visited);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitFixedStatement(FixedStatementSyntax node)
    {
        var visited = (FixedStatementSyntax)base.VisitFixedStatement(node);

        return EnsureBlankLineBefore(node, node.FixedKeyword, visited);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitLockStatement(LockStatementSyntax node)
    {
        var visited = (LockStatementSyntax)base.VisitLockStatement(node);

        return EnsureBlankLineBefore(node, node.LockKeyword, visited);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitYieldStatement(YieldStatementSyntax node)
    {
        var visited = (YieldStatementSyntax)base.VisitYieldStatement(node);

        if (node.IsKind(SyntaxKind.YieldReturnStatement) == false)
        {
            return visited;
        }

        if (IsPreviousStatementYield(node))
        {
            return visited;
        }

        return EnsureBlankLineBefore(node, node.YieldKeyword, visited);
    }

    #endregion // FormattingRuleBase

    #region Methods

    /// <summary>
    /// Gets the token immediately preceding the given keyword token.
    /// </summary>
    /// <param name="keyword">The keyword token.</param>
    /// <returns>The previous token.</returns>
    private static SyntaxToken GetPreviousToken(SyntaxToken keyword)
    {
        return keyword.GetPreviousToken();
    }

    /// <summary>
    /// Determines whether the previous token indicates that the statement is the
    /// first in a block (i.e., preceded by <c>{</c>, <c>:</c>, or is the start of the file).
    /// </summary>
    /// <param name="previousToken">The token preceding the statement's keyword.</param>
    /// <returns><c>true</c> if the statement is the first in a block; otherwise, <c>false</c>.</returns>
    private static bool IsFirstInBlock(SyntaxToken previousToken)
    {
        return previousToken.IsKind(SyntaxKind.OpenBraceToken)
               || previousToken.IsKind(SyntaxKind.ColonToken)
               || previousToken.IsKind(SyntaxKind.None);
    }

    /// <summary>
    /// Combines two trivia lists into a single enumerable sequence.
    /// </summary>
    /// <param name="trailing">The trailing trivia of the previous token.</param>
    /// <param name="leading">The leading trivia of the current node.</param>
    /// <returns>The combined trivia sequence.</returns>
    private static IEnumerable<SyntaxTrivia> CombineTrivia(SyntaxTriviaList trailing, SyntaxTriviaList leading)
    {
        foreach (var trivia in trailing)
        {
            yield return trivia;
        }

        foreach (var trivia in leading)
        {
            yield return trivia;
        }
    }

    /// <summary>
    /// Determines whether the combined trivia contains a blank line
    /// (two or more end-of-line trivia entries).
    /// </summary>
    /// <param name="trivia">The trivia sequence to examine.</param>
    /// <returns><c>true</c> if a blank line exists; otherwise, <c>false</c>.</returns>
    private static bool HasBlankLine(IEnumerable<SyntaxTrivia> trivia)
    {
        var endOfLineCount = 0;

        foreach (var triviaEntry in trivia)
        {
            if (triviaEntry.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                endOfLineCount++;

                if (endOfLineCount >= 2)
                {
                    return true;
                }
            }
            else if (triviaEntry.IsKind(SyntaxKind.WhitespaceTrivia) == false)
            {
                endOfLineCount = 0;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether the given break statement is a direct child of a switch section.
    /// </summary>
    /// <param name="node">The break statement to check.</param>
    /// <returns><c>true</c> if the break is inside a switch section; otherwise, <c>false</c>.</returns>
    private static bool IsInsideSwitchSection(BreakStatementSyntax node)
    {
        return node.Parent is SwitchSectionSyntax;
    }

    /// <summary>
    /// Determines whether the statement immediately preceding the given yield statement
    /// in the parent block is also a <see cref="YieldStatementSyntax"/>.
    /// </summary>
    /// <param name="node">The yield statement to check.</param>
    /// <returns><c>true</c> if the previous statement is a yield statement; otherwise, <c>false</c>.</returns>
    private static bool IsPreviousStatementYield(YieldStatementSyntax node)
    {
        if (node.Parent is BlockSyntax block)
        {
            var index = block.Statements.IndexOf(node);

            if (index > 0 && block.Statements[index - 1] is YieldStatementSyntax)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Ensures that a blank line exists before the visited node by examining the
    /// original node's context in the unmodified tree.
    /// </summary>
    /// <param name="originalNode">The original node in the unmodified tree (used for context checks).</param>
    /// <param name="keyword">The keyword token of the original node.</param>
    /// <param name="visitedNode">The visited node (potentially modified by child rewrites).</param>
    /// <returns>The visited node, potentially with an inserted blank line.</returns>
    private SyntaxNode EnsureBlankLineBefore(SyntaxNode originalNode, SyntaxToken keyword, SyntaxNode visitedNode)
    {
        var previousToken = GetPreviousToken(keyword);

        if (IsFirstInBlock(previousToken))
        {
            return visitedNode;
        }

        var trivia = CombineTrivia(previousToken.TrailingTrivia, originalNode.GetLeadingTrivia());

        if (HasBlankLine(trivia))
        {
            return visitedNode;
        }

        var leadingTrivia = visitedNode.GetLeadingTrivia();
        var newLeadingTrivia = leadingTrivia.Insert(0, SyntaxFactory.EndOfLine(Context.EndOfLine));

        return visitedNode.WithLeadingTrivia(newLeadingTrivia);
    }

    #endregion // Methods
}