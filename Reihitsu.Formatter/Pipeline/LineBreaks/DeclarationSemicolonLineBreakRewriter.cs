using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.LineBreaks;

/// <summary>
/// Collapses a stray terminating semicolon back onto the line that ends the declaration for
/// field, event field, and delegate declarations
/// </summary>
internal sealed class DeclarationSemicolonLineBreakRewriter : CSharpSyntaxRewriter
{
    #region Fields

    /// <summary>
    /// Cancellation token
    /// </summary>
    private readonly CancellationToken _cancellationToken;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    public DeclarationSemicolonLineBreakRewriter(CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Collapses the semicolon onto the previous token's line when it has been broken onto its own line
    /// </summary>
    /// <typeparam name="TNode">Node type</typeparam>
    /// <param name="node">Declaration node</param>
    /// <param name="semicolonToken">Terminating semicolon token</param>
    /// <returns>The updated declaration node</returns>
    private static TNode CollapseStraySemicolon<TNode>(TNode node, SyntaxToken semicolonToken)
        where TNode : SyntaxNode
    {
        if (semicolonToken.IsMissing || LineBreakTriviaUtilities.HasLeadingEndOfLine(semicolonToken) == false)
        {
            return node;
        }

        return LineBreakTriviaUtilities.CollapseTokenToSameLine(node, semicolonToken);
    }

    #endregion // Methods

    #region CSharpSyntaxVisitor

    /// <inheritdoc/>
    public override SyntaxNode VisitFieldDeclaration(FieldDeclarationSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (FieldDeclarationSyntax)base.VisitFieldDeclaration(node);

        if (node == null)
        {
            return null;
        }

        return CollapseStraySemicolon(node, node.SemicolonToken);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitEventFieldDeclaration(EventFieldDeclarationSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (EventFieldDeclarationSyntax)base.VisitEventFieldDeclaration(node);

        if (node == null)
        {
            return null;
        }

        return CollapseStraySemicolon(node, node.SemicolonToken);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitDelegateDeclaration(DelegateDeclarationSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (DelegateDeclarationSyntax)base.VisitDelegateDeclaration(node);

        if (node == null)
        {
            return null;
        }

        return CollapseStraySemicolon(node, node.SemicolonToken);
    }

    #endregion // CSharpSyntaxVisitor
}