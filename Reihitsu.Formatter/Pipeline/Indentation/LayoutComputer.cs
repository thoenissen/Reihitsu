using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Formatter.Pipeline.Indentation.Contributors;

namespace Reihitsu.Formatter.Pipeline.Indentation;

/// <summary>
/// Computes the indentation layout model for a syntax tree.
/// Uses a three-pass approach: block indentation, alignment overrides, and comment alignment.
/// </summary>
internal static class LayoutComputer
{
    #region Methods

    /// <summary>
    /// Computes the layout model for all first-on-line tokens in the syntax tree.
    /// </summary>
    /// <param name="root">The root syntax node.</param>
    /// <param name="context">The formatting context.</param>
    /// <returns>A layout model mapping line numbers to desired indentation.</returns>
    public static LayoutModel Compute(SyntaxNode root, FormattingContext context)
    {
        var model = new LayoutModel();
        var baseColumn = context.BaseIndentLevel * FormattingContext.IndentSize;
        var rootScope = new FormattingScope(baseColumn);

        // Pass 1: Block indentation — recursive descent over the tree
        ComputeBlockIndentation(root, 0, model, baseColumn);

        // Pass 2: Alignment contributors — override block indentation for specific constructs
        var contributors = CreateContributors();

        foreach (var node in root.DescendantNodesAndSelf())
        {
            foreach (var contributor in contributors)
            {
                contributor.Contribute(node, rootScope, model, context);
            }
        }

        // Pass 3: Comment alignment — align comments to the code they precede
        var commentContributor = new CommentIndentationContributor();
        commentContributor.Contribute(root, rootScope, model, context);

        return model;
    }

    #endregion // Methods

    #region Private methods

    /// <summary>
    /// Creates the ordered list of alignment contributors.
    /// Later contributors override earlier ones for the same line.
    /// </summary>
    /// <returns>An array of layout contributors in priority order.</returns>
    private static ILayoutContributor[] CreateContributors()
    {
        return [
                   new ArgumentAlignmentContributor(),
                   new MethodChainAlignmentContributor(),
                   new ObjectInitializerContributor(),
                   new CollectionExpressionContributor(),
                   new BinaryExpressionContributor(),
                   new ConditionalExpressionContributor(),
                   new SwitchExpressionContributor(),
                   new ConstructorInitializerContributor(),
                   new GenericConstraintContributor(),
                   new BaseTypeListContributor(),
                   new AnonymousObjectContributor(),
                   new LambdaAlignmentContributor()
               ];
    }

    /// <summary>
    /// Pass 1: Walks the syntax tree top-down and sets block indentation
    /// for all first-on-line tokens based on nesting depth.
    /// </summary>
    /// <param name="node">The current syntax node.</param>
    /// <param name="indentLevel">The current block indentation level.</param>
    /// <param name="model">The layout model to write to.</param>
    /// <param name="baseColumn">The base column offset.</param>
    private static void ComputeBlockIndentation(SyntaxNode node, int indentLevel, LayoutModel model, int baseColumn)
    {
        var braceRange = GetIndentingBraceRange(node);
        var isSwitchSection = node is SwitchSectionSyntax;

        foreach (var child in node.ChildNodesAndTokens())
        {
            var childIndent = indentLevel;

            if (braceRange != null)
            {
                var (openEnd, closeStart) = braceRange.Value;

                if (child.SpanStart >= openEnd && child.SpanStart < closeStart)
                {
                    childIndent = indentLevel + 1;
                }
            }

            if (isSwitchSection && child.IsNode && child.AsNode() is StatementSyntax)
            {
                childIndent = indentLevel + 1;
            }

            if (child.IsToken)
            {
                var token = child.AsToken();

                foreach (var trivia in token.LeadingTrivia)
                {
                    if (trivia.IsKind(SyntaxKind.RegionDirectiveTrivia)
                        || trivia.IsKind(SyntaxKind.EndRegionDirectiveTrivia))
                    {
                        var directiveIndent = indentLevel;

                        if (braceRange != null)
                        {
                            var (openEnd, closeStart) = braceRange.Value;

                            if (trivia.SpanStart >= openEnd && trivia.SpanStart < closeStart)
                            {
                                directiveIndent = indentLevel + 1;
                            }
                        }

                        var directiveLine = trivia.GetLocation().GetLineSpan().StartLinePosition.Line;

                        model.Set(directiveLine, new TokenLayout(directiveIndent * FormattingContext.IndentSize + baseColumn, "Directive"));
                    }
                }

                if (IsFirstOnLine(token))
                {
                    var line = token.GetLocation().GetLineSpan().StartLinePosition.Line;

                    model.Set(line, new TokenLayout(childIndent * FormattingContext.IndentSize + baseColumn, "Block"));
                }
            }
            else
            {
                ComputeBlockIndentation(child.AsNode(), childIndent, model, baseColumn);
            }
        }
    }

