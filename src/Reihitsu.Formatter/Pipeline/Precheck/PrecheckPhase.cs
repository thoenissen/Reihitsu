using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;
using Reihitsu.Formatter.Data;

namespace Reihitsu.Formatter.Pipeline.Precheck;

/// <summary>
/// The first phase: refuses an input that holds a shape <c>sdd/specs/formatter/unsupported-syntax.md</c> declares unsupported,
/// decided on exactly the node the phases receive and from that node alone, and changes nothing (FMT-UNSUP-001 to
/// FMT-UNSUP-003). <see cref="FindUnsupportedSyntax"/> is the one statement of the unsupported shapes; the phase and
/// <see cref="ReihitsuFormatter.FindUnsupportedSyntax"/> both ask it (FMT-UNSUP-007)
/// </summary>
internal sealed class PrecheckPhase : IFormattingPhase
{
    #region Constants

    /// <summary>
    /// The reason given for a comment inside a variable declaration (FMT-UNSUP-004)
    /// </summary>
    internal const string VariableDeclarationCommentReason = "a comment between a variable declaration's own tokens";

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Finds the unsupported shape that starts first in the text of a node, deciding from the tokens and trivia inside the node
    /// alone (FMT-UNSUP-001, FMT-UNSUP-003)
    /// </summary>
    /// <param name="node">The node</param>
    /// <returns>The shape with its location in the node's syntax tree, or <see langword="null"/> when the node holds none</returns>
    internal static UnsupportedSyntax FindUnsupportedSyntax(SyntaxNode node)
    {
        SyntaxTrivia? firstComment = null;

        foreach (var declaration in node.DescendantNodesAndSelf().OfType<VariableDeclarationSyntax>())
        {
            var comment = FindCommentInDeclaration(declaration, declaration == node);

            if (comment != null
                && (firstComment == null || comment.Value.FullSpan.Start < firstComment.Value.FullSpan.Start))
            {
                firstComment = comment;
            }
        }

        return firstComment == null
                   ? null
                   : new UnsupportedSyntax(VariableDeclarationCommentReason, Location.Create(node.SyntaxTree, firstComment.Value.FullSpan));
    }

    /// <summary>
    /// Finds the first comment in the span of a variable declaration outside the places FMT-UNSUP-004 excepts: the leading
    /// trivia of the span's first token, the trailing trivia of the <c>;</c> or <c>)</c> that ends it, and the inside of an
    /// initializer value. A declaration inside a value is a declaration of its own and is not looked at here
    /// </summary>
    /// <param name="declaration">The variable declaration</param>
    /// <param name="isInputRoot">Whether the declaration is the input itself, so that the statement or member around it is not part of the input</param>
    /// <returns>The first such comment, or <see langword="null"/></returns>
    private static SyntaxTrivia? FindCommentInDeclaration(VariableDeclarationSyntax declaration, bool isInputRoot)
    {
        var (tokens, terminator) = GetSpanTokens(declaration, isInputRoot);

        var values = declaration.Variables
                                .Select(static variable => variable.Initializer?.Value)
                                .Where(static value => value != null)
                                .ToList();
        var isFirst = true;

        foreach (var token in tokens)
        {
            var value = values.Find(candidate => candidate.Span.Contains(token.Span));
            var checksLeadingTrivia = isFirst == false
                                      && (value == null || token == value.GetFirstToken());
            var checksTrailingTrivia = token != terminator
                                       && (value == null || token == value.GetLastToken());

            isFirst = false;

            var comment = (checksLeadingTrivia ? FindFirstComment(token.LeadingTrivia) : null)
                              ?? (checksTrailingTrivia ? FindFirstComment(token.TrailingTrivia) : null);

            if (comment != null)
            {
                return comment;
            }
        }

        return null;
    }

    /// <summary>
    /// Gets the tokens of a variable declaration's span, from its first token through its terminator (FMT-UNSUP-004)
    /// </summary>
    /// <param name="declaration">The variable declaration</param>
    /// <param name="isInputRoot">Whether the declaration is the input itself</param>
    /// <returns>The tokens in text order, and the terminator, or the default token when the span has none in the input</returns>
    private static (List<SyntaxToken> Tokens, SyntaxToken Terminator) GetSpanTokens(VariableDeclarationSyntax declaration, bool isInputRoot)
    {
        var tokens = new List<SyntaxToken>();
        SyntaxToken terminator = default;

        switch (isInputRoot ? null : declaration.Parent)
        {
            case BaseFieldDeclarationSyntax:
            case LocalDeclarationStatementSyntax:
                {
                    tokens.AddRange(declaration.Parent.DescendantTokens());
                    terminator = tokens[tokens.Count - 1];
                }
                break;

            case ForStatementSyntax forStatement:
                {
                    terminator = forStatement.FirstSemicolonToken;
                }
                break;

            case UsingStatementSyntax usingStatement:
                {
                    terminator = usingStatement.CloseParenToken;
                }
                break;

            case FixedStatementSyntax fixedStatement:
                {
                    terminator = fixedStatement.CloseParenToken;
                }
                break;
        }

        if (tokens.Count == 0)
        {
            tokens.AddRange(declaration.DescendantTokens());

            if (terminator != default)
            {
                tokens.Add(terminator);
            }
        }

        return (tokens, terminator);
    }

    /// <summary>
    /// Gets the first comment of a trivia list
    /// </summary>
    /// <param name="trivia">The trivia list</param>
    /// <returns>The first comment, or <see langword="null"/> when the list holds none</returns>
    private static SyntaxTrivia? FindFirstComment(SyntaxTriviaList trivia)
    {
        var comment = trivia.FirstOrDefault(SyntaxTriviaUtilities.IsCommentTrivia);

        return comment == default ? null : comment;
    }

    #endregion // Methods

    #region IFormattingPhase

    /// <inheritdoc/>
    /// <remarks>
    /// Returns its input unchanged, or throws <see cref="SyntaxNotSupportedException"/> for the first unsupported shape in it,
    /// which the pipeline turns into the unsupported outcome before any other phase runs (FMT-UNSUP-001)
    /// </remarks>
    public SyntaxNode Execute(SyntaxNode root, FormattingContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var unsupported = FindUnsupportedSyntax(root);

        if (unsupported != null)
        {
            throw new SyntaxNotSupportedException(unsupported);
        }

        return root;
    }

    #endregion // IFormattingPhase
}