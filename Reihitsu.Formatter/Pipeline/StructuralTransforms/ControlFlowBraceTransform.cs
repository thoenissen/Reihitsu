using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.StructuralTransforms;

/// <summary>
/// Inserts missing braces for control-flow statements handled by the formatter
/// </summary>
internal sealed class ControlFlowBraceTransform : CSharpSyntaxRewriter
{
    #region Fields

    /// <summary>
    /// The cancellation token
    /// </summary>
    private readonly CancellationToken _cancellationToken;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    public ControlFlowBraceTransform(CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Wraps a statement in a block when it is not already braced
    /// </summary>
    /// <param name="statement">The statement to inspect</param>
    /// <returns>The braced statement</returns>
    private static StatementSyntax WrapWithBraces(StatementSyntax statement)
    {
        if (statement is null or BlockSyntax)
        {
            return statement;
        }

        var normalizedStatement = statement.WithLeadingTrivia(SyntaxFactory.ElasticMarker)
                                           .WithTrailingTrivia(SyntaxFactory.ElasticMarker);

        return SyntaxFactory.Block(SyntaxFactory.SingletonList(normalizedStatement));
    }

    #endregion // Methods

    #region CSharpSyntaxVisitor

    /// <inheritdoc/>
    public override SyntaxNode VisitIfStatement(IfStatementSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (IfStatementSyntax)base.VisitIfStatement(node);

        if (node == null)
        {
            return null;
        }

        node = node.WithStatement(WrapWithBraces(node.Statement));

        if (node.Else is { Statement: not null and not BlockSyntax and not IfStatementSyntax } elseClause)
        {
            node = node.WithElse(elseClause.WithStatement(WrapWithBraces(elseClause.Statement)));
        }

        return node;
    }

    #endregion // CSharpSyntaxVisitor
}