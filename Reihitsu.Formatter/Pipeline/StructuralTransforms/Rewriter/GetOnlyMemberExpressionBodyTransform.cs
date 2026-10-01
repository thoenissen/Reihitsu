using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;
using Reihitsu.Formatter.Data;

namespace Reihitsu.Formatter.Pipeline.StructuralTransforms.Rewriter;

/// <summary>
/// Replaces the accessor list of a property or indexer that holds nothing but a <c>get</c> accessor with a
/// member-level expression body, so <c>{ get => e; }</c> and <c>{ get { return e; } }</c> both become <c>=> e;</c>.
/// A block-bodied getter qualifies exactly when <see cref="AccessorExpressionBodyTransform"/> would convert it.
/// The accessor list is kept when the getter carries attributes or modifiers, has no body, or the property has an
/// initializer, because none of them has a place on an expression-bodied member. It is also kept whenever a comment,
/// directive, or disabled text sits in trivia the rewrite would delete or join onto another line; a comment trailing
/// the getter's semicolon or the accessor list's closing brace already ends the member's last line and follows the
/// new semicolon instead
/// </summary>
internal sealed class GetOnlyMemberExpressionBodyTransform : CSharpSyntaxRewriter
{
    #region Fields

    /// <summary>
    /// The formatting context
    /// </summary>
    private readonly FormattingContext _context;

