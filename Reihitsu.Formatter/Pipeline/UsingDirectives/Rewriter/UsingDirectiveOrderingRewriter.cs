using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;
using Reihitsu.Formatter.Pipeline.UsingDirectives.Utilities;

namespace Reihitsu.Formatter.Pipeline.UsingDirectives.Rewriter;

/// <summary>
/// Rewrites using directive scopes into canonical grouped order. It is thin glue that reads each
/// scope, orders the directives via <see cref="UsingGrouping"/>, restitches their leading trivia via
/// <see cref="UsingLeadingTriviaBuilder"/>, normalizes each directive's trailing line break for its new
/// position, and writes the result back with <c>WithUsings</c>
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

        var originalBlockEndsWithLineBreak = EndsWithLineBreak(usingDirectives.Last().GetTrailingTrivia());
        var result = new List<UsingDirectiveSyntax>();

        for (var usingIndex = 0; usingIndex < canonical.Count; usingIndex++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var current = canonical[usingIndex];
            var trailingTrivia = CreateTrailingTrivia(current, isLast: usingIndex == canonical.Count - 1, originalBlockEndsWithLineBreak, endOfLine);

            if (usingIndex == 0)
            {
                result.Add(current.WithLeadingTrivia(UsingLeadingTriviaBuilder.CreateLeadingTrivia(current, firstLeadingTriviaPrefix, startsNewGroup: false, isFirst: true, endOfLine))
                                  .WithTrailingTrivia(trailingTrivia));

                continue;
            }

            result.Add(current.WithLeadingTrivia(UsingLeadingTriviaBuilder.CreateLeadingTrivia(current,
                                                                                               firstLeadingTriviaPrefix,
                                                                                               startsNewGroup: UsingGrouping.AreInSameGroup(canonical[usingIndex - 1], current) == false,
                                                                                               isFirst: false,
                                                                                               endOfLine))
                              .WithTrailingTrivia(trailingTrivia));
        }

        return SyntaxFactory.List(result);
    }

    /// <summary>
    /// Rebuilds the trailing trivia of a reordered directive so a line break always separates it from
    /// its new successor. Reordering carries each directive's original trailing trivia along with its
    /// node, but that trivia was authored for the directive's old neighbor: the block's original last
    /// directive typically ends with no line break at all, since nothing followed it before end of file
    /// or an enclosing scope, and moving it out of last position would otherwise glue it to whatever now
    /// follows. Every non-last directive is therefore guaranteed exactly one trailing line break; the
    /// directive that ends up last is given the block's own original terminating shape instead of
    /// whatever line break its occupying node happened to carry
    /// </summary>
    /// <param name="current">Current using directive</param>
    /// <param name="isLast">Whether the directive is the last in the reordered block</param>
    /// <param name="originalBlockEndsWithLineBreak">Whether the original, pre-reorder block ended in a line break</param>
    /// <param name="endOfLine">Preferred end-of-line sequence</param>
    /// <returns>The trailing trivia to apply</returns>
    private static SyntaxTriviaList CreateTrailingTrivia(UsingDirectiveSyntax current, bool isLast, bool originalBlockEndsWithLineBreak, string endOfLine)
    {
        var withoutLineBreak = StripTrailingLineBreak(current.GetTrailingTrivia());

        if (isLast)
        {
            return originalBlockEndsWithLineBreak
                       ? withoutLineBreak.Add(SyntaxFactory.EndOfLine(endOfLine))
                       : withoutLineBreak;
        }

        return withoutLineBreak.Add(SyntaxFactory.EndOfLine(endOfLine));
    }

    /// <summary>
    /// Removes a single trailing end-of-line trivia from the end of a trivia list, if present
    /// </summary>
    /// <param name="trivia">Trivia list to strip</param>
    /// <returns>The trivia list without its trailing end-of-line trivia</returns>
    private static SyntaxTriviaList StripTrailingLineBreak(SyntaxTriviaList trivia)
    {
        return EndsWithLineBreak(trivia)
                   ? SyntaxFactory.TriviaList(trivia.Take(trivia.Count - 1))
                   : trivia;
    }

    /// <summary>
    /// Determines whether a trivia list ends with an end-of-line trivia
    /// </summary>
    /// <param name="trivia">Trivia list to inspect</param>
    /// <returns><see langword="true"/> if the trivia list ends with an end-of-line trivia; otherwise, <see langword="false"/></returns>
    private static bool EndsWithLineBreak(SyntaxTriviaList trivia)
    {
        return trivia.Count > 0 && trivia[trivia.Count - 1].IsKind(SyntaxKind.EndOfLineTrivia);
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

        return (CompilationUnitSyntax)UsingDirectiveOrderingUtilities.WithUsings(node, OrganizeUsingDirectives(node.Usings, _endOfLine, _cancellationToken));
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

        return (FileScopedNamespaceDeclarationSyntax)UsingDirectiveOrderingUtilities.WithUsings(node, OrganizeUsingDirectives(node.Usings, _endOfLine, _cancellationToken));
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

        return (NamespaceDeclarationSyntax)UsingDirectiveOrderingUtilities.WithUsings(node, OrganizeUsingDirectives(node.Usings, _endOfLine, _cancellationToken));
    }

    #endregion // CSharpSyntaxVisitor
}