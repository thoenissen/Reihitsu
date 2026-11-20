using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;
using Reihitsu.Formatter.Data;

namespace Reihitsu.Formatter.Pipeline.StructuralTransforms.Rewriter;

/// <summary>
/// Replaces the accessor list of a property or indexer that holds nothing but a <c>get</c> accessor with a
/// member-level expression body, so <c>{ get => e; }</c> and <c>{ get { return e; } }</c> both become <c>=> e;</c>.
/// Whether a member is converted, and the conversion itself, are owned by
/// <see cref="GetOnlyMemberExpressionBodyRewriteUtilities.TryConvertToExpressionBody"/>, which the analyzers that report
/// get-only members and their code fixes use as well. The version gate reads
/// <see cref="FormattingContext.LanguageVersion"/> rather than the node's parse options, because a node that an earlier
/// rewriter replaced belongs to a new tree whose options fall back to the defaults
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

    #region CSharpSyntaxVisitor

    /// <inheritdoc/>
    public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (PropertyDeclarationSyntax)base.VisitPropertyDeclaration(node);

        return node != null
               && GetOnlyMemberExpressionBodyRewriteUtilities.TryConvertToExpressionBody(node, _context.LanguageVersion, _context.EndOfLine, out var convertedMember)
                   ? convertedMember
                   : node;
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitIndexerDeclaration(IndexerDeclarationSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (IndexerDeclarationSyntax)base.VisitIndexerDeclaration(node);

        return node != null
               && GetOnlyMemberExpressionBodyRewriteUtilities.TryConvertToExpressionBody(node, _context.LanguageVersion, _context.EndOfLine, out var convertedMember)
                   ? convertedMember
                   : node;
    }

    #endregion // CSharpSyntaxVisitor
}