    /// <summary>
    /// The cancellation token
    /// </summary>
    private readonly CancellationToken _cancellationToken;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context">The formatting context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public GetOnlyMemberExpressionBodyTransform(FormattingContext context, CancellationToken cancellationToken)
    {
        _context = context;
        _cancellationToken = cancellationToken;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines the expression-bodied form of the accessor list's only accessor
    /// </summary>
    /// <param name="accessorList">The accessor list to inspect</param>
    /// <param name="endOfLine">The end-of-line sequence of the formatting run</param>
    /// <param name="getter">The expression-bodied getter, converted from a block body when necessary</param>
    /// <returns><see langword="true"/> if the list holds a single plain getter that has an expression-bodied form; otherwise, <see langword="false"/></returns>
    /// <remarks>
    /// Attributes and modifiers on the getter have no place on the member: moving an attribute would change its
    /// target, and dropping a modifier would lose a token. A block body is converted by
    /// <see cref="AccessorExpressionBodyTransform.ConvertToExpressionBody"/>, so it qualifies under exactly the shape,
    /// seam, and directive rules of the accessor-level conversion
    /// </remarks>
    private static bool TryGetExpressionBodiedGetter(AccessorListSyntax accessorList, string endOfLine, out AccessorDeclarationSyntax getter)
    {
        getter = null;

        if (accessorList.OpenBraceToken.IsMissing
            || accessorList.CloseBraceToken.IsMissing
            || accessorList.Accessors.Count != 1)
        {
            return false;
        }

        var accessor = accessorList.Accessors[0];

        if (accessor.IsKind(SyntaxKind.GetAccessorDeclaration) == false
            || accessor.AttributeLists.Count > 0
            || accessor.Modifiers.Count > 0)
        {
            return false;
        }

        if (accessor.Body != null)
        {
            accessor = AccessorExpressionBodyTransform.ConvertToExpressionBody(accessor, endOfLine);
        }

        if (accessor.ExpressionBody == null
            || accessor.SemicolonToken.IsMissing)
        {
            return false;
        }

        getter = accessor;

        return true;
    }

    /// <summary>
    /// Determines whether the trivia the rewrite deletes or joins onto the member signature's line is plain layout.
    /// These seams are the trailing trivia of the token before the accessor list, both sides of the opening brace,
    /// both sides of the <c>get</c> keyword, both sides of the getter's arrow, the expression's leading trivia, the
    /// getter semicolon's leading trivia, and the closing brace's leading trivia
    /// </summary>
    /// <param name="accessorList">The accessor list to inspect</param>
    /// <param name="getter">The expression-bodied getter</param>
    /// <returns><see langword="true"/> if every seam holds only layout trivia; otherwise, <see langword="false"/></returns>
    /// <remarks>
    /// The token before the accessor list lies outside the list. A comment trailing it would leave the arrow on the
    /// next line once the opening brace is gone, so it is inspected together with the list's own seams
    /// </remarks>
    private static bool HasLayoutOnlySeams(AccessorListSyntax accessorList, AccessorDeclarationSyntax getter)
    {
        var arrowToken = getter.ExpressionBody.ArrowToken;

        return AccessorExpressionBodyTransform.IsLayoutOnly(accessorList.OpenBraceToken.GetPreviousToken().TrailingTrivia)
               && AccessorExpressionBodyTransform.IsLayoutOnly(accessorList.OpenBraceToken.LeadingTrivia)
               && AccessorExpressionBodyTransform.IsLayoutOnly(accessorList.OpenBraceToken.TrailingTrivia)
               && AccessorExpressionBodyTransform.IsLayoutOnly(getter.Keyword.LeadingTrivia)
               && AccessorExpressionBodyTransform.IsLayoutOnly(getter.Keyword.TrailingTrivia)
               && AccessorExpressionBodyTransform.IsLayoutOnly(arrowToken.LeadingTrivia)
               && AccessorExpressionBodyTransform.IsLayoutOnly(arrowToken.TrailingTrivia)
               && AccessorExpressionBodyTransform.IsLayoutOnly(getter.ExpressionBody.Expression.GetFirstToken().LeadingTrivia)
               && AccessorExpressionBodyTransform.IsLayoutOnly(getter.SemicolonToken.LeadingTrivia)
               && AccessorExpressionBodyTransform.IsLayoutOnly(accessorList.CloseBraceToken.LeadingTrivia);
    }

    /// <summary>
    /// Determines whether a directive inside the accessor list would be split or reconstructed by the rewrite. A
    /// conditional group whose partner lies outside the list is orphaned once the braces disappear, and a region
    /// directive cannot keep both endpoints in place while the list is rebuilt. A balanced conditional group or
    /// other directive wholly inside the expression travels with it unchanged
    /// </summary>
    /// <param name="member">The property or indexer declaration</param>
    /// <param name="accessorList">The member's accessor list</param>
    /// <returns><see langword="true"/> if the accessor list carries such a directive; otherwise, <see langword="false"/></returns>
    private static bool ContainsBlockingDirective(SyntaxNode member, AccessorListSyntax accessorList)
    {
        return SyntaxTriviaUtilities.ContainsUnbalancedConditionalDirectives(member, accessorList.Span)
               || SyntaxTriviaUtilities.ContainsRegionDirectives(member, accessorList.FullSpan);
    }

    /// <summary>
    /// Removes the layout trivia that separated the member signature from the accessor list, so the arrow follows
    /// the signature on its line
    /// </summary>
    /// <typeparam name="TNode">The declaration type</typeparam>
    /// <param name="node">The property or indexer declaration</param>
    /// <returns>The declaration without trailing trivia on the token before the accessor list</returns>
    /// <remarks>
    /// The trivia is layout only, which <see cref="HasLayoutOnlySeams"/> establishes before the rewrite starts
    /// </remarks>
    private static TNode RemoveSignatureTrailingTrivia<TNode>(TNode node)
        where TNode : BasePropertyDeclarationSyntax
    {
        var signatureEnd = node.AccessorList.OpenBraceToken.GetPreviousToken();

        return node.ReplaceToken(signatureEnd, signatureEnd.WithTrailingTrivia(SyntaxFactory.TriviaList()));
    }

    /// <summary>
    /// Builds the member-level arrow clause and semicolon that replace the accessor list
    /// </summary>
    /// <param name="accessorList">The accessor list to replace</param>
    /// <param name="arrowExpressionClause">The member-level arrow clause</param>
    /// <param name="semicolonToken">The member-level semicolon</param>
    /// <returns><see langword="true"/> if the accessor list can be replaced without losing or misplacing trivia; otherwise, <see langword="false"/></returns>
    /// <remarks>
    /// A throw expression only exists from C# 7.0 on, so a getter that throws keeps its accessor list for earlier
    /// language versions. The version is read from <see cref="FormattingContext.LanguageVersion"/> rather than the
    /// node's parse options, because a node that an earlier rewriter replaced belongs to a new tree whose options fall
    /// back to the defaults
    /// </remarks>
    private bool TryCreateExpressionBody(AccessorListSyntax accessorList,
                                         out ArrowExpressionClauseSyntax arrowExpressionClause,
                                         out SyntaxToken semicolonToken)
    {
        arrowExpressionClause = null;
        semicolonToken = default;

        if (TryGetExpressionBodiedGetter(accessorList, _context.EndOfLine, out var getter) == false)
        {
            return false;
        }

        var expression = getter.ExpressionBody.Expression;

        if (expression is ThrowExpressionSyntax
            && _context.LanguageVersion < LanguageVersion.CSharp7)
        {
            return false;
        }

        var expressionTail = expression.GetLastToken().TrailingTrivia;

        if (HasLayoutOnlySeams(accessorList, getter) == false
            || ContainsBlockingDirective(accessorList.Parent, accessorList)
            || expressionTail.Any(SyntaxKind.SingleLineCommentTrivia)
            || AccessorExpressionBodyTransform.TryBuildSemicolonTrailingTrivia(getter.SemicolonToken, accessorList.CloseBraceToken, _context.EndOfLine, out var semicolonTrailingTrivia) == false)
        {
            return false;
        }

        var bodyExpression = expression.WithoutLeadingTrivia()
                                       .WithTrailingTrivia(AccessorExpressionBodyTransform.RemoveLineBreaks(expressionTail));

        arrowExpressionClause = SyntaxFactory.ArrowExpressionClause(SyntaxFactory.Token(SyntaxKind.EqualsGreaterThanToken)
                                                                                 .WithLeadingTrivia(SyntaxFactory.Space)
                                                                                 .WithTrailingTrivia(SyntaxFactory.Space),
                                                                    bodyExpression);
        semicolonToken = SyntaxFactory.Token(SyntaxKind.SemicolonToken).WithTrailingTrivia(semicolonTrailingTrivia);

        return true;
    }

    #endregion // Methods

    #region CSharpSyntaxVisitor

    /// <inheritdoc/>
    public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (PropertyDeclarationSyntax)base.VisitPropertyDeclaration(node);

        if (node?.AccessorList == null
            || node.Initializer != null
            || TryCreateExpressionBody(node.AccessorList, out var arrowExpressionClause, out var semicolonToken) == false)
        {
            return node;
        }

        return RemoveSignatureTrailingTrivia(node).WithAccessorList(null)
                                                  .WithExpressionBody(arrowExpressionClause)
                                                  .WithSemicolonToken(semicolonToken);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitIndexerDeclaration(IndexerDeclarationSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (IndexerDeclarationSyntax)base.VisitIndexerDeclaration(node);

        if (node?.AccessorList == null
            || TryCreateExpressionBody(node.AccessorList, out var arrowExpressionClause, out var semicolonToken) == false)
        {
            return node;
        }

        return RemoveSignatureTrailingTrivia(node).WithAccessorList(null)
                                                  .WithExpressionBody(arrowExpressionClause)
                                                  .WithSemicolonToken(semicolonToken);
    }

    #endregion // CSharpSyntaxVisitor
}