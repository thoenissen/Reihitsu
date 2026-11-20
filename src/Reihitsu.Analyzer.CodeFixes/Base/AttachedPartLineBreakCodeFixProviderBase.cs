using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.CodeFixes.Core;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.CodeFixes.Base;

/// <summary>
/// Base class for the code fixes that move an attached part — an index access (RH5115) or a null-forgiving operator
/// (RH5116) — onto the line of the token in front of it. The reported token is the part's first token; its gaps are the
/// gap in front of it and, for the <c>?</c> of a conditional element access, the gap between the <c>?</c> and the
/// <c>[</c>, as the attached-part predicate states them (<see cref="AttachedPartGapUtilities"/>). The fix is withheld when
/// a gap holds a comment, a directive, or disabled text. When the part is an attached part of a chain with at least one
/// link, the fix lays out that chain through the formatter; otherwise it deletes the gaps and changes nothing else. Fix All
/// applies the fixes of a document one after another in document order (<see cref="DocumentOrderFixAllProvider"/>), so a
/// part joined inside a chain that a later fix lays out reaches that fix already joined
/// </summary>
public abstract class AttachedPartLineBreakCodeFixProviderBase : ReihitsuCodeFixProviderBase
{
    #region Fields

    /// <summary>
    /// Analyzer that reports the diagnostics the code fix corrects
    /// </summary>
    private readonly DiagnosticAnalyzer _analyzer;

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    private readonly string _diagnosticId;

    /// <summary>
    /// Code fix title
    /// </summary>
    private readonly string _title;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="analyzer">Analyzer that reports the diagnostics the code fix corrects</param>
    /// <param name="diagnosticId">Diagnostic ID</param>
    /// <param name="title">Code fix title</param>
    protected AttachedPartLineBreakCodeFixProviderBase(DiagnosticAnalyzer analyzer, string diagnosticId, string title)
    {
        _analyzer = analyzer;
        _diagnosticId = diagnosticId;
        _title = title;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Gets the gaps of the part a token starts: the gap in front of the token and, when the token is the <c>?</c> of a
    /// conditional element access, the gap between it and the <c>[</c> behind it
    /// </summary>
    /// <param name="firstToken">The part's first token</param>
    /// <returns>The gaps that are not empty, each from the end of the token in front of it to the start of the token behind it</returns>
    private static List<TextSpan> GetGaps(SyntaxToken firstToken)
    {
        var gaps = new List<TextSpan>();

        AddGap(gaps, SyntaxTokenNavigationUtilities.GetPreviousToken(firstToken), firstToken);

        var nextToken = SyntaxTokenNavigationUtilities.GetNextToken(firstToken);

        if (AttachedPartGapUtilities.IsElementBindingOpenBracket(nextToken))
        {
            AddGap(gaps, firstToken, nextToken);
        }

        return gaps;
    }

    /// <summary>
    /// Adds the gap between two adjacent tokens, when it is not empty
    /// </summary>
    /// <param name="gaps">The gaps</param>
    /// <param name="previousToken">The token in front of the gap</param>
    /// <param name="token">The token behind the gap</param>
    private static void AddGap(List<TextSpan> gaps, SyntaxToken previousToken, SyntaxToken token)
    {
        if (token.SpanStart > previousToken.Span.End)
        {
            gaps.Add(TextSpan.FromBounds(previousToken.Span.End, token.SpanStart));
        }
    }

    /// <summary>
    /// Gets the outermost node of the chain with at least one link that the part a token starts is an attached part of
    /// (SHARED-CHAIN-001)
    /// </summary>
    /// <param name="firstToken">The part's first token</param>
    /// <returns>The outermost chain node, or <see langword="null"/> when the part belongs to no chain with a link</returns>
    private static SyntaxNode FindChainNodeWithLinks(SyntaxToken firstToken)
    {
        var partNode = firstToken.Parent is BracketedArgumentListSyntax argumentList
                           ? argumentList.Parent
                           : firstToken.Parent;

        if (partNode == null
            || FluentChain.IsChainNode(partNode) == false)
        {
            return null;
        }

        var chainNode = FluentChain.GetOutermostChainNode(partNode);

        return FluentChain.Create(chainNode) != null
                   ? chainNode
                   : null;
    }

    /// <summary>
    /// Deletes the gaps of a part, so that the part directly follows the token in front of it (SHARED-WS-003)
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="gaps">The gaps to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    private static async Task<Document> JoinAsync(Document document, List<TextSpan> gaps, CancellationToken cancellationToken)
    {
        var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
        var changes = new List<TextChange>(gaps.Count);

        foreach (var gap in gaps)
        {
            changes.Add(new TextChange(gap, string.Empty));
        }

        return document.WithText(sourceText.WithChanges(changes));
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [_diagnosticId];

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider()
    {
        return DocumentOrderFixAllProvider.Create(_analyzer);
    }

    #endregion // CodeFixProvider

    #region ReihitsuCodeFixProviderBase

    /// <inheritdoc/>
    protected override bool FormatsThroughFormatter => true;

    /// <inheritdoc/>
    protected sealed override async Task RegisterCodeFixesCoreAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return;
        }

        foreach (var diagnostic in context.Diagnostics)
        {
            var firstToken = root.FindToken(diagnostic.Location.SourceSpan.Start);

            if (AttachedPartGapUtilities.StartsAttachedPart(firstToken) == false)
            {
                continue;
            }

            var gaps = GetGaps(firstToken);

            if (gaps.Exists(gap => SyntaxNodeUtilities.SpanContainsCommentOrDirective(root, gap)))
            {
                continue;
            }

            var chainNode = FindChainNodeWithLinks(firstToken);

            if (chainNode == null)
            {
                context.RegisterCodeFix(CodeAction.Create(_title,
                                                          cancellationToken => JoinAsync(context.Document, gaps, cancellationToken),
                                                          GetType().Name),
                                        diagnostic);
            }
            else if (FluentChainCodeFixHelper.IsFormattingRefused(chainNode) == false)
            {
                context.RegisterCodeFix(CodeAction.Create(_title,
                                                          cancellationToken => FluentChainCodeFixHelper.FormatChainAsync(context.Document, chainNode, cancellationToken),
                                                          GetType().Name),
                                        diagnostic);
            }
        }
    }

    #endregion // ReihitsuCodeFixProviderBase
}