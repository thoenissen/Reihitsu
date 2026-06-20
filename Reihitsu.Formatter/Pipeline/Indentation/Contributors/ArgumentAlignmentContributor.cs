using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.Indentation.Contributors;

/// <summary>
/// Aligns arguments, parameters, and attribute arguments to the column after the opening parenthesis
/// when the list spans multiple lines
/// </summary>
internal sealed class ArgumentAlignmentContributor : ILayoutContributor
{
    #region Methods

    /// <summary>
    /// Aligns items in a separated list to the column after the open token
    /// when the list spans multiple lines
    /// </summary>
    /// <typeparam name="T">The syntax node type of the list items</typeparam>
    /// <param name="openToken">The opening token (parenthesis or bracket)</param>
    /// <param name="closeToken">The closing token</param>
    /// <param name="items">The items in the list</param>
    /// <param name="model">The layout model to write to</param>
    private static void AlignToOpenToken<T>(SyntaxToken openToken, SyntaxToken closeToken, SeparatedSyntaxList<T> items, LayoutModel model)
        where T : SyntaxNode
    {
        if (items.Count == 0)
        {
            return;
        }

        var firstLine = LayoutComputer.GetLine(openToken);
        var lastLine = LayoutComputer.GetLine(closeToken);

        if (firstLine == lastLine)
        {
            return;
        }

        var alignColumn = LayoutComputer.GetAdjustedColumn(openToken, model) + 1;

        foreach (var item in items)
        {
            var firstToken = item.GetFirstToken();

            LayoutComputer.SetIfFirstOnLine(firstToken, alignColumn, "ArgumentAlignment", model);
        }

        LayoutComputer.SetIfFirstOnLine(closeToken, alignColumn, "ArgumentAlignment", model);
    }

    /// <summary>
    /// Aligns a multi-line dictionary indexer key as a block: the key body is indented
    /// one level deeper than the opening bracket and the closing bracket is aligned
    /// with the opening bracket
    /// </summary>
    /// <param name="openBracket">The opening bracket token</param>
    /// <param name="closeBracket">The closing bracket token</param>
    /// <param name="arguments">The bracketed arguments representing the indexer key</param>
    /// <param name="model">The layout model to write to</param>
    private static void AlignDictionaryKey(SyntaxToken openBracket, SyntaxToken closeBracket, SeparatedSyntaxList<ArgumentSyntax> arguments, LayoutModel model)
    {
        if (arguments.Count == 0)
        {
            return;
        }

        var firstLine = LayoutComputer.GetLine(openBracket);
        var lastLine = LayoutComputer.GetLine(closeBracket);

        if (firstLine == lastLine)
        {
            return;
        }

        var openColumn = LayoutComputer.GetAdjustedColumn(openBracket, model);
        var bodyColumn = openColumn + FormattingContext.IndentSize;

        foreach (var argument in arguments)
        {
            var firstToken = argument.GetFirstToken();

            LayoutComputer.SetIfFirstOnLine(firstToken, bodyColumn, "DictionaryIndexerKey", model);
        }

        LayoutComputer.SetIfFirstOnLine(closeBracket, openColumn, "DictionaryIndexerKey", model);
    }

    #endregion // Methods

    #region ILayoutContributor

    /// <inheritdoc/>
    public void Contribute(SyntaxNode node, LayoutModel model, FormattingContext context)
    {
        switch (node)
        {
            case ArgumentListSyntax argumentList:
                AlignToOpenToken(argumentList.OpenParenToken, argumentList.CloseParenToken, argumentList.Arguments, model);
                break;

            case BracketedArgumentListSyntax { Parent: ImplicitElementAccessSyntax } dictionaryKey:
                AlignDictionaryKey(dictionaryKey.OpenBracketToken, dictionaryKey.CloseBracketToken, dictionaryKey.Arguments, model);
                break;

            case BracketedArgumentListSyntax bracketedArgumentList:
                AlignToOpenToken(bracketedArgumentList.OpenBracketToken, bracketedArgumentList.CloseBracketToken, bracketedArgumentList.Arguments, model);
                break;

            case ParameterListSyntax parameterList:
                AlignToOpenToken(parameterList.OpenParenToken, parameterList.CloseParenToken, parameterList.Parameters, model);
                break;

            case BracketedParameterListSyntax bracketedParameterList:
                AlignToOpenToken(bracketedParameterList.OpenBracketToken, bracketedParameterList.CloseBracketToken, bracketedParameterList.Parameters, model);
                break;

            case AttributeArgumentListSyntax attributeArgumentList:
                AlignToOpenToken(attributeArgumentList.OpenParenToken, attributeArgumentList.CloseParenToken, attributeArgumentList.Arguments, model);
                break;

            case TupleExpressionSyntax tuple:
                AlignToOpenToken(tuple.OpenParenToken, tuple.CloseParenToken, tuple.Arguments, model);
                break;
        }
    }

    #endregion // ILayoutContributor
}