using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.Indentation.Contributors;

/// <summary>
/// Indents constructor initializers (<c>: base()</c> / <c>: this()</c>)
/// +1 level from the constructor declaration.
/// </summary>
internal sealed class ConstructorInitializerContributor : ILayoutContributor
{
    #region ILayoutContributor

    /// <inheritdoc/>
    public void Contribute(SyntaxNode node, FormattingScope scope, LayoutModel model, FormattingContext context)
    {
        if (node is not ConstructorInitializerSyntax initializer)
        {
            return;
        }

        if (initializer.Parent is not ConstructorDeclarationSyntax constructor)
        {
            return;
        }

        var constructorFirstToken = constructor.GetFirstToken();
        var constructorLine = LayoutComputer.GetLine(constructorFirstToken);

        if (model.TryGetLayout(constructorLine, out var constructorLayout) == false)
        {
            return;
        }

        var colonColumn = constructorLayout.Column + FormattingContext.IndentSize;

        LayoutComputer.SetIfFirstOnLine(initializer.ColonToken, colonColumn, "ConstructorInitializer", model);
    }

    #endregion // ILayoutContributor
}