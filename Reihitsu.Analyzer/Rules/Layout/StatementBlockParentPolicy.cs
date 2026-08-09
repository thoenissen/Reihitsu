using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// Defines the statement parents whose directly owned blocks are governed by RH5402 and RH5403
/// </summary>
internal static class StatementBlockParentPolicy
{
    #region Methods

    /// <summary>
    /// Determines whether a block is directly owned by a covered statement parent
    /// </summary>
    /// <param name="block">Block to inspect</param>
    /// <returns><see langword="true"/> when the parent is covered; otherwise, <see langword="false"/></returns>
    internal static bool IsCovered(BlockSyntax block)
    {
        return block.Parent is IfStatementSyntax
                            or ElseClauseSyntax
                            or WhileStatementSyntax
                            or ForStatementSyntax
                            or ForEachStatementSyntax
                            or ForEachVariableStatementSyntax
                            or UsingStatementSyntax
                            or LockStatementSyntax
                            or FixedStatementSyntax
                            or DoStatementSyntax;
    }

    #endregion // Methods
}