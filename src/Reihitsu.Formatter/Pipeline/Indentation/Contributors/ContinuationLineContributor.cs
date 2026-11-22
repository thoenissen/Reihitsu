using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;
using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.Core.Utilities;
using Reihitsu.Formatter.Pipeline.Indentation.Utilities;

namespace Reihitsu.Formatter.Pipeline.Indentation.Contributors;

/// <summary>
/// Indents a continuation line that no other requirement places four columns right of the column at which the line of the
/// construct it continues begins (FMT-INDENT-034). A continuation line starts with a token of a statement, a member
/// declaration, or a using directive that starts on an earlier line, and the token does not begin a part of it that block
/// indentation places by its scope. The construct it continues is the nearest node around that token whose first token stands
/// on an earlier line
/// </summary>
/// <remarks>
/// The contributor runs once per sweep, at the root, before every other contributor, and writes through
/// <see cref="LayoutModel.SetContinuation"/>: every other contributor that places the line takes it over. The column of the
/// construct's line is read where the phase puts it, so a continuation of a continuation is indented one more level, and the
/// sweeps repeat until the columns settle
/// </remarks>
internal sealed class ContinuationLineContributor : ILayoutContributor
{
    #region Methods

    /// <summary>
    /// Finds the construct a line continues when the line is a continuation line: the token that starts it stands inside a
    /// statement, a member declaration, or a using directive that starts on an earlier line, and it does not begin a part of
    /// it that block indentation places by its scope. The construct is the nearest node around the token whose first token
    /// stands on an earlier line, which is the nearest node the token does not begin, because the token starts its line
    /// </summary>
    /// <param name="token">The token that starts the line</param>
    /// <param name="containerCache">Whether a node or one of its ancestors is a statement, a member declaration, or a using directive, for the nodes already asked</param>
    /// <returns>The construct, or <see langword="null"/> when the line is no continuation line</returns>
    private static SyntaxNode FindContinuedConstruct(SyntaxToken token, Dictionary<SyntaxNode, bool> containerCache)
    {
        if (IsScopeToken(token))
        {
            return null;
        }

        var node = token.Parent;

        while (node != null
               && node.SpanStart == token.SpanStart)
        {
            if (IsScopePart(node)
                || IsFirstTokenBehindAttributeLists(node, token))
            {
                return null;
            }

            node = node.Parent;
        }

        if (node == null
            || IsFirstTokenBehindAttributeLists(node, token)
            || IsInsideContainer(node, containerCache) == false)
        {
            return null;
        }

        return node;
    }

    /// <summary>
    /// Determines whether a node is, or stands inside, a statement, a member declaration, or a using directive, or is a
    /// node-level root, which stands inside one in its document. The answer of every node on the way up is remembered, so the
    /// nodes of one root are walked once
    /// </summary>
    /// <param name="node">The node</param>
    /// <param name="containerCache">The answers already known</param>
    /// <returns><see langword="true"/> if the node is or stands inside such a container; otherwise, <see langword="false"/></returns>
    private static bool IsInsideContainer(SyntaxNode node, Dictionary<SyntaxNode, bool> containerCache)
    {
        var walked = new List<SyntaxNode>();
        var result = false;

        for (var current = node; current != null; current = current.Parent)
        {
            if (containerCache.TryGetValue(current, out result))
            {
                break;
            }

            walked.Add(current);

            // A node-level root stands inside a statement, a member declaration, or a using directive of its document,
            // which the root no longer reaches (FMT-PIPE-023)
            if (current is StatementSyntax or MemberDeclarationSyntax or UsingDirectiveSyntax
                || (current.Parent == null && current is not CompilationUnitSyntax))
            {
                result = true;

                break;
            }
        }

        foreach (var walkedNode in walked)
        {
            containerCache[walkedNode] = result;
        }

        return result;
    }

    /// <summary>
    /// Determines whether a token is a brace of a scope that block indentation places, or the <c>while</c> of a <c>do</c>
    /// statement
    /// </summary>
    /// <param name="token">The token</param>
    /// <returns><see langword="true"/> if the token is placed by its scope; otherwise, <see langword="false"/></returns>
    private static bool IsScopeToken(SyntaxToken token)
    {
        return token.Kind() switch
               {
                   SyntaxKind.OpenBraceToken or SyntaxKind.CloseBraceToken => token.Parent is BlockSyntax
                                                                                           or BaseTypeDeclarationSyntax
                                                                                           or BaseNamespaceDeclarationSyntax
                                                                                           or AccessorListSyntax
                                                                                           or SwitchStatementSyntax,
                   SyntaxKind.WhileKeyword => token.Parent is DoStatementSyntax,
                   _ => false
               };
    }

