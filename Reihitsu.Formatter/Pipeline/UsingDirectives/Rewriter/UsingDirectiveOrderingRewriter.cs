using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;
using Reihitsu.Formatter.Pipeline.UsingDirectives.Utilities;

namespace Reihitsu.Formatter.Pipeline.UsingDirectives.Rewriter;

/// <summary>
/// Rewrites using directive scopes into canonical grouped order with every directive on its own line.
/// It is thin glue that reads each scope, orders the directives via <see cref="UsingGrouping"/>,
/// restitches their leading trivia via <see cref="UsingLeadingTriviaBuilder"/> and their trailing trivia
/// via <see cref="UsingTrailingTriviaBuilder"/>, and writes the result back with <c>WithUsings</c>
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
    /// Organizes the provided using directives into grouped canonical order. A block that cannot be
    /// reordered safely keeps its order and blank lines, and only has directives that share a line
    /// separated, see <see cref="SeparateDirectivesSharingALine"/>
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

        var originalFirst = usingDirectives.First();
        var firstLeadingTriviaPrefix = UsingLeadingTriviaBuilder.GetWhitespacePrefix(originalFirst.GetLeadingTrivia());
        var scopeIndentation = GetScopeIndentation(originalFirst);

        if (UsingDirectiveOrderingSafety.CanSafelyReorder(usingDirectives) == false)
        {
            return SeparateDirectivesSharingALine(usingDirectives, scopeIndentation, endOfLine, cancellationToken);
        }

        var directivesStartingTheirLine = new HashSet<UsingDirectiveSyntax>(usingDirectives.Where(UsingDirectiveOrderingUtilities.StartsItsLine));
        var canonical = UsingGrouping.ComputeCanonicalOrder(usingDirectives);

        if (ReferenceEquals(canonical[0], originalFirst) == false)
        {
            var (header, remainder) = UsingLeadingTriviaBuilder.SplitOriginalFirstHeaderTrivia(originalFirst.GetLeadingTrivia());

            if (header.Count > 0)
            {
                firstLeadingTriviaPrefix = firstLeadingTriviaPrefix.AddRange(header);

                var detachedFirst = originalFirst.WithLeadingTrivia(remainder);

                if (directivesStartingTheirLine.Contains(originalFirst))
                {
                    directivesStartingTheirLine.Add(detachedFirst);
                }

                canonical = canonical.ConvertAll(current => ReferenceEquals(current, originalFirst) ? detachedFirst : current);
            }
        }

        var originalBlockTerminalTrivia = UsingTrailingTriviaBuilder.GetTrailingLayoutTrivia(usingDirectives.Last().GetTrailingTrivia());
        var result = new List<UsingDirectiveSyntax>();

        for (var usingIndex = 0; usingIndex < canonical.Count; usingIndex++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var current = canonical[usingIndex];
            var isLast = usingIndex == canonical.Count - 1;
            var requiresSeparatingLineBreak = isLast == false
                                              && UsingTrailingTriviaBuilder.RequiresSeparatingLineBreak(current.GetTrailingTrivia(),
                                                                                                        canonical[usingIndex + 1].GetLeadingTrivia(),
                                                                                                        UsingGrouping.AreInSameGroup(current, canonical[usingIndex + 1]) == false);
            var trailingTrivia = UsingTrailingTriviaBuilder.CreateTrailingTrivia(current, isLast, requiresSeparatingLineBreak, originalBlockTerminalTrivia, endOfLine);

            if (usingIndex == 0)
            {
                result.Add(current.WithLeadingTrivia(UsingLeadingTriviaBuilder.CreateLeadingTrivia(current, firstLeadingTriviaPrefix, startsNewGroup: false, isFirst: true, lineIndentation: null, endOfLine))
                                  .WithTrailingTrivia(trailingTrivia));

                continue;
            }

            // A directive that shared a line before the rebuild has no indentation of its own; once it
            // begins a line - behind a line break or behind its group separator - it is placed at the
            // scope's indentation instead
            var startsNewGroup = UsingGrouping.AreInSameGroup(canonical[usingIndex - 1], current) == false;
            var beginsLine = startsNewGroup || UsingTrailingTriviaBuilder.ContainsLineBreak(result[usingIndex - 1].GetTrailingTrivia());
            SyntaxTriviaList? lineIndentation = beginsLine && directivesStartingTheirLine.Contains(current) == false
                                                    ? scopeIndentation
                                                    : null;

            result.Add(current.WithLeadingTrivia(UsingLeadingTriviaBuilder.CreateLeadingTrivia(current, firstLeadingTriviaPrefix, startsNewGroup, isFirst: false, lineIndentation, endOfLine))
                              .WithTrailingTrivia(trailingTrivia));
        }

        return SyntaxFactory.List(result);
    }

    /// <summary>
    /// Separates directives that share a line in a block that cannot be reordered safely. The order, the
    /// blank lines and every leading trivia — which holds every preprocessor directive — stay as they
    /// are; only a directive whose successor would otherwise share its line, as decided by
    /// <see cref="UsingTrailingTriviaBuilder.RequiresSeparatingLineBreak"/>, has the whitespace at the end
    /// of its trailing trivia replaced by a line break, and its successor is placed at the scope's
    /// indentation
    /// </summary>
    /// <param name="usingDirectives">Using directives to separate</param>
    /// <param name="scopeIndentation">Indentation of the scope's using directives</param>
    /// <param name="endOfLine">Preferred end-of-line sequence</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The separated directives</returns>
    private static SyntaxList<UsingDirectiveSyntax> SeparateDirectivesSharingALine(SyntaxList<UsingDirectiveSyntax> usingDirectives,
                                                                                   SyntaxTriviaList scopeIndentation,
                                                                                   string endOfLine,
                                                                                   CancellationToken cancellationToken)
    {
        var result = new List<UsingDirectiveSyntax>();
        var followsInsertedLineBreak = false;

        for (var usingIndex = 0; usingIndex < usingDirectives.Count; usingIndex++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var current = usingDirectives[usingIndex];

            if (followsInsertedLineBreak)
            {
                current = current.WithLeadingTrivia(scopeIndentation.AddRange(current.GetLeadingTrivia()));
            }

            followsInsertedLineBreak = usingIndex < usingDirectives.Count - 1
                                       && UsingTrailingTriviaBuilder.RequiresSeparatingLineBreak(current.GetTrailingTrivia(),
                                                                                                 usingDirectives[usingIndex + 1].GetLeadingTrivia(),
                                                                                                 successorStartsNewGroup: false);

            if (followsInsertedLineBreak)
            {
                current = current.WithTrailingTrivia(UsingTrailingTriviaBuilder.CreateTrailingTrivia(current,
                                                                                                     isLast: false,
                                                                                                     requiresSeparatingLineBreak: true,
                                                                                                     SyntaxFactory.TriviaList(),
                                                                                                     endOfLine));
            }

            result.Add(current);
        }

        return SyntaxFactory.List(result);
    }

    /// <summary>
    /// Gets the indentation of a scope's using directives as trivia, see
    /// <see cref="UsingDirectiveOrderingUtilities.GetLineIndentation"/>
    /// </summary>
    /// <param name="firstUsingDirective">First using directive of the scope</param>
    /// <returns>The indentation trivia; empty when the line has no indentation</returns>
    private static SyntaxTriviaList GetScopeIndentation(UsingDirectiveSyntax firstUsingDirective)
    {
        var indentation = UsingDirectiveOrderingUtilities.GetLineIndentation(firstUsingDirective);

        return indentation.Length == 0
                   ? SyntaxFactory.TriviaList()
                   : SyntaxFactory.TriviaList(SyntaxFactory.Whitespace(indentation));
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