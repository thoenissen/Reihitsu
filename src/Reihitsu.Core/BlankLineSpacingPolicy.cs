using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Core;

/// <summary>
/// Shared policy for statement shapes that are exempt from otherwise applicable blank-line rules
/// </summary>
public static class BlankLineSpacingPolicy
{
    #region Methods

    /// <summary>
    /// Determines whether a break statement is the terminal statement owned directly by a switch section. A
    /// non-terminal direct break and a break inside a block owned by the section are not exempt
    /// </summary>
    /// <param name="statement">Statement to inspect</param>
    /// <returns><see langword="true"/> if the statement is the terminal break owned directly by a switch section</returns>
    public static bool IsTerminalDirectSwitchSectionBreak(StatementSyntax statement)
    {
        return statement is BreakStatementSyntax { Parent: SwitchSectionSyntax section }
               && section.Statements.Count > 0
               && section.Statements[section.Statements.Count - 1] == statement;
    }

    /// <summary>
    /// Determines whether a block is a block statement that follows another statement of the same block or switch section.
    /// The opening brace of such a block starts a statement and continues no header above it, so the blank lines above it
    /// are decided by the rules between statements (FMT-BLANK-017, RH5025-005). The first statement of a list and a block
    /// that stands in no statement list are not such a block
    /// </summary>
    /// <param name="block">Block to inspect</param>
    /// <returns><see langword="true"/> if the block follows another statement of its block or switch section</returns>
    public static bool IsBlockStatementAfterStatement(BlockSyntax block)
    {
        switch (block.Parent)
        {
            case BlockSyntax parentBlock:
                {
                    return parentBlock.Statements.IndexOf(block) > 0;
                }

            case SwitchSectionSyntax section:
                {
                    return section.Statements.IndexOf(block) > 0;
                }

            default:
                {
                    return false;
                }
        }
    }

    /// <summary>
    /// Determines whether a statement needs a blank line in front of it because the statement before it in the same list
    /// ends with a closing brace (FMT-BLANK-005, RH5030). The terminal <see langword="break"/> owned directly by a switch
    /// section is exempt
    /// </summary>
    /// <param name="previousLastTokenKind">The kind of the last token of the preceding statement</param>
    /// <param name="isTerminalDirectSwitchSectionBreak">Whether the statement is the terminal <see langword="break"/> directly inside its switch section</param>
    /// <returns><see langword="true"/> if the statement follows a closing brace and needs a blank line</returns>
    public static bool RequiresBlankLineBehindClosingBrace(SyntaxKind previousLastTokenKind, bool isTerminalDirectSwitchSectionBreak)
    {
        return previousLastTokenKind == SyntaxKind.CloseBraceToken
               && isTerminalDirectSwitchSectionBreak == false;
    }

    /// <summary>
    /// Determines whether a statement needs a blank line in front of it because of its own kind and the kind of the statement
    /// before it in the same list (FMT-BLANK-001 to FMT-BLANK-004)
    /// </summary>
    /// <param name="statement">The statement</param>
    /// <param name="previousKind">The kind of the preceding statement</param>
    /// <param name="isTerminalDirectSwitchSectionBreak">Whether the statement is the terminal <see langword="break"/> directly inside its switch section</param>
    /// <returns><see langword="true"/> if a blank line is required in front of the statement</returns>
    public static bool RequiresBlankLineForStatementKind(StatementSyntax statement, SyntaxKind previousKind, bool isTerminalDirectSwitchSectionBreak)
    {
        switch (statement)
        {
            case LocalDeclarationStatementSyntax:
                return previousKind != SyntaxKind.LocalDeclarationStatement;

            case TryStatementSyntax:
            case IfStatementSyntax:
            case WhileStatementSyntax:
            case DoStatementSyntax:
            case UsingStatementSyntax:
            case CommonForEachStatementSyntax:
            case ForStatementSyntax:
            case ReturnStatementSyntax:
            case GotoStatementSyntax:
            case ContinueStatementSyntax:
            case ThrowStatementSyntax:
            case SwitchStatementSyntax:
            case CheckedStatementSyntax:
            case FixedStatementSyntax:
            case LockStatementSyntax:
                return true;

            case BreakStatementSyntax:
                return isTerminalDirectSwitchSectionBreak == false;

            case YieldStatementSyntax:
                return previousKind is not (SyntaxKind.YieldReturnStatement or SyntaxKind.YieldBreakStatement);

            case ExpressionStatementSyntax expressionStatement:
                return previousKind == SyntaxKind.LocalDeclarationStatement
                       && expressionStatement.Expression is AssignmentExpressionSyntax == false;

            default:
                return false;
        }
    }

    /// <summary>
    /// Determines whether a statement rule asks for a blank line between a statement and the statement before it in the same
    /// list, either because of the statement's kind or because the statement before it ends with a closing brace. Directives
    /// and existing blank lines in the gap are not considered: the result is the rule, read from the syntax alone
    /// </summary>
    /// <param name="previousStatement">The preceding statement</param>
    /// <param name="statement">The statement</param>
    /// <returns><see langword="true"/> if a statement rule separates the two statements by a blank line</returns>
    public static bool RequiresBlankLineBetweenStatements(StatementSyntax previousStatement, StatementSyntax statement)
    {
        var isTerminalDirectSwitchSectionBreak = IsTerminalDirectSwitchSectionBreak(statement);

        return RequiresBlankLineBehindClosingBrace(previousStatement.GetLastToken().Kind(), isTerminalDirectSwitchSectionBreak)
               || RequiresBlankLineForStatementKind(statement, previousStatement.Kind(), isTerminalDirectSwitchSectionBreak);
    }

    /// <summary>
    /// Determines whether a blank line is required between a statement that ends with a closing brace and the
    /// statement that follows it in the same list. The rule itself is <see cref="RequiresBlankLineBehindClosingBrace"/>
    /// (the statement ends with a closing brace and the following statement is not a terminal break owned directly by a
    /// switch section); on top of it, no blank line is required when one already separates the two or when both share a line
    /// </summary>
    /// <param name="statement">Statement to inspect</param>
    /// <param name="nextStatement">Statement that follows <paramref name="statement"/> in the same list</param>
    /// <returns><see langword="true"/> if a blank line must be inserted after the closing brace of <paramref name="statement"/></returns>
    public static bool RequiresBlankLineAfterClosingBrace(StatementSyntax statement, StatementSyntax nextStatement)
    {
        var lastToken = statement.GetLastToken();

        if (RequiresBlankLineBehindClosingBrace(lastToken.Kind(), IsTerminalDirectSwitchSectionBreak(nextStatement)) == false)
        {
            return false;
        }

        var nextFirstToken = nextStatement.GetFirstToken();

        if (TokenGapAnalysis.Between(lastToken, nextFirstToken).BlankLineCount > 0)
        {
            return false;
        }

        var lastLine = lastToken.GetLocation().GetLineSpan().EndLinePosition.Line;
        var nextLine = nextFirstToken.GetLocation().GetLineSpan().StartLinePosition.Line;

        // Pairs on the same line (e.g. inline blocks like if (x) { } Consume();) are left alone
        return lastLine < nextLine;
    }

    #endregion // Methods
}