using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Formatter.Data;

namespace Reihitsu.Formatter.Pipeline.TabReplacement;

/// <summary>
/// First phase of the pipeline. Replaces every tab in the whitespace of the code and between the tokens of a
/// directive with four spaces, before any other phase runs, so no later phase receives a tab in that whitespace and
/// every phase counts each of its characters as one column. Comments, documentation comments, literals, disabled
/// text, and a directive's free text use other trivia and token kinds and keep their tabs
/// </summary>
internal sealed class TabReplacementPhase : IFormattingPhase
{
    #region Methods

    /// <summary>
    /// Executes the tab replacement phase on the given syntax tree. Only the tokens and directives that hold a tab in
    /// their whitespace are rebuilt; without such a tab the input node itself is returned, so a node formatted inside
    /// its document keeps its place in the document's tree for the phases after this one
    /// </summary>
    /// <param name="root">The root syntax node to process</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The syntax node without tabs in the whitespace of the code and between directive tokens</returns>
    public static SyntaxNode Execute(SyntaxNode root, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var tokens = root.DescendantTokens()
                         .Where(HasWhitespaceTab)
                         .ToArray();
        var replaced = tokens.Length == 0
                           ? root
                           : root.ReplaceTokens(tokens, (_, rewritten) => ReplaceTabsAroundToken(rewritten, cancellationToken));

        return ReplaceDirectiveTriviaTabs(replaced, cancellationToken);
    }

    /// <summary>
    /// Determines whether the whitespace trivia in front of or behind a token holds a tab
    /// </summary>
    /// <param name="token">The token</param>
    /// <returns><see langword="true"/> if a whitespace trivia of the token holds a tab; otherwise, <see langword="false"/></returns>
    private static bool HasWhitespaceTab(SyntaxToken token)
    {
        return ContainsWhitespaceTab(token.LeadingTrivia)
               || ContainsWhitespaceTab(token.TrailingTrivia);
    }

    /// <summary>
    /// Determines whether a whitespace trivia of the list holds a tab
    /// </summary>
    /// <param name="triviaList">The trivia list to inspect</param>
    /// <returns><see langword="true"/> if a whitespace trivia of the list holds a tab; otherwise, <see langword="false"/></returns>
    private static bool ContainsWhitespaceTab(SyntaxTriviaList triviaList)
    {
        return triviaList.Any(static trivia => trivia.IsKind(SyntaxKind.WhitespaceTrivia)
                                               && trivia.ToFullString().Contains('\t'));
    }

    /// <summary>
    /// Replaces the tabs in the whitespace trivia in front of and behind a token
    /// </summary>
    /// <param name="token">The token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The token with tab-free whitespace trivia</returns>
    private static SyntaxToken ReplaceTabsAroundToken(SyntaxToken token, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return token.WithLeadingTrivia(ReplaceWhitespaceTabs(token.LeadingTrivia))
                    .WithTrailingTrivia(ReplaceWhitespaceTabs(token.TrailingTrivia));
    }

    /// <summary>
    /// Replaces each tab character within whitespace trivia with four spaces. Comment, disabled-text, and string
    /// content use distinct trivia and token kinds, never <see cref="SyntaxKind.WhitespaceTrivia"/>, so this only ever
    /// touches indentation and the gaps around tokens
    /// </summary>
    /// <param name="triviaList">The trivia list to process</param>
    /// <returns>The trivia list with tab characters in whitespace replaced by spaces</returns>
    private static SyntaxTriviaList ReplaceWhitespaceTabs(SyntaxTriviaList triviaList)
    {
        if (triviaList.Count == 0)
        {
            return triviaList;
        }

        var result = new List<SyntaxTrivia>(triviaList.Count);
        var changed = false;

        foreach (var triviaItem in triviaList)
        {
            if (triviaItem.IsKind(SyntaxKind.WhitespaceTrivia) && triviaItem.ToFullString().Contains('\t'))
            {
                result.Add(SyntaxFactory.Whitespace(triviaItem.ToFullString().Replace("\t", new string(' ', FormattingContext.IndentSize))));
                changed = true;
            }
            else
            {
                result.Add(triviaItem);
            }
        }

        return changed
                   ? SyntaxFactory.TriviaList(result)
                   : triviaList;
    }

    /// <summary>
    /// Replaces the tabs within a preprocessor directive's own interior whitespace with spaces. Directive trivia is
    /// structured, so the token-level pass above never visits the tokens inside it; this walks each directive's own
    /// token stream and rebuilds the trivia entry when it changes
    /// </summary>
    /// <param name="root">The syntax node to process</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The syntax node with directive-interior tabs replaced by spaces</returns>
    private static SyntaxNode ReplaceDirectiveTriviaTabs(SyntaxNode root, CancellationToken cancellationToken)
    {
        var directiveTrivia = root.DescendantTrivia()
                                  .Where(static trivia => trivia.HasStructure
                                                          && trivia.GetStructure() is DirectiveTriviaSyntax directive
                                                          && directive.DescendantTokens().Any(HasWhitespaceTab))
                                  .ToArray();

        if (directiveTrivia.Length == 0)
        {
            return root;
        }

        return root.ReplaceTrivia(directiveTrivia, (original, _) => ReplaceDirectiveTabs(original, cancellationToken));
    }

    /// <summary>
    /// Rebuilds a single directive trivia entry with the tabs in its interior whitespace replaced by spaces
    /// </summary>
    /// <param name="trivia">The directive trivia to process</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The directive trivia without tabs between its tokens</returns>
    private static SyntaxTrivia ReplaceDirectiveTabs(SyntaxTrivia trivia, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var structure = (DirectiveTriviaSyntax)trivia.GetStructure();
        var rewrittenStructure = structure.ReplaceTokens(structure.DescendantTokens(),
                                                         (_, rewritten) => rewritten.WithLeadingTrivia(ReplaceWhitespaceTabs(rewritten.LeadingTrivia))
                                                                                    .WithTrailingTrivia(ReplaceWhitespaceTabs(rewritten.TrailingTrivia)));

        return ReferenceEquals(rewrittenStructure, structure) ? trivia : SyntaxFactory.Trivia(rewrittenStructure);
    }

    #endregion // Methods

    #region IFormattingPhase

    /// <inheritdoc/>
    /// <remarks>
    /// The <paramref name="context"/> is part of the uniform phase contract and is not used by this phase
    /// </remarks>
    public SyntaxNode Execute(SyntaxNode root, FormattingContext context, CancellationToken cancellationToken)
    {
        return Execute(root, cancellationToken);
    }

    #endregion // IFormattingPhase
}