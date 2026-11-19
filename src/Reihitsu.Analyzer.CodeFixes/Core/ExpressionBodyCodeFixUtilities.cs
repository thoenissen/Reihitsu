using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Analyzer.CodeFixes.Core;

/// <summary>
/// Shared helper for the code fixes that replace a member's body with the one the formatter writes: an expression body
/// converted to a block body (SHARED-BODY-003), and the accessor list of a get-only property or indexer converted to an
/// expression body (SHARED-GETONLY-005). The formatter owns the conversion and the layout of the body, but it lays out a
/// body only as part of its member, so it is given the member; of its result, only the body replaces the member's, and the
/// rest of the member stays as written (SHARED-FIX-006)
/// </summary>
internal static class ExpressionBodyCodeFixUtilities
{
    #region Methods

    /// <summary>
    /// Converts the expression body of a member or local function to the block body the formatter produces when it formats
    /// the member where it stands in the document, and changes nothing else of the member
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="member">Member or local function that owns the expression body</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The document with the expression body replaced, or the unchanged document when the formatter does not convert the body</returns>
    public static Task<Document> ConvertBodyAsync(Document document, SyntaxNode member, CancellationToken cancellationToken)
    {
        return ConvertBodyAsync(document, member, BlankLineCodeFixUtilities.FormatNodeInDocumentAsync, cancellationToken);
    }

    /// <summary>
    /// Converts the expression body of a member or local function to the block body the formatter produces for it, and
    /// changes nothing else of the member (<see cref="ReplaceBodyAsync"/>)
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="member">Member or local function that owns the expression body</param>
    /// <param name="formatAsync">Formats the given member, which belongs to the given document, and returns the document with the formatted member</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The document with the expression body replaced, or the unchanged document when the formatter does not convert the body</returns>
    public static Task<Document> ConvertBodyAsync(Document document,
                                                  SyntaxNode member,
                                                  Func<Document, SyntaxNode, CancellationToken, Task<Document>> formatAsync,
                                                  CancellationToken cancellationToken)
    {
        if (GetExpressionBody(member) == null)
        {
            return Task.FromResult(document);
        }

        return ReplaceBodyAsync(document,
                                member,
                                member,
                                GetBodyStartToken,
                                (signature, formattedMember) => GetExpressionBody(formattedMember) == null && GetBody(formattedMember) is { } body
                                                                    ? WithBody(signature, body)
                                                                    : null,
                                formatAsync,
                                cancellationToken);
    }

