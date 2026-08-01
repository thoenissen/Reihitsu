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

    #endregion // Methods
}