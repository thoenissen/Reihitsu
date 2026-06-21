using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Rules.Layout;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Code fix provider for <see cref="RH5113DeclarationSemicolonMustStayOnDeclarationLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5113DeclarationSemicolonMustStayOnDeclarationLineCodeFixProvider))]
public class RH5113DeclarationSemicolonMustStayOnDeclarationLineCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applies the code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="diagnosticSpan">Diagnostic span</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, TextSpan diagnosticSpan, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        var diagnosticNode = root.FindNode(diagnosticSpan, getInnermostNodeForTie: true);
        var declarationNode = GetDeclarationNode(diagnosticNode);
        var updatedNode = declarationNode == null ? null : FixDeclarationNode(declarationNode);

        return declarationNode == null || updatedNode == null
                   ? document
                   : document.WithSyntaxRoot(root.ReplaceNode(declarationNode, updatedNode));
    }

    /// <summary>
    /// Gets the declaration node that owns the stray semicolon
    /// </summary>
    /// <param name="diagnosticNode">Diagnostic node</param>
    /// <returns>Declaration node, or <see langword="null"/> when no supported declaration is found</returns>
    private static SyntaxNode GetDeclarationNode(SyntaxNode diagnosticNode)
    {
        return (diagnosticNode.FirstAncestorOrSelf<FieldDeclarationSyntax>()
                    ?? (SyntaxNode)diagnosticNode.FirstAncestorOrSelf<EventFieldDeclarationSyntax>())
                   ?? diagnosticNode.FirstAncestorOrSelf<DelegateDeclarationSyntax>();
    }

    /// <summary>
    /// Determines whether the trivia between the previous token and the semicolon contains a comment or directive.
    /// A line join across a comment would move code into the comment, so the fix is not offered in that case
    /// </summary>
    /// <param name="previousToken">Previous token</param>
    /// <param name="semicolonToken">Semicolon token</param>
    /// <returns><see langword="true"/> if comments or directives are present; otherwise, <see langword="false"/></returns>
    private static bool HasCommentsOrDirectives(SyntaxToken previousToken, SyntaxToken semicolonToken)
    {
        return ContainsCommentOrDirective(previousToken.TrailingTrivia)
               || ContainsCommentOrDirective(semicolonToken.LeadingTrivia);
    }

    /// <summary>
    /// Determines whether the trivia list contains a comment or directive
    /// </summary>
    /// <param name="trivia">Trivia list</param>
    /// <returns><see langword="true"/> if comments or directives are present; otherwise, <see langword="false"/></returns>
    private static bool ContainsCommentOrDirective(SyntaxTriviaList trivia)
    {
        foreach (var item in trivia)
        {
            if (item.IsDirective
                || item.IsKind(SyntaxKind.SingleLineCommentTrivia)
                || item.IsKind(SyntaxKind.MultiLineCommentTrivia)
                || item.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)
                || item.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Fixes the reported declaration node without formatting the surrounding scope
    /// </summary>
    /// <param name="declarationNode">Declaration node</param>
    /// <returns>Updated declaration node</returns>
    private static SyntaxNode FixDeclarationNode(SyntaxNode declarationNode)
    {
        return declarationNode switch
               {
                   FieldDeclarationSyntax field => CollapseSemicolonToPreviousLine(field, field.SemicolonToken),
                   EventFieldDeclarationSyntax eventField => CollapseSemicolonToPreviousLine(eventField, eventField.SemicolonToken),
                   DelegateDeclarationSyntax delegateDeclaration => CollapseSemicolonToPreviousLine(delegateDeclaration, delegateDeclaration.SemicolonToken),
                   _ => declarationNode,
               };
    }

    /// <summary>
    /// Moves the semicolon onto the same line as the previous token
    /// </summary>
    /// <typeparam name="TNode">Node type</typeparam>
    /// <param name="node">Node to update</param>
    /// <param name="semicolonToken">Semicolon token</param>
    /// <returns>Updated node</returns>
    private static TNode CollapseSemicolonToPreviousLine<TNode>(TNode node, SyntaxToken semicolonToken)
        where TNode : SyntaxNode
    {
        if (semicolonToken.IsMissing)
        {
            return node;
        }

        var previousToken = semicolonToken.GetPreviousToken();

        if (previousToken.IsKind(SyntaxKind.None))
        {
            return node;
        }

        if (ContainsEndOfLine(previousToken.TrailingTrivia) == false
            && ContainsEndOfLine(semicolonToken.LeadingTrivia) == false)
        {
            return node;
        }

        var newPreviousToken = previousToken.WithTrailingTrivia(RemoveTrailingWhitespaceAndLineBreaks(previousToken.TrailingTrivia));
        var newSemicolonToken = semicolonToken.WithLeadingTrivia(RemoveLeadingWhitespaceAndLineBreaks(semicolonToken.LeadingTrivia));

        return node.ReplaceTokens([previousToken, semicolonToken],
                                  (original, _) =>
                                  {
                                      if (original == previousToken)
                                      {
                                          return newPreviousToken;
                                      }

                                      return newSemicolonToken;
                                  });
    }

    /// <summary>
    /// Removes leading whitespace and line breaks from trivia
    /// </summary>
    /// <param name="trivia">Trivia list</param>
    /// <returns>Trimmed trivia list</returns>
    private static SyntaxTriviaList RemoveLeadingWhitespaceAndLineBreaks(SyntaxTriviaList trivia)
    {
        var trimmedTrivia = new List<SyntaxTrivia>(trivia);

        while (trimmedTrivia.Count > 0
               && (trimmedTrivia[0].IsKind(SyntaxKind.WhitespaceTrivia) || trimmedTrivia[0].IsKind(SyntaxKind.EndOfLineTrivia)))
        {
            trimmedTrivia.RemoveAt(0);
        }

        return SyntaxFactory.TriviaList(trimmedTrivia);
    }

    /// <summary>
    /// Removes trailing whitespace and line breaks from trivia
    /// </summary>
    /// <param name="trivia">Trivia list</param>
    /// <returns>Trimmed trivia list</returns>
    private static SyntaxTriviaList RemoveTrailingWhitespaceAndLineBreaks(SyntaxTriviaList trivia)
    {
        var trimmedTrivia = new List<SyntaxTrivia>(trivia);

        while (trimmedTrivia.Count > 0)
        {
            var lastTrivia = trimmedTrivia[trimmedTrivia.Count - 1];

            if (lastTrivia.IsKind(SyntaxKind.WhitespaceTrivia) == false
                && lastTrivia.IsKind(SyntaxKind.EndOfLineTrivia) == false)
            {
                break;
            }

            trimmedTrivia.RemoveAt(trimmedTrivia.Count - 1);
        }

        return SyntaxFactory.TriviaList(trimmedTrivia);
    }

    /// <summary>
    /// Determines whether the trivia contains a line break
    /// </summary>
    /// <param name="trivia">Trivia list</param>
    /// <returns><see langword="true"/> if a line break exists; otherwise, <see langword="false"/></returns>
    private static bool ContainsEndOfLine(SyntaxTriviaList trivia)
    {
        return trivia.Any(currentTrivia => currentTrivia.IsKind(SyntaxKind.EndOfLineTrivia));
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH5113DeclarationSemicolonMustStayOnDeclarationLineAnalyzer.DiagnosticId];

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    /// <inheritdoc/>
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return;
        }

        foreach (var diagnostic in context.Diagnostics)
        {
            var diagnosticNode = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
            var declarationNode = GetDeclarationNode(diagnosticNode);

            if (declarationNode == null)
            {
                continue;
            }

            var semicolonToken = declarationNode switch
                                 {
                                     FieldDeclarationSyntax field => field.SemicolonToken,
                                     EventFieldDeclarationSyntax eventField => eventField.SemicolonToken,
                                     DelegateDeclarationSyntax delegateDeclaration => delegateDeclaration.SemicolonToken,
                                     _ => default,
                                 };

            if (semicolonToken.IsKind(SyntaxKind.None) || semicolonToken.IsMissing)
            {
                continue;
            }

            var previousToken = semicolonToken.GetPreviousToken();

            if (previousToken.IsKind(SyntaxKind.None) || HasCommentsOrDirectives(previousToken, semicolonToken))
            {
                continue;
            }

            context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH5113Title,
                                                      token => ApplyCodeFixAsync(context.Document, diagnostic.Location.SourceSpan, token),
                                                      nameof(RH5113DeclarationSemicolonMustStayOnDeclarationLineCodeFixProvider)),
                                    diagnostic);
        }
    }

    #endregion // CodeFixProvider
}