    /// <summary>
    /// Replaces the body of a member with the body the formatter writes for it, in either direction: the formatter receives
    /// <paramref name="formatterInput"/> in place of the member, either the member itself, which the formatter converts, or
    /// the member already converted, and lays it out where it stands in the document. Of its result, only the body and the
    /// whitespace and comments between the last token in front of the body and the body's first token replace those of the
    /// member, because that gap holds the line break in front of the body, which the formatter places; everything in front
    /// of that gap stays as written (SHARED-BODY-003, SHARED-GETONLY-005)
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="member">Member as written, which belongs to <paramref name="document"/></param>
    /// <param name="formatterInput">The node the formatter receives in place of the member</param>
    /// <param name="getBodyStartToken">Gets the first token of a member's body, or <see langword="default"/> when it has none</param>
    /// <param name="withBodyOf">Gives the member as written, with the formatter's gap in front of its body, the body of the formatted member; returns <see langword="null"/> when the formatted member holds no body to take</param>
    /// <param name="formatAsync">Formats the given node, which belongs to the given document, and returns the document with the formatted node</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The document with the body replaced, or the unchanged document when the formatter's result holds no body to take</returns>
    public static async Task<Document> ReplaceBodyAsync(Document document,
                                                        SyntaxNode member,
                                                        SyntaxNode formatterInput,
                                                        Func<SyntaxNode, SyntaxToken> getBodyStartToken,
                                                        Func<SyntaxNode, SyntaxNode, SyntaxNode> withBodyOf,
                                                        Func<Document, SyntaxNode, CancellationToken, Task<Document>> formatAsync,
                                                        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var bodyStartToken = getBodyStartToken(member);

        if (root == null
            || bodyStartToken.RawKind == 0)
        {
            return document;
        }

        // The formatter's input is marked so that it can be found in the formatter's result; the formatter receives it from
        // the root of the marked document, the tree it belongs to
        var annotation = new SyntaxAnnotation();
        var annotatedDocument = document.WithSyntaxRoot(root.ReplaceNode(member, formatterInput.WithAdditionalAnnotations(annotation)));
        var annotatedRoot = await annotatedDocument.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (annotatedRoot?.GetAnnotatedNodes(annotation).SingleOrDefault() is not { } annotatedInput)
        {
            return document;
        }

        var formattedDocument = await formatAsync(annotatedDocument, annotatedInput, cancellationToken).ConfigureAwait(false);
        var formattedRoot = await formattedDocument.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (formattedRoot?.GetAnnotatedNodes(annotation).SingleOrDefault() is not { } formattedMember
            || getBodyStartToken(formattedMember) is not { RawKind: not 0 } formattedBodyStartToken)
        {
            return document;
        }

        var lastSignatureToken = bodyStartToken.GetPreviousToken();
        var formattedLastSignatureToken = formattedBodyStartToken.GetPreviousToken();
        var fixedMember = withBodyOf(member.ReplaceToken(lastSignatureToken, lastSignatureToken.WithTrailingTrivia(formattedLastSignatureToken.TrailingTrivia)), formattedMember);

        return fixedMember == null
                   ? document
                   : document.WithSyntaxRoot(root.ReplaceNode(member, fixedMember));
    }

    /// <summary>
    /// Gets the first token of the body of a member or local function: the arrow of its expression body, or the opening
    /// brace of its block body
    /// </summary>
    /// <param name="member">Member or local function</param>
    /// <returns>The first token of the body, or <see langword="default"/> when there is none</returns>
    private static SyntaxToken GetBodyStartToken(SyntaxNode member)
    {
        return GetExpressionBody(member)?.ArrowToken ?? GetBody(member)?.OpenBraceToken ?? default;
    }

    /// <summary>
    /// Gets the expression body of a member or local function
    /// </summary>
    /// <param name="member">Member or local function</param>
    /// <returns>The expression body, or <see langword="null"/> when there is none</returns>
    private static ArrowExpressionClauseSyntax GetExpressionBody(SyntaxNode member)
    {
        return member switch
               {
                   BaseMethodDeclarationSyntax method => method.ExpressionBody,
                   LocalFunctionStatementSyntax localFunction => localFunction.ExpressionBody,
                   _ => null
               };
    }

    /// <summary>
    /// Gets the block body of a member or local function
    /// </summary>
    /// <param name="member">Member or local function</param>
    /// <returns>The block body, or <see langword="null"/> when there is none</returns>
    private static BlockSyntax GetBody(SyntaxNode member)
    {
        return member switch
               {
                   BaseMethodDeclarationSyntax method => method.Body,
                   LocalFunctionStatementSyntax localFunction => localFunction.Body,
                   _ => null
               };
    }

    /// <summary>
    /// Replaces the expression body and its semicolon of a member or local function with a block body
    /// </summary>
    /// <param name="member">Member or local function with an expression body</param>
    /// <param name="body">Block body, which carries the trivia behind the member</param>
    /// <returns>The member with the block body</returns>
    private static SyntaxNode WithBody(SyntaxNode member, BlockSyntax body)
    {
        var semicolonToken = SyntaxFactory.Token(SyntaxKind.None);

        return member switch
               {
                   BaseMethodDeclarationSyntax method => method.WithExpressionBody(null)
                                                               .WithSemicolonToken(semicolonToken)
                                                               .WithBody(body),
                   LocalFunctionStatementSyntax localFunction => localFunction.WithExpressionBody(null)
                                                                              .WithSemicolonToken(semicolonToken)
                                                                              .WithBody(body),
                   _ => member
               };
    }

    #endregion // Methods
}