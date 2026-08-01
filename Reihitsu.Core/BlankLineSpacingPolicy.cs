using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Core;

/// <summary>
/// Shared policy for statement shapes that are exempt from otherwise applicable blank-line rules
/// </summary>
public static class BlankLineSpacingPolicy
{
    #region Methods

    /// <summary>
    /// Determines whether a break statement belongs directly to a switch section. A break inside a block owned by
    /// the section is not exempt because the block owns that statement list
    /// </summary>
    /// <param name="statement">Statement to inspect</param>
    /// <returns><see langword="true"/> if the statement is a break owned directly by a switch section</returns>
    public static bool IsDirectSwitchSectionBreak(StatementSyntax statement)
    {
        return statement is BreakStatementSyntax { Parent: SwitchSectionSyntax };
    }

    #endregion // Methods
}