    /// <summary>
    /// Determines whether a node is a part that block indentation places by its scope when a line starts with its first
    /// token: a statement, a member declaration, an accessor, a switch section or label, an <c>else</c>, <c>catch</c>, or
    /// <c>finally</c> clause, a using directive, and an attribute list of a member, an accessor, or a local function
    /// </summary>
    /// <param name="node">The node</param>
    /// <returns><see langword="true"/> if the node is such a part; otherwise, <see langword="false"/></returns>
    private static bool IsScopePart(SyntaxNode node)
    {
        return node is StatementSyntax
                    or MemberDeclarationSyntax
                    or AccessorDeclarationSyntax
                    or SwitchSectionSyntax
                    or SwitchLabelSyntax
                    or ElseClauseSyntax
                    or CatchClauseSyntax
                    or FinallyClauseSyntax
                    or UsingDirectiveSyntax
                    or ExternAliasDirectiveSyntax
                    or AttributeListSyntax { Parent: MemberDeclarationSyntax or AccessorDeclarationSyntax or LocalFunctionStatementSyntax };
    }

    /// <summary>
    /// Determines whether a token is the first token behind the attribute lists of a member, an accessor, or a local function
    /// </summary>
    /// <param name="node">The node that may own the attribute lists</param>
    /// <param name="token">The token</param>
    /// <returns><see langword="true"/> if the token stands directly behind the node's last attribute list; otherwise, <see langword="false"/></returns>
    private static bool IsFirstTokenBehindAttributeLists(SyntaxNode node, SyntaxToken token)
    {
        var attributeLists = node switch
                             {
                                 MemberDeclarationSyntax member => member.AttributeLists,
                                 AccessorDeclarationSyntax accessor => accessor.AttributeLists,
                                 LocalFunctionStatementSyntax localFunction => localFunction.AttributeLists,
                                 _ => default
                             };

        return attributeLists.Count > 0
               && SyntaxTokenNavigationUtilities.GetNextToken(attributeLists[attributeLists.Count - 1].GetLastToken()) == token;
    }

    /// <summary>
    /// Gets the line on which the text that a line begins inside starts: a line that begins inside the text of a multi-line
    /// string literal or of a multi-line comment is measured from the line on which that text starts
    /// </summary>
    /// <param name="root">The root</param>
    /// <param name="line">The line</param>
    /// <returns>The line on which the text the line begins with starts</returns>
    private static int GetLineOfText(SyntaxNode root, int line)
    {
        var lineStartIndex = LineStartIndex.For(root.SyntaxTree);
        var text = root.SyntaxTree.GetText();

        while (lineStartIndex.TryGetLineStartColumn(line, out _) == false)
        {
            var position = text.Lines[line].Start;
            var token = root.FindToken(position);
            var textStart = token.Span.Contains(position)
                                ? token.SpanStart
                                : root.FindTrivia(position).SpanStart;
            var textLine = text.Lines.GetLineFromPosition(textStart).LineNumber;

            if (textLine >= line)
            {
                break;
            }

            line = textLine;
        }

        return line;
    }

    /// <summary>
    /// Gets the column at which a line begins in the output of the indentation phase. The line on which a node-level root's
    /// first token stands begins at the line-start column the entry point passes, where it passes one (FMT-PIPE-024)
    /// </summary>
    /// <param name="root">The root</param>
    /// <param name="line">The line</param>
    /// <param name="model">The layout model</param>
    /// <param name="context">The formatting context</param>
    /// <param name="column">The column at which the line begins</param>
    /// <returns><see langword="true"/> if the line has a column; otherwise, <see langword="false"/></returns>
    private static bool TryGetLineColumn(SyntaxNode root, int line, LayoutModel model, FormattingContext context, out int column)
    {
        if (context.RootLineStartColumn is { } rootLineStartColumn
            && line == LayoutComputer.GetLine(root.GetFirstToken()))
        {
            column = rootLineStartColumn;

            return true;
        }

        return model.TryGetPlacedColumn(line, out column);
    }

    #endregion // Methods

    #region ILayoutContributor

    /// <inheritdoc/>
    public void Contribute(SyntaxNode node, LayoutModel model, FormattingContext context)
    {
        if (node.Parent != null)
        {
            return;
        }

        var containerCache = new Dictionary<SyntaxNode, bool>();
        var lineStartIndex = LineStartIndex.For(node.SyntaxTree);

        foreach (var token in node.DescendantTokens())
        {
            if (token.Span.IsEmpty
                || LayoutComputer.IsFirstOnLine(token) == false)
            {
                continue;
            }

            var construct = FindContinuedConstruct(token, containerCache);

            if (construct == null)
            {
                continue;
            }

            var constructLine = GetLineOfText(node, lineStartIndex.GetLine(construct.SpanStart));

            if (TryGetLineColumn(node, constructLine, model, context, out var column))
            {
                model.SetContinuation(LayoutComputer.GetLine(token), new TokenLayout(column + FormattingContext.IndentSize, "Continuation"));
            }
        }
    }

    #endregion // ILayoutContributor
}