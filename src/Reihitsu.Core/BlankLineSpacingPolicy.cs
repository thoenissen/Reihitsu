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
    /// Determines whether a blank line is required between a statement that ends with a closing brace and the
    /// statement that follows it in the same list. No blank line is required when the statement does not end with a
    /// closing brace, when one already separates the two, when both share a line, or when the following statement is
    /// a terminal break owned directly by a switch section
    /// </summary>
    /// <param name="statement">Statement to inspect</param>
    /// <param name="nextStatement">Statement that follows <paramref name="statement"/> in the same list</param>
    /// <returns><see langword="true"/> if a blank line must be inserted after the closing brace of <paramref name="statement"/></returns>
    public static bool RequiresBlankLineAfterClosingBrace(StatementSyntax statement, StatementSyntax nextStatement)
    {
        var lastToken = statement.GetLastToken();

        if (lastToken.IsKind(SyntaxKind.CloseBraceToken) == false)
        {
            return false;
        }

        if (IsTerminalDirectSwitchSectionBreak(nextStatement))
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