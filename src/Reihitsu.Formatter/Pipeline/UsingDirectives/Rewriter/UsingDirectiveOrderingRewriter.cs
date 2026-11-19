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
    /// Organizes the provided using directives into grouped canonical order, with one blank line between two
    /// groups and none within a group other than one kept above a comment that leads a directive, where one
    /// stood in the source (FMT-USING-010). A block that cannot be
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
        var (sourceIndices, directivesWithBlankLineAboveComment) = GetSourceLayout(usingDirectives);

        var canonical = UsingGrouping.ComputeCanonicalOrder(usingDirectives);

        if (ReferenceEquals(canonical[0], originalFirst) == false)
        {
            DetachHeaderOfOriginalFirst(originalFirst,
                                        scopeIndentation,
                                        ref firstLeadingTriviaPrefix,
                                        ref canonical,
                                        directivesStartingTheirLine,
                                        sourceIndices,
                                        directivesWithBlankLineAboveComment);
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
                                                                                                        UsingGrouping.AreInSameGroup(current, canonical[usingIndex + 1]) == false,
                                                                                                        sourceIndices[canonical[usingIndex + 1]] == sourceIndices[current] + 1);
            var trailingTrivia = UsingTrailingTriviaBuilder.CreateTrailingTrivia(current, isLast, requiresSeparatingLineBreak, originalBlockTerminalTrivia, endOfLine);

            if (usingIndex == 0)
            {
                result.Add(current.WithLeadingTrivia(UsingLeadingTriviaBuilder.CreateLeadingTrivia(current, firstLeadingTriviaPrefix, startsNewGroup: false, isFirst: true, lineIndentation: null, endOfLine))
                                  .WithTrailingTrivia(trailingTrivia));

                continue;
            }

            var startsNewGroup = UsingGrouping.AreInSameGroup(canonical[usingIndex - 1], current) == false;
            var beginsLine = startsNewGroup || UsingTrailingTriviaBuilder.ContainsLineBreak(result[usingIndex - 1].GetTrailingTrivia());
            var leadingTrivia = CreateLeadingTriviaOfLaterDirective(current,
                                                                    firstLeadingTriviaPrefix,
                                                                    startsNewGroup,
                                                                    beginsLine,
                                                                    directivesStartingTheirLine.Contains(current) ? null : scopeIndentation,
                                                                    directivesWithBlankLineAboveComment.Contains(current),
                                                                    endOfLine);

            result.Add(current.WithLeadingTrivia(leadingTrivia)
                              .WithTrailingTrivia(trailingTrivia));
        }

        return SyntaxFactory.List(result);
    }

    /// <summary>
    /// Creates the leading trivia of a directive that is not the first of the organized block. A directive that shared a
    /// line before the rebuild is placed at the scope's indentation once it begins a line; a directive that continues its
    /// group gets no blank line other than the one kept above the comment that leads it (FMT-USING-010); and a line break
    /// that puts a leading comment on a line of its own behind the directive in front is kept
    /// </summary>
    /// <param name="current">The directive</param>
    /// <param name="firstLeadingTriviaPrefix">The whitespace prefix of the block's first leading trivia</param>
    /// <param name="startsNewGroup">Whether the directive starts a new group</param>
    /// <param name="beginsLine">Whether the directive begins a line, behind a line break or behind its group separator</param>
    /// <param name="scopeIndentation">The scope's indentation, or <see langword="null"/> when the directive started its line in the source</param>
    /// <param name="hasBlankLineAboveComment">Whether a blank line stood directly above the comment that leads the directive in the source</param>
    /// <param name="endOfLine">Preferred end-of-line sequence</param>
    /// <returns>The leading trivia of the directive</returns>
    private static SyntaxTriviaList CreateLeadingTriviaOfLaterDirective(UsingDirectiveSyntax current,
                                                                        SyntaxTriviaList firstLeadingTriviaPrefix,
                                                                        bool startsNewGroup,
                                                                        bool beginsLine,
                                                                        SyntaxTriviaList? scopeIndentation,
                                                                        bool hasBlankLineAboveComment,
                                                                        string endOfLine)
    {
        // A directive that shared a line before the rebuild has no indentation of its own; once it
        // begins a line - behind a line break or behind its group separator - it is placed at the
        // scope's indentation instead
        var lineIndentation = beginsLine
                                  ? scopeIndentation
                                  : null;

        var leadingTrivia = UsingLeadingTriviaBuilder.CreateLeadingTrivia(current, firstLeadingTriviaPrefix, startsNewGroup, isFirst: false, lineIndentation, endOfLine);

        // A directive that continues its group gets no blank line, except the one kept above the comment that
        // leads it when a blank line stood directly above that comment in the source - however many stood
        // there, exactly one is written (FMT-USING-010)
        if (startsNewGroup == false
            && hasBlankLineAboveComment)
        {
            leadingTrivia = leadingTrivia.Insert(0, SyntaxFactory.EndOfLine(endOfLine));
        }

        // The rebuilt leading trivia drops the layout in front of its first comment, because the line in front
        // normally already ends. When it does not, a line break in that layout is the one that puts the comment
        // on a line of its own behind the directive in front - the documentation-comment phase writes it there
        // when it moves a delimited documentation comment written behind that directive - so it is kept
        if (beginsLine == false
            && UsingLeadingTriviaBuilder.HasLineBreakBeforeSignificantTrivia(current.GetLeadingTrivia()))
        {
            leadingTrivia = leadingTrivia.Insert(0, SyntaxFactory.EndOfLine(endOfLine));
        }

        return leadingTrivia;
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
                                                                                                 successorStartsNewGroup: false,
                                                                                                 wereSourceNeighbors: true);

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
    /// Gets, for every directive of the block, its index in the source, and the directives that are led by a comment with
    /// a blank line directly above that comment in the source (FMT-USING-010)
    /// </summary>
    /// <param name="usingDirectives">Using directives as written</param>
    /// <returns>The source index of every directive, and the directives with a blank line above their leading comment</returns>
    private static (Dictionary<UsingDirectiveSyntax, int> SourceIndices, HashSet<UsingDirectiveSyntax> DirectivesWithBlankLineAboveComment) GetSourceLayout(SyntaxList<UsingDirectiveSyntax> usingDirectives)
    {
        var sourceIndices = new Dictionary<UsingDirectiveSyntax, int>();
        var directivesWithBlankLineAboveComment = new HashSet<UsingDirectiveSyntax>();

        for (var sourceIndex = 0; sourceIndex < usingDirectives.Count; sourceIndex++)
        {
            sourceIndices[usingDirectives[sourceIndex]] = sourceIndex;

            if (sourceIndex > 0
                && UsingDirectiveOrderingUtilities.IsLedByComment(usingDirectives[sourceIndex])
                && UsingDirectiveOrderingUtilities.HasBlankLineAbove(usingDirectives[sourceIndex - 1], usingDirectives[sourceIndex]))
            {
                directivesWithBlankLineAboveComment.Add(usingDirectives[sourceIndex]);
            }
        }

        return (sourceIndices, directivesWithBlankLineAboveComment);
    }

    /// <summary>
    /// Detaches the header trivia of the original first directive when another directive takes the first position: the
    /// header joins the leading trivia of the first position, and the original first directive moves away with the rest of
    /// its leading trivia, keeping its source index and whether it starts its line or has a blank line above its comment
    /// </summary>
    /// <param name="originalFirst">The first directive as written</param>
    /// <param name="scopeIndentation">The indentation of the scope that holds the directives</param>
    /// <param name="firstLeadingTriviaPrefix">The leading trivia that the first position starts with</param>
    /// <param name="canonical">The directives in canonical order</param>
    /// <param name="directivesStartingTheirLine">The directives that start their line in the source</param>
    /// <param name="sourceIndices">The source index of every directive</param>
    /// <param name="directivesWithBlankLineAboveComment">The directives with a blank line above their leading comment</param>
    private static void DetachHeaderOfOriginalFirst(UsingDirectiveSyntax originalFirst,
                                                    SyntaxTriviaList scopeIndentation,
                                                    ref SyntaxTriviaList firstLeadingTriviaPrefix,
                                                    ref List<UsingDirectiveSyntax> canonical,
                                                    HashSet<UsingDirectiveSyntax> directivesStartingTheirLine,
                                                    Dictionary<UsingDirectiveSyntax, int> sourceIndices,
                                                    HashSet<UsingDirectiveSyntax> directivesWithBlankLineAboveComment)
    {
        var (header, remainder) = UsingLeadingTriviaBuilder.SplitOriginalFirstHeaderTrivia(originalFirst.GetLeadingTrivia());

        if (header.Count > 0)
        {
            firstLeadingTriviaPrefix = firstLeadingTriviaPrefix.AddRange(header);

            // A header that ends its line leaves the directive taking the first position on a line of its
            // own, which starts at the whitespace of the original first directive's line (FMT-USING-011);
            // that whitespace stays in the remainder for the directive moving away from the top
            if (SyntaxTriviaUtilities.EndsLine(header.Last()))
            {
                firstLeadingTriviaPrefix = firstLeadingTriviaPrefix.AddRange(scopeIndentation);
            }

            var detachedFirst = originalFirst.WithLeadingTrivia(remainder);

            if (directivesStartingTheirLine.Contains(originalFirst))
            {
                directivesStartingTheirLine.Add(detachedFirst);
            }

            sourceIndices[detachedFirst] = sourceIndices[originalFirst];

            // The header ends with a blank line, so when a comment is left to lead the directive that moves
            // away from the top, that blank line stood directly above the comment
            if (UsingDirectiveOrderingUtilities.IsLedByComment(detachedFirst))
            {
                directivesWithBlankLineAboveComment.Add(detachedFirst);
            }

            canonical = canonical.ConvertAll(current => ReferenceEquals(current, originalFirst) ? detachedFirst : current);
        }
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