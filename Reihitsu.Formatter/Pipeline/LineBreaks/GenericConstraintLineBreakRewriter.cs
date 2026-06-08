using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.LineBreaks;

/// <summary>
/// Applies line-break rules for generic <c>where</c> constraint clauses
/// </summary>
internal sealed class GenericConstraintLineBreakRewriter : CSharpSyntaxRewriter
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
    public GenericConstraintLineBreakRewriter(FormattingContext context,
                                              CancellationToken cancellationToken)
    {
        _context = context;
        _cancellationToken = cancellationToken;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Adapts the constraint clauses of a declaration that owns them, exposing both the current
    /// clauses and a factory that writes updated clauses back onto the declaration
    /// </summary>
    /// <param name="node">The syntax node to inspect</param>
    /// <returns>The clauses and a write-back factory, or <see langword="null"/> when the node does not own constraint clauses</returns>
    private static (SyntaxList<TypeParameterConstraintClauseSyntax> Clauses, Func<SyntaxList<TypeParameterConstraintClauseSyntax>, SyntaxNode> WithClauses)? GetConstraintClauseAdapter(SyntaxNode node)
    {
        return node switch
               {
                   ClassDeclarationSyntax classDeclaration => (classDeclaration.ConstraintClauses, clauses => classDeclaration.WithConstraintClauses(clauses)),
                   StructDeclarationSyntax structDeclaration => (structDeclaration.ConstraintClauses, clauses => structDeclaration.WithConstraintClauses(clauses)),
                   InterfaceDeclarationSyntax interfaceDeclaration => (interfaceDeclaration.ConstraintClauses, clauses => interfaceDeclaration.WithConstraintClauses(clauses)),
                   RecordDeclarationSyntax recordDeclaration => (recordDeclaration.ConstraintClauses, clauses => recordDeclaration.WithConstraintClauses(clauses)),
                   MethodDeclarationSyntax methodDeclaration => (methodDeclaration.ConstraintClauses, clauses => methodDeclaration.WithConstraintClauses(clauses)),
                   DelegateDeclarationSyntax delegateDeclaration => (delegateDeclaration.ConstraintClauses, clauses => delegateDeclaration.WithConstraintClauses(clauses)),
                   LocalFunctionStatementSyntax localFunction => (localFunction.ConstraintClauses, clauses => localFunction.WithConstraintClauses(clauses)),
                   _ => null,
               };
    }

    /// <summary>
    /// Ensures all <c>where</c> constraint clauses on a declaration start on new lines
    /// </summary>
    /// <param name="node">The declaration node</param>
    /// <returns>The node with <c>where</c> clauses on new lines</returns>
    private SyntaxNode EnsureGenericConstraintsOnNewLines(SyntaxNode node)
    {
        var adapter = GetConstraintClauseAdapter(node);

        if (adapter == null || adapter.Value.Clauses.Count == 0)
        {
            return node;
        }

        var modified = false;
        var newClauses = new List<TypeParameterConstraintClauseSyntax>();

        foreach (var clause in adapter.Value.Clauses)
        {
            var whereKeyword = clause.WhereKeyword;

            if (LineBreakTriviaUtilities.HasLeadingEndOfLine(whereKeyword) == false)
            {
                var newWhereKeyword = LineBreakTriviaUtilities.PrependEndOfLine(whereKeyword, _context.EndOfLine);

                newClauses.Add(clause.WithWhereKeyword(newWhereKeyword));
                modified = true;
            }
            else
            {
                newClauses.Add(clause);
            }
        }

        if (modified == false)
        {
            return node;
        }

        return adapter.Value.WithClauses(SyntaxFactory.List(newClauses));
    }

    #endregion // Methods

    #region CSharpSyntaxVisitor

    /// <inheritdoc/>
    public override SyntaxNode Visit(SyntaxNode node)
    {
        if (node == null)
        {
            return null;
        }

        _cancellationToken.ThrowIfCancellationRequested();

        var visited = base.Visit(node);

        if (visited == null)
        {
            return null;
        }

        return EnsureGenericConstraintsOnNewLines(visited);
    }

    #endregion // CSharpSyntaxVisitor
}