using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Formatter;

namespace Reihitsu.Analyzer.CodeFixes.Core;

/// <summary>
/// Shared helper for the code fixes that convert an expression body to a block body (SHARED-BODY-003). The formatter owns
/// the conversion and the layout of the block, but it converts a body only as part of its member, so it is given the
/// member; of its result, only the block replaces the expression body, and the rest of the member stays as written
/// (SHARED-FIX-006)
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
        return ConvertBodyAsync(document, member, FormatNodeAsync, cancellationToken);
    }

    /// <summary>
    /// Converts the expression body of a member or local function to the block body the formatter produces for it, and
    /// changes nothing else of the member. The whitespace and comments between the last token in front of <c>=&gt;</c> and
    /// the new <c>{</c> are those of the formatter's result, because that gap holds the line break in front of the block,
    /// which the formatter places; everything in front of that gap stays as written
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="member">Member or local function that owns the expression body</param>
    /// <param name="formatAsync">Formats the given member, which belongs to the given document, and returns the document with the formatted member</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The document with the expression body replaced, or the unchanged document when the formatter does not convert the body</returns>
    public static async Task<Document> ConvertBodyAsync(Document document,
                                                        SyntaxNode member,
                                                        Func<Document, SyntaxNode, CancellationToken, Task<Document>> formatAsync,
                                                        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null
            || GetExpressionBody(member) is not { } expressionBody)
        {
            return document;
        }

        // The member is marked so that it can be found in the formatter's result; the formatter receives it from the root of
        // the marked document, the tree it belongs to
        var annotation = new SyntaxAnnotation();
        var annotatedDocument = document.WithSyntaxRoot(root.ReplaceNode(member, member.WithAdditionalAnnotations(annotation)));
        var annotatedRoot = await annotatedDocument.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (annotatedRoot?.GetAnnotatedNodes(annotation).SingleOrDefault() is not { } annotatedMember)
        {
            return document;
        }

        var formattedDocument = await formatAsync(annotatedDocument, annotatedMember, cancellationToken).ConfigureAwait(false);
        var formattedRoot = await formattedDocument.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (formattedRoot?.GetAnnotatedNodes(annotation).SingleOrDefault() is not { } formattedMember
            || GetExpressionBody(formattedMember) != null
            || GetBody(formattedMember) is not { } body)
        {
            return document;
        }

        var lastSignatureToken = expressionBody.ArrowToken.GetPreviousToken();
        var formattedLastSignatureToken = body.OpenBraceToken.GetPreviousToken();
        var convertedMember = member.ReplaceToken(lastSignatureToken, lastSignatureToken.WithTrailingTrivia(formattedLastSignatureToken.TrailingTrivia));

        return document.WithSyntaxRoot(root.ReplaceNode(member, WithBody(convertedMember, body)));
    }

    /// <summary>
    /// Formats a member where it stands in the document
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="member">Member, which belongs to <paramref name="document"/></param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The document with the formatted member</returns>
    private static async Task<Document> FormatNodeAsync(Document document, SyntaxNode member, CancellationToken cancellationToken)
    {
        return (await ReihitsuFormatter.FormatNodeInDocumentAsync(document, member, cancellationToken).ConfigureAwait(false)).Output;
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