    /// <summary>
    /// Returns the span range between open and close braces for indenting constructs,
    /// or <see langword="null"/> if the node is not an indenting brace construct.
    /// </summary>
    /// <param name="node">The syntax node to check.</param>
    /// <returns>The open brace end and close brace start positions, or null.</returns>
    private static (int OpenEnd, int CloseStart)? GetIndentingBraceRange(SyntaxNode node)
    {
        SyntaxToken openBrace;
        SyntaxToken closeBrace;

        switch (node)
        {
            case NamespaceDeclarationSyntax ns:
                {
                    openBrace = ns.OpenBraceToken;
                    closeBrace = ns.CloseBraceToken;
                }
                break;

            case BaseTypeDeclarationSyntax typeDecl:
                {
                    openBrace = typeDecl.OpenBraceToken;
                    closeBrace = typeDecl.CloseBraceToken;
                }
                break;

            case BlockSyntax block:
                {
                    openBrace = block.OpenBraceToken;
                    closeBrace = block.CloseBraceToken;
                }
                break;

            case SwitchStatementSyntax switchStmt:
                {
                    openBrace = switchStmt.OpenBraceToken;
                    closeBrace = switchStmt.CloseBraceToken;
                }
                break;

            case AccessorListSyntax accessorList:
                {
                    openBrace = accessorList.OpenBraceToken;
                    closeBrace = accessorList.CloseBraceToken;
                }
                break;

            default:
                {
                    return null;
                }
        }

        if (openBrace.IsMissing || closeBrace.IsMissing)
        {
            return null;
        }

        return (openBrace.Span.End, closeBrace.SpanStart);
    }

    /// <summary>
    /// Determines whether a token is the first token on its line.
    /// </summary>
    /// <param name="token">The token to check.</param>
    /// <returns><see langword="true"/> if the token is first on its line; otherwise, <see langword="false"/>.</returns>
    internal static bool IsFirstOnLine(SyntaxToken token)
    {
        if (token.IsKind(SyntaxKind.None))
        {
            return false;
        }

        var previousToken = token.GetPreviousToken();

        if (previousToken == default || previousToken.IsKind(SyntaxKind.None))
        {
            return true;
        }

        foreach (var trivia in token.LeadingTrivia)
        {
            if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                return true;
            }
        }

        foreach (var trivia in previousToken.TrailingTrivia)
        {
            if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Gets the 0-based line number of a token.
    /// </summary>
    /// <param name="token">The token.</param>
    /// <returns>The line number.</returns>
    internal static int GetLine(SyntaxToken token)
    {
        return token.GetLocation().GetLineSpan().StartLinePosition.Line;
    }

    /// <summary>
    /// Gets the 0-based column of a token.
    /// </summary>
    /// <param name="token">The token.</param>
    /// <returns>The column.</returns>
    internal static int GetColumn(SyntaxToken token)
    {
        return token.GetLocation().GetLineSpan().StartLinePosition.Character;
    }

    /// <summary>
    /// Gets the column of a token as it will be after indentation is applied.
    /// For tokens that are first on their line, this returns the layout model's column.
    /// For tokens that are not first on their line, this computes the offset from the
    /// first token's adjusted position.
    /// </summary>
    /// <param name="token">The token to get the adjusted column for.</param>
    /// <param name="model">The layout model from Pass 1.</param>
    /// <returns>The adjusted column position.</returns>
    internal static int GetAdjustedColumn(SyntaxToken token, LayoutModel model)
    {
        var originalColumn = GetColumn(token);
        var line = GetLine(token);

        if (model.TryGetLayout(line, out var layout) == false)
        {
            return originalColumn;
        }

        var firstTokenOnLine = FindFirstTokenOnLine(token);
        var originalLineStart = GetColumn(firstTokenOnLine);

        return layout.Column + (originalColumn - originalLineStart);
    }

    /// <summary>
    /// Finds the first token on the same line as the given token.
    /// </summary>
    /// <param name="token">The token to find the first token on the same line for.</param>
    /// <returns>The first token on the same line.</returns>
    private static SyntaxToken FindFirstTokenOnLine(SyntaxToken token)
    {
        var targetLine = GetLine(token);
        var current = token;

        while (true)
        {
            var prev = current.GetPreviousToken();

            if (prev == default || prev.IsKind(SyntaxKind.None) || GetLine(prev) != targetLine)
            {
                return current;
            }

            current = prev;
        }
    }

    /// <summary>
    /// Sets layout for a token if it is first on its line.
    /// </summary>
    /// <param name="token">The token.</param>
    /// <param name="column">The desired column.</param>
    /// <param name="source">Debug label for the contributor.</param>
    /// <param name="model">The layout model.</param>
    internal static void SetIfFirstOnLine(SyntaxToken token, int column, string source, LayoutModel model)
    {
        if (IsFirstOnLine(token))
        {
            model.Set(GetLine(token), new TokenLayout(column, source));
        }
    }

    #endregion // Private methods
}