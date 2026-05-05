using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.UsingDirectives;

/// <summary>
/// Rewrites using directive scopes into canonical grouped order
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
    private static List<UsingDirectiveSyntax> ComputeCanonicalOrder(SyntaxList<UsingDirectiveSyntax> usingDirectives)
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
    /// Creates the leading trivia for the reordered using directive
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

    /// <summary>
    /// Organizes the provided using directives into grouped canonical order
    /// </summary>
    /// <param name="usingDirectives">Using directives to organize</param>
    /// <returns>The organized directives</returns>
    private SyntaxList<UsingDirectiveSyntax> OrganizeUsings(SyntaxList<UsingDirectiveSyntax> usingDirectives)
    {
        if (UsingDirectiveOrderingSafety.CanSafelyReorder(usingDirectives) == false)
        {
            return usingDirectives;
        }

        var firstLeadingTriviaPrefix = GetWhitespacePrefix(usingDirectives.First().GetLeadingTrivia());
        var canonical = ComputeCanonicalOrder(usingDirectives);
        var result = new List<UsingDirectiveSyntax>();

        for (var usingIndex = 0; usingIndex < canonical.Count; usingIndex++)
        {
            _cancellationToken.ThrowIfCancellationRequested();

            var current = canonical[usingIndex];

            if (usingIndex == 0)
            {
                result.Add(current.WithLeadingTrivia(CreateLeadingTrivia(current, firstLeadingTriviaPrefix, startsNewGroup: false, isFirst: true, _endOfLine)));

                continue;
            }

            result.Add(current.WithLeadingTrivia(CreateLeadingTrivia(current,
                                                                     firstLeadingTriviaPrefix,
                                                                     startsNewGroup: AreInSameGroup(canonical[usingIndex - 1], current) == false,
                                                                     isFirst: false,
                                                                     _endOfLine)));
        }

        return SyntaxFactory.List(result);
    }

    #endregion // Methods

    #region CSharpSyntaxRewriter

    /// <inheritdoc/>
    public override SyntaxNode VisitCompilationUnit(CompilationUnitSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (CompilationUnitSyntax)base.VisitCompilationUnit(node);

        if (node == null || node.Usings.Count < 2)
        {
            return node;
        }

        return (CompilationUnitSyntax)WithUsings(node, OrganizeUsings(node.Usings));
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

        return (FileScopedNamespaceDeclarationSyntax)WithUsings(node, OrganizeUsings(node.Usings));
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

        return (NamespaceDeclarationSyntax)WithUsings(node, OrganizeUsings(node.Usings));
    }

    #endregion // CSharpSyntaxRewriter
}