using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Formatter.Pipeline.UsingDirectives;

namespace Reihitsu.Analyzer.Core;

/// <summary>
/// Shared helpers for code fixes that only reorganize using directives within a single scope
/// </summary>
internal static class UsingDirectiveCodeFixUtilities
{
    #region Methods

    /// <summary>
    /// Reorganizes the using directives of a single scope without formatting unrelated nodes
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="scope">Compilation unit or namespace declaration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    internal static async Task<Document> OrganizeScopeUsingsAsync(Document document, SyntaxNode scope, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        var usingDirectives = UsingDirectiveOrderingUtilities.GetUsings(scope);

        if (UsingDirectiveOrderingSafety.CanSafelyReorder(usingDirectives) == false)
        {
            return document;
        }

        var organizedUsings = OrganizeUsings(usingDirectives, DetectEndOfLine(root), cancellationToken);
        var updatedScope = UsingDirectiveOrderingUtilities.WithUsings(scope, organizedUsings);
        var updatedRoot = root.ReplaceNode(scope, updatedScope);

        return document.WithSyntaxRoot(updatedRoot);
    }

    /// <summary>
    /// Determines whether two using directives belong to the same formatter group
    /// </summary>
    /// <param name="left">Left using directive</param>
    /// <param name="right">Right using directive</param>
    /// <returns><see langword="true"/> if both directives belong to the same group</returns>
    private static bool AreInSameGroup(UsingDirectiveSyntax left, UsingDirectiveSyntax right)
    {
        return GetUsingTypeOrder(left) == GetUsingTypeOrder(right)
               && string.Equals(GetRootNamespace(left), GetRootNamespace(right), StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Computes the canonical order for the given using directives
    /// </summary>
    /// <param name="usingDirectives">Using directives to order</param>
    /// <returns>The canonical order</returns>
    private static List<UsingDirectiveSyntax> ComputeCanonicalOrder(IReadOnlyList<UsingDirectiveSyntax> usingDirectives)
    {
        return usingDirectives.Select((usingDirective, directiveIndex) => new
                                                                          {
                                                                              UsingDirective = usingDirective,
                                                                              DirectiveIndex = directiveIndex,
                                                                          })
                              .OrderBy(obj => GetUsingTypeOrder(obj.UsingDirective))
                              .ThenBy(obj => GetNamespaceGroupOrderKey(obj.UsingDirective), StringComparer.OrdinalIgnoreCase)
                              .ThenBy(obj => GetSortKey(obj.UsingDirective), StringComparer.OrdinalIgnoreCase)
                              .ThenBy(obj => obj.DirectiveIndex)
                              .Select(obj => obj.UsingDirective)
                              .ToList();
    }

    /// <summary>
    /// Creates the leading trivia for a reordered using directive
    /// </summary>
    /// <param name="current">Current using directive</param>
    /// <param name="firstLeadingTriviaPrefix">Whitespace prefix from the first using directive</param>
    /// <param name="startsNewGroup"><see langword="true"/> if the using starts a new group</param>
    /// <param name="isFirst"><see langword="true"/> if the using is the first directive in the block</param>
    /// <param name="endOfLine">Preferred end-of-line sequence</param>
    /// <returns>The leading trivia to apply</returns>
    private static SyntaxTriviaList CreateLeadingTrivia(UsingDirectiveSyntax current,
                                                        SyntaxTriviaList firstLeadingTriviaPrefix,
                                                        bool startsNewGroup,
                                                        bool isFirst,
                                                        string endOfLine)
    {
        var leadingTrivia = current.GetLeadingTrivia();
        var firstSignificantTriviaIndex = GetFirstSignificantTriviaIndex(leadingTrivia);

        if (firstSignificantTriviaIndex < 0)
        {
            if (isFirst)
            {
                return firstLeadingTriviaPrefix;
            }

            var indentation = GetIndentationTrivia(leadingTrivia);

            return startsNewGroup
                       ? SyntaxFactory.TriviaList(SyntaxFactory.EndOfLine(endOfLine))
                                      .AddRange(indentation)
                       : indentation;
        }

        var significantLeadingTrivia = SyntaxFactory.TriviaList(leadingTrivia.Skip(firstSignificantTriviaIndex));

        if (isFirst)
        {
            return firstLeadingTriviaPrefix.AddRange(significantLeadingTrivia);
        }

        var indentationBeforeSignificantTrivia = GetIndentationTriviaBefore(leadingTrivia, firstSignificantTriviaIndex);
        var linePrefix = startsNewGroup
                             ? SyntaxFactory.TriviaList(SyntaxFactory.EndOfLine(endOfLine))
                             : SyntaxFactory.TriviaList();

        return linePrefix.AddRange(indentationBeforeSignificantTrivia)
                         .AddRange(significantLeadingTrivia);
    }

    /// <summary>
    /// Detects the predominant end-of-line sequence used by the node
    /// </summary>
    /// <param name="node">Node to inspect</param>
    /// <returns>The preferred end-of-line sequence</returns>
    private static string DetectEndOfLine(SyntaxNode node)
    {
        var endOfLines = node.DescendantTrivia(descendIntoTrivia: true)
                             .Where(static trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia))
                             .Select(static trivia => trivia.ToString())
                             .ToList();

        if (endOfLines.Count == 0)
        {
            return Environment.NewLine;
        }

        var counts = new Dictionary<string, int>(StringComparer.Ordinal);

        foreach (var endOfLine in endOfLines)
        {
            counts[endOfLine] = counts.TryGetValue(endOfLine, out var count)
                                    ? count + 1
                                    : 1;
        }

        var predominantCount = counts.Values.Max();

        foreach (var endOfLine in endOfLines)
        {
            if (counts[endOfLine] == predominantCount)
            {
                return endOfLine;
            }
        }

        return Environment.NewLine;
    }

    /// <summary>
    /// Gets the first index containing non-whitespace trivia
    /// </summary>
    /// <param name="triviaList">Trivia list</param>
    /// <returns>The index of the first significant trivia, or -1 when none exists</returns>
    private static int GetFirstSignificantTriviaIndex(SyntaxTriviaList triviaList)
    {
        for (var triviaIndex = 0; triviaIndex < triviaList.Count; triviaIndex++)
        {
            if (IsWhitespaceOrEndOfLineTrivia(triviaList[triviaIndex]) == false)
            {
                return triviaIndex;
            }
        }

        return -1;
    }

    /// <summary>
    /// Extracts indentation whitespace from the end of a trivia list
    /// </summary>
    /// <param name="leadingTrivia">Leading trivia to inspect</param>
    /// <returns>Trivia list containing only indentation whitespace</returns>
    private static SyntaxTriviaList GetIndentationTrivia(SyntaxTriviaList leadingTrivia)
    {
        var result = new List<SyntaxTrivia>();

        for (var triviaIndex = leadingTrivia.Count - 1; triviaIndex >= 0; triviaIndex--)
        {
            if (leadingTrivia[triviaIndex].IsKind(SyntaxKind.WhitespaceTrivia))
            {
                result.Insert(0, leadingTrivia[triviaIndex]);
            }
            else
            {
                break;
            }
        }

        return SyntaxFactory.TriviaList(result);
    }

    /// <summary>
    /// Gets the indentation whitespace that appears immediately before the given trivia index
    /// </summary>
    /// <param name="leadingTrivia">Leading trivia</param>
    /// <param name="significantTriviaIndex">Index of the first significant trivia</param>
    /// <returns>Trivia list containing only the indentation whitespace</returns>
    private static SyntaxTriviaList GetIndentationTriviaBefore(SyntaxTriviaList leadingTrivia, int significantTriviaIndex)
    {
        var result = new List<SyntaxTrivia>();

        for (var triviaIndex = significantTriviaIndex - 1; triviaIndex >= 0; triviaIndex--)
        {
            if (leadingTrivia[triviaIndex].IsKind(SyntaxKind.WhitespaceTrivia))
            {
                result.Insert(0, leadingTrivia[triviaIndex]);

                continue;
            }

            if (leadingTrivia[triviaIndex].IsKind(SyntaxKind.EndOfLineTrivia))
            {
                break;
            }

            result.Clear();

            break;
        }

        return SyntaxFactory.TriviaList(result);
    }

    /// <summary>
    /// Gets the namespace ordering key for a using directive
    /// </summary>
    /// <param name="usingDirective">Using directive</param>
    /// <returns>The ordering key</returns>
    private static string GetNamespaceGroupOrderKey(UsingDirectiveSyntax usingDirective)
    {
        var rootNamespace = GetRootNamespace(usingDirective);

        return string.Equals(rootNamespace, "System", StringComparison.OrdinalIgnoreCase)
                   ? string.Empty
                   : rootNamespace;
    }

    /// <summary>
    /// Gets the root namespace segment for a using directive
    /// </summary>
    /// <param name="usingDirective">Using directive</param>
    /// <returns>The first namespace segment, or an empty string</returns>
    private static string GetRootNamespace(UsingDirectiveSyntax usingDirective)
    {
        var name = usingDirective.Name?.ToString() ?? string.Empty;
        var dotIndex = name.IndexOf('.');

        return dotIndex >= 0 ? name.Substring(0, dotIndex) : name;
    }

    /// <summary>
    /// Gets a stable sort key for a using directive
    /// </summary>
    /// <param name="usingDirective">Using directive</param>
    /// <returns>The string key to sort by</returns>
    private static string GetSortKey(UsingDirectiveSyntax usingDirective)
    {
        if (usingDirective.Alias != null)
        {
            return $"{usingDirective.Alias.Name}={usingDirective.Name}";
        }

        return usingDirective.Name?.ToString() ?? string.Empty;
    }

    /// <summary>
    /// Gets the using-type ordering slot
    /// </summary>
    /// <param name="usingDirective">Using directive</param>
    /// <returns>The using-type order</returns>
    private static int GetUsingTypeOrder(UsingDirectiveSyntax usingDirective)
    {
        if (usingDirective.StaticKeyword.IsKind(SyntaxKind.StaticKeyword))
        {
            return 1;
        }

        if (usingDirective.Alias != null)
        {
            return 2;
        }

        return 0;
    }

    /// <summary>
    /// Gets the whitespace-only prefix from the start of the trivia list
    /// </summary>
    /// <param name="triviaList">Trivia list</param>
    /// <returns>The whitespace-only prefix</returns>
    private static SyntaxTriviaList GetWhitespacePrefix(SyntaxTriviaList triviaList)
    {
        var result = new List<SyntaxTrivia>();

        foreach (var trivia in triviaList)
        {
            if (IsWhitespaceOrEndOfLineTrivia(trivia) == false)
            {
                break;
            }

            result.Add(trivia);
        }

        return SyntaxFactory.TriviaList(result);
    }

    /// <summary>
    /// Determines whether the trivia is whitespace or an end-of-line marker
    /// </summary>
    /// <param name="trivia">Trivia</param>
    /// <returns><see langword="true"/> if the trivia is whitespace-only</returns>
    private static bool IsWhitespaceOrEndOfLineTrivia(SyntaxTrivia trivia)
    {
        return trivia.IsKind(SyntaxKind.WhitespaceTrivia)
               || trivia.IsKind(SyntaxKind.EndOfLineTrivia);
    }

    /// <summary>
    /// Organizes the provided using directives into grouped canonical order
    /// </summary>
    /// <param name="usingDirectives">Using directives to organize</param>
    /// <param name="endOfLine">Preferred end-of-line sequence</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The organized directives</returns>
    private static SyntaxList<UsingDirectiveSyntax> OrganizeUsings(SyntaxList<UsingDirectiveSyntax> usingDirectives,
                                                                   string endOfLine,
                                                                   CancellationToken cancellationToken)
    {
        var orderedGlobalUsings = OrganizeSubset(usingDirectives.Where(UsingDirectiveOrderingUtilities.IsGlobalUsing).ToList(), endOfLine, cancellationToken);
        var orderedLocalUsings = OrganizeSubset(usingDirectives.Where(obj => UsingDirectiveOrderingUtilities.IsGlobalUsing(obj) == false).ToList(), endOfLine, cancellationToken);
        var organizedUsings = new List<UsingDirectiveSyntax>(usingDirectives.Count);
        var globalUsingIndex = 0;
        var localUsingIndex = 0;

        foreach (var usingDirective in usingDirectives)
        {
            organizedUsings.Add(UsingDirectiveOrderingUtilities.IsGlobalUsing(usingDirective)
                                    ? orderedGlobalUsings[globalUsingIndex++]
                                    : orderedLocalUsings[localUsingIndex++]);
        }

        return SyntaxFactory.List(organizedUsings);
    }

    /// <summary>
    /// Organizes a homogeneous global or local using subset
    /// </summary>
    /// <param name="usingDirectives">Using directives</param>
    /// <param name="endOfLine">Preferred end-of-line sequence</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The organized subset</returns>
    private static List<UsingDirectiveSyntax> OrganizeSubset(IReadOnlyList<UsingDirectiveSyntax> usingDirectives,
                                                             string endOfLine,
                                                             CancellationToken cancellationToken)
    {
        if (usingDirectives.Count <= 1)
        {
            return usingDirectives.ToList();
        }

        var firstLeadingTriviaPrefix = GetWhitespacePrefix(usingDirectives[0].GetLeadingTrivia());
        var canonical = ComputeCanonicalOrder(usingDirectives);
        var result = new List<UsingDirectiveSyntax>(canonical.Count);

        for (var usingIndex = 0; usingIndex < canonical.Count; usingIndex++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var current = canonical[usingIndex];

            if (usingIndex == 0)
            {
                result.Add(current.WithLeadingTrivia(CreateLeadingTrivia(current, firstLeadingTriviaPrefix, startsNewGroup: false, isFirst: true, endOfLine)));

                continue;
            }

            result.Add(current.WithLeadingTrivia(CreateLeadingTrivia(current,
                                                                     firstLeadingTriviaPrefix,
                                                                     startsNewGroup: AreInSameGroup(canonical[usingIndex - 1], current) == false,
                                                                     isFirst: false,
                                                                     endOfLine)));
        }

        return result;
    }

    #endregion // Methods
}