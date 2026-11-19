using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;
using Reihitsu.Formatter.Data;

namespace Reihitsu.Formatter.Pipeline.StructuralTransforms.Rewriter;

/// <summary>
/// Converts block-bodied <c>get</c>, <c>set</c>, and <c>init</c> accessors whose body consists of exactly one
/// convertible statement into expression-bodied accessors. The shape, seam, directive, and trivia-transfer rules are
/// owned by <see cref="GetOnlyMemberExpressionBodyRewriteUtilities.ConvertAccessorToExpressionBody"/>, under which
/// <see cref="GetOnlyMemberExpressionBodyTransform"/> also converts a block-bodied getter it lifts to the member level
/// </summary>
internal sealed class AccessorExpressionBodyTransform : CSharpSyntaxRewriter
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
    public AccessorExpressionBodyTransform(FormattingContext context, CancellationToken cancellationToken)
    {
        _context = context;
        _cancellationToken = cancellationToken;
    }

    #endregion // Constructor

    #region CSharpSyntaxVisitor

    /// <inheritdoc/>
    public override SyntaxNode VisitAccessorDeclaration(AccessorDeclarationSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (AccessorDeclarationSyntax)base.VisitAccessorDeclaration(node);

        return GetOnlyMemberExpressionBodyRewriteUtilities.ConvertAccessorToExpressionBody(node, _context.EndOfLine);
    }

    #endregion // CSharpSyntaxVisitor
}