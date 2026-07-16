using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.UsingDirectives;

/// <summary>
/// Rewrites using directive scopes into canonical grouped order. It is thin glue that reads each
/// scope, orders the directives via <see cref="UsingGrouping"/>, restitches their leading trivia via
/// <see cref="UsingLeadingTriviaBuilder"/> and writes the result back with <c>WithUsings</c>
/// </summary>
internal sealed class UsingDirectiveOrderingRewriter : CSharpSyntaxRewriter
{
    #region Fields

    /// <summary>
    /// Cancellation token
    /// </summary>
    private readonly CancellationToken _cancellationToken;

    /// <summary>
    /// Preferred end-of-line sequence
    /// </summary>
    private readonly string _endOfLine;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="endOfLine">Preferred line ending</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public UsingDirectiveOrderingRewriter(string endOfLine, CancellationToken cancellationToken)
    {
        _endOfLine = endOfLine;
        _cancellationToken = cancellationToken;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Organizes the provided using directives into grouped canonical order
    /// </summary>
    /// <param name="usingDirectives">Using directives to organize</param>
    /// <param name="endOfLine">Preferred end-of-line sequence</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The organized directives</returns>
    internal static SyntaxList<UsingDirectiveSyntax> OrganizeUsingDirectives(SyntaxList<UsingDirectiveSyntax> usingDirectives,
                                                                             string endOfLine,
                                                                             CancellationToken cancellationToken)
    {
        if (usingDirectives.Count <= 1)
        {
            return usingDirectives;
        }

        if (UsingDirectiveOrderingSafety.CanSafelyReorder(usingDirectives) == false)
        {
            return usingDirectives;
        }

        var originalFirst = usingDirectives.First();
        var firstLeadingTriviaPrefix = UsingLeadingTriviaBuilder.GetWhitespacePrefix(originalFirst.GetLeadingTrivia());
        var canonical = UsingGrouping.ComputeCanonicalOrder(usingDirectives);

        if (ReferenceEquals(canonical[0], originalFirst) == false)
        {
            var (header, remainder) = UsingLeadingTriviaBuilder.SplitOriginalFirstHeaderTrivia(originalFirst.GetLeadingTrivia());

            if (header.Count > 0)
            {
                firstLeadingTriviaPrefix = firstLeadingTriviaPrefix.AddRange(header);

                var detachedFirst = originalFirst.WithLeadingTrivia(remainder);

                canonical = canonical.ConvertAll(current => ReferenceEquals(current, originalFirst) ? detachedFirst : current);
            }
        }

        var result = new List<UsingDirectiveSyntax>();

        for (var usingIndex = 0; usingIndex < canonical.Count; usingIndex++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var current = canonical[usingIndex];

            if (usingIndex == 0)
            {
                result.Add(current.WithLeadingTrivia(UsingLeadingTriviaBuilder.CreateLeadingTrivia(current, firstLeadingTriviaPrefix, startsNewGroup: false, isFirst: true, endOfLine)));

                continue;
            }

            result.Add(current.WithLeadingTrivia(UsingLeadingTriviaBuilder.CreateLeadingTrivia(current,
                                                                                               firstLeadingTriviaPrefix,
                                                                                               startsNewGroup: UsingGrouping.AreInSameGroup(canonical[usingIndex - 1], current) == false,
                                                                                               isFirst: false,
                                                                                               endOfLine)));
        }

        return SyntaxFactory.List(result);
    }

    /// <summary>
    /// Replaces the using directive list on a scope node
    /// </summary>
    /// <param name="scope">Compilation unit or namespace declaration</param>
    /// <param name="usingDirectives">Updated using directives</param>
    /// <returns>The updated scope node</returns>
    private static SyntaxNode WithUsings(SyntaxNode scope, SyntaxList<UsingDirectiveSyntax> usingDirectives)
    {
        return scope switch
               {
                   CompilationUnitSyntax compilationUnit => compilationUnit.WithUsings(usingDirectives),
                   BaseNamespaceDeclarationSyntax namespaceDeclaration => namespaceDeclaration.WithUsings(usingDirectives),
                   _ => scope,
               };
    }

    #endregion // Methods

    #region CSharpSyntaxVisitor

    /// <inheritdoc/>
    public override SyntaxNode VisitCompilationUnit(CompilationUnitSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (CompilationUnitSyntax)base.VisitCompilationUnit(node);

        if (node == null || node.Usings.Count < 2)
        {
            return node;
        }

        return (CompilationUnitSyntax)WithUsings(node, OrganizeUsingDirectives(node.Usings, _endOfLine, _cancellationToken));
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitFileScopedNamespaceDeclaration(FileScopedNamespaceDeclarationSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (FileScopedNamespaceDeclarationSyntax)base.VisitFileScopedNamespaceDeclaration(node);

        if (node == null || node.Usings.Count < 2)
        {
            return node;
        }

        return (FileScopedNamespaceDeclarationSyntax)WithUsings(node, OrganizeUsingDirectives(node.Usings, _endOfLine, _cancellationToken));
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (NamespaceDeclarationSyntax)base.VisitNamespaceDeclaration(node);

        if (node == null || node.Usings.Count < 2)
        {
            return node;
        }

        return (NamespaceDeclarationSyntax)WithUsings(node, OrganizeUsingDirectives(node.Usings, _endOfLine, _cancellationToken));
    }

    #endregion // CSharpSyntaxVisitor
}