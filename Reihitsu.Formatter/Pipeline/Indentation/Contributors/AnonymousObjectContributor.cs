using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.Indentation.Contributors;

/// <summary>
/// Aligns anonymous object braces to the <c>new</c> keyword column and members at +1 level
/// </summary>
internal sealed class AnonymousObjectContributor : ILayoutContributor
{
    #region ILayoutContributor

    /// <inheritdoc/>
    public void Contribute(SyntaxNode node, FormattingScope scope, LayoutModel model, FormattingContext context)
    {
        if (node is not AnonymousObjectCreationExpressionSyntax anon)
        {
            return;
        }

        var newColumn = LayoutComputer.GetAdjustedColumn(anon.NewKeyword, model);

        LayoutComputer.SetIfFirstOnLine(anon.OpenBraceToken, newColumn, "AnonymousObject", model);
        LayoutComputer.SetIfFirstOnLine(anon.CloseBraceToken, newColumn, "AnonymousObject", model);

        var memberColumn = newColumn + FormattingContext.IndentSize;

        foreach (var initializer in anon.Initializers)
        {
            var firstToken = initializer.GetFirstToken();

            LayoutComputer.SetIfFirstOnLine(firstToken, memberColumn, "AnonymousObject", model);
        }
    }

    #endregion // ILayoutContributor
}