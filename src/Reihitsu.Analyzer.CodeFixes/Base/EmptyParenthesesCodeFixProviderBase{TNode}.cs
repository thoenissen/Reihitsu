using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.CodeFixes.Core;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.CodeFixes.Base;

/// <summary>
/// Code fix provider base class for rules that report an empty list in parentheses, such as the parameter list of an
/// anonymous method or the argument list of an attribute, and remove it (SHARED-PARENS-002). The fix is withheld when a
/// comment or a preprocessor directive stands between the parentheses, which the removal would delete
/// (SHARED-PARENS-003). The whitespace the removal leaves next to each other becomes one space, no whitespace stays in
/// front of a line break, and the whitespace in front of the token behind the list is laid out as the derived rule states
/// </summary>
/// <typeparam name="TNode">Node that owns the list</typeparam>
public abstract class EmptyParenthesesCodeFixProviderBase<TNode> : CodeFixProvider
    where TNode : SyntaxNode
{
    #region Fields

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    private readonly string _diagnosticId;

    /// <summary>
    /// Title
    /// </summary>
    private readonly string _title;

    /// <summary>
    /// Analyzer that reports the diagnostic
    /// </summary>
    private readonly DiagnosticAnalyzer _analyzer;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="diagnosticId">Diagnostic ID</param>
    /// <param name="title">Title</param>
    /// <param name="analyzer">Analyzer that reports the diagnostic</param>
    private protected EmptyParenthesesCodeFixProviderBase(string diagnosticId, string title, DiagnosticAnalyzer analyzer)
    {
        _diagnosticId = diagnosticId;
        _title = title;
        _analyzer = analyzer;
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// Whether one space separates the code in front of the removed list from the token behind it, as between
    /// <c>delegate</c> and the body of an anonymous method; otherwise, no whitespace stays in front of that token, as in
    /// front of the <c>]</c> or the <c>,</c> behind an attribute
    /// </summary>
    private protected abstract bool KeepsSpaceInFrontOfFollowingToken { get; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Gets the reported list of the node
    /// </summary>
    /// <param name="node">Node that owns the list</param>
    /// <returns>The list, or <see langword="null"/> when the node has none</returns>
    private protected abstract SyntaxNode GetList(TNode node);

    /// <summary>
    /// Removes the list from the node and gives the code in front of it the trailing trivia that separates it from the
    /// token behind the removed list
    /// </summary>
    /// <param name="node">Node that owns the list</param>
    /// <param name="separatingTrivia">Trailing trivia of the code in front of the list</param>
    /// <returns>The node without the list</returns>
    private protected abstract TNode RemoveList(TNode node, SyntaxTriviaList separatingTrivia);

    /// <summary>
    /// Determines whether removing the list keeps the meaning of the code. Every list does by default
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="root">Root of the document</param>
    /// <param name="node">Node that owns the list</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> if the list can be removed</returns>
    private protected virtual Task<bool> KeepsMeaningAsync(Document document, SyntaxNode root, TNode node, CancellationToken cancellationToken)
    {
        return Task.FromResult(true);
    }

    /// <summary>
    /// Builds the node without its list
    /// </summary>
    /// <param name="node">Node that owns the list</param>
    /// <returns>The node without the list</returns>
    private protected TNode BuildNodeWithoutList(TNode node)
    {
        var list = GetList(node);
        var openParenthesis = list.GetFirstToken();
        var closeParenthesis = list.GetLastToken();
        var precedingTrivia = openParenthesis.GetPreviousToken().TrailingTrivia.AddRange(openParenthesis.LeadingTrivia).AddRange(closeParenthesis.TrailingTrivia);

        return RemoveList(node, BuildSeparatingTrivia(precedingTrivia));
    }

    /// <summary>
    /// Builds the trivia that separates the code in front of the removed list from the token behind it: each run of
    /// whitespace within a line becomes one space, whitespace that starts a line stays as written, no whitespace stays
    /// in front of a line break, and the whitespace at the end follows <see cref="KeepsSpaceInFrontOfFollowingToken"/>
    /// </summary>
    /// <param name="triviaList">The trivia in front of the list and behind it</param>
    /// <returns>The separating trivia</returns>
    private SyntaxTriviaList BuildSeparatingTrivia(SyntaxTriviaList triviaList)
    {
        var separatingTrivia = new List<SyntaxTrivia>();

        foreach (var trivia in triviaList)
        {
            var previousTrivia = separatingTrivia.Count > 0
                                     ? separatingTrivia[separatingTrivia.Count - 1]
                                     : default;

            if (trivia.IsKind(SyntaxKind.WhitespaceTrivia))
            {
                if (previousTrivia.IsKind(SyntaxKind.EndOfLineTrivia))
                {
                    separatingTrivia.Add(trivia);
                }
                else if (previousTrivia.IsKind(SyntaxKind.WhitespaceTrivia) == false)
                {
                    separatingTrivia.Add(SyntaxFactory.Space);
                }

                continue;
            }

            if (trivia.IsKind(SyntaxKind.EndOfLineTrivia)
                && previousTrivia.IsKind(SyntaxKind.WhitespaceTrivia))
            {
                separatingTrivia.RemoveAt(separatingTrivia.Count - 1);
            }

            separatingTrivia.Add(trivia);
        }

        var lastTrivia = separatingTrivia.Count > 0
                             ? separatingTrivia[separatingTrivia.Count - 1]
                             : default;
        var lastTriviaStartsLine = separatingTrivia.Count > 1
                                   && separatingTrivia[separatingTrivia.Count - 2].IsKind(SyntaxKind.EndOfLineTrivia);

        if (lastTrivia.IsKind(SyntaxKind.EndOfLineTrivia) == false
            && lastTriviaStartsLine == false)
        {
            if (lastTrivia.IsKind(SyntaxKind.WhitespaceTrivia))
            {
                separatingTrivia.RemoveAt(separatingTrivia.Count - 1);
            }

            if (KeepsSpaceInFrontOfFollowingToken)
            {
                separatingTrivia.Add(SyntaxFactory.Space);
            }
        }

        return SyntaxFactory.TriviaList(separatingTrivia);
    }

    /// <summary>
    /// Applying the code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="node">Node that owns the list</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated <see cref="Document"/> with the code fix applied</returns>
    private async Task<Document> ApplyCodeFixAsync(Document document, TNode node, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        return document.WithSyntaxRoot(root.ReplaceNode(node, BuildNodeWithoutList(node)));
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [_diagnosticId];

    /// <inheritdoc/>
    /// <remarks>
    /// The fixes of a document are applied in document order, each decided on the code the fixes in front of it
    /// produced, so that two fixes that are safe alone cannot together write the code the guard withholds
    /// (SHARED-FIX-002)
    /// </remarks>
    public sealed override FixAllProvider GetFixAllProvider()
    {
        return DocumentOrderFixAllProvider.Create(_analyzer);
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
            var node = root.FindToken(diagnostic.Location.SourceSpan.Start)
                           .Parent
                           ?.AncestorsAndSelf()
                           .OfType<TNode>()
                           .FirstOrDefault();

            if (node == null
                || GetList(node) is not { } list
                || SyntaxNodeUtilities.InteriorContainsCommentOrDirective(list)
                || await KeepsMeaningAsync(context.Document, root, node, context.CancellationToken).ConfigureAwait(false) == false)
            {
                continue;
            }

            context.RegisterCodeFix(CodeAction.Create(_title,
                                                      token => ApplyCodeFixAsync(context.Document, node, token),
                                                      GetType().Name),
                                    diagnostic);
        }
    }

    #endregion // CodeFixProvider
}