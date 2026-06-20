using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.Indentation.Contributors;

/// <summary>
/// Indents a wrapped <c>when</c> guard clause +4 spaces from the <c>case</c> keyword of its label.
/// Only applies when the guard sits on its own line; an inline guard keeps its single-space layout
/// </summary>
internal sealed class CaseWhenClauseContributor : ILayoutContributor
{
    #region ILayoutContributor

    /// <inheritdoc/>
    public void Contribute(SyntaxNode node, LayoutModel model, FormattingContext context)
    {
        if (node is not CasePatternSwitchLabelSyntax label || label.WhenClause == null)
        {
            return;
        }

        var caseLine = LayoutComputer.GetLine(label.Keyword);

        if (model.TryGetLayout(caseLine, out var caseLayout) == false)
        {
            return;
        }

        var whenColumn = caseLayout.Column + FormattingContext.IndentSize;

        LayoutComputer.SetIfFirstOnLine(label.WhenClause.WhenKeyword, whenColumn, "CaseWhenClause", model);
    }

    #endregion // ILayoutContributor
}