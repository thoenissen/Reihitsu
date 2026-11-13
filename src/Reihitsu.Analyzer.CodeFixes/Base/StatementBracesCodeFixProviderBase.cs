using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Core;

namespace Reihitsu.Analyzer.CodeFixes.Base;

/// <summary>
/// Base class for code fixes that wrap the body of an <c>if</c> or <c>else</c> in braces. The fix edits the text around
/// the body and changes nothing else (SHARED-BRACE-002): the header, the other body, and the body's own lines stay as
/// written, and it is withheld where the body cannot be wrapped as written (SHARED-BRACE-003)
/// </summary>
public abstract class StatementBracesCodeFixProviderBase : CodeFixProvider
{
    #region Constants

    /// <summary>
    /// One indentation level, written in front of a body that moves off the line of its header
    /// </summary>
    private const string IndentationLevel = "    ";

    #endregion // Constants

    #region Fields

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
    /// <param name="diagnosticId">Diagnostic ID</param>
    /// <param name="title">Code fix title</param>
    protected StatementBracesCodeFixProviderBase(string diagnosticId, string title)
    {
        _diagnosticId = diagnosticId;
        _title = title;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Applies the code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="diagnosticSpan">Diagnostic span</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, TextSpan diagnosticSpan, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);

        if (root == null
            || TryCreateChanges(root, text, diagnosticSpan, out var changes) == false)
        {
            return document;
        }

        return document.WithText(text.WithChanges(changes));
    }

    /// <summary>
    /// Creates the text changes that wrap the reported body in braces (SHARED-BRACE-002), or refuses where
    /// SHARED-BRACE-003 withholds the fix
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <param name="text">Source text</param>
    /// <param name="diagnosticSpan">The diagnostic's span</param>
    /// <param name="changes">The text changes</param>
    /// <returns><see langword="true"/> when the fix applies; otherwise, <see langword="false"/></returns>
    private static bool TryCreateChanges(SyntaxNode root, SourceText text, TextSpan diagnosticSpan, out List<TextChange> changes)
    {
        changes = null;

        var body = root.FindNode(diagnosticSpan).FirstAncestorOrSelf<StatementSyntax>();
        SyntaxToken ownerKeyword;
        SyntaxToken headerEnd;

        switch (body?.Parent)
        {
            case IfStatementSyntax ifStatement:
                {
                    ownerKeyword = ifStatement.IfKeyword;
                    headerEnd = ifStatement.CloseParenToken;
                }
                break;

            case ElseClauseSyntax { Parent: IfStatementSyntax } elseClause:
                {
                    ownerKeyword = elseClause.ElseKeyword;
                    headerEnd = elseClause.ElseKeyword;
                }
                break;

            default:
                {
                    return false;
                }
        }

        var lines = text.Lines;
        var anchor = FindLineAnchor(ownerKeyword);
        var anchorLine = lines.GetLineFromPosition(anchor.SpanStart);
        var indentation = text.ToString(TextSpan.FromBounds(anchorLine.Start, anchor.SpanStart));

        // Other code in front of the owner, or a tab in front of it (SHARED-BRACE-003)
        if (indentation.Any(character => character != ' '))
        {
            return false;
        }

        var firstToken = body.GetFirstToken();
        var lastToken = body.GetLastToken();
        var gapTrivia = headerEnd.TrailingTrivia.Concat(firstToken.LeadingTrivia).ToList();

        // A directive or disabled text between the header and the body (SHARED-BRACE-003)
        if (gapTrivia.Any(trivia => trivia.IsDirective || trivia.IsKind(SyntaxKind.DisabledTextTrivia)))
        {
            return false;
        }

        var trailingTrivia = lastToken.TrailingTrivia;
        var trailingLineBreakIndex = trailingTrivia.IndexOf(SyntaxKind.EndOfLineTrivia);
        var bodyLineEnd = trailingTrivia.Take(trailingLineBreakIndex < 0 ? trailingTrivia.Count : trailingLineBreakIndex)
                                        .Where(trivia => trivia.IsKind(SyntaxKind.WhitespaceTrivia) == false)
                                        .Select(trivia => trivia.Span.End)
                                        .DefaultIfEmpty(lastToken.Span.End)
                                        .Max();
        var bodyLastLine = lines.GetLineFromPosition(bodyLineEnd);
        var headerLineBreak = gapTrivia.FirstOrDefault(trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia));
        var isOnHeaderLine = headerLineBreak.IsKind(SyntaxKind.None);

        // A body on the line of its header that spans several lines (SHARED-BRACE-003)
        if (isOnHeaderLine
            && lines.GetLineFromPosition(firstToken.SpanStart).LineNumber != bodyLastLine.LineNumber)
        {
            return false;
        }

        var endOfLine = LineEndingUtilities.DetectEndOfLine(root);

        // Another line ending in the code the fix moves, through the line break that ends the body's last line (SHARED-BRACE-003)
        if (LineEndingUtilities.HoldsOtherLineBreak(text, TextSpan.FromBounds(headerEnd.Span.End, bodyLastLine.EndIncludingLineBreak), endOfLine))
        {
            return false;
        }

        var openingChange = isOnHeaderLine
                                ? CreateOpeningChangeOnHeaderLine(text, headerEnd, firstToken, indentation, endOfLine)
                                : CreateOpeningChangeBelowHeader(text, headerLineBreak, firstToken, indentation, endOfLine);

        if (TryCreateClosingChange(text, body, lastToken, trailingLineBreakIndex, indentation, endOfLine, out var closingChange) == false)
        {
            return false;
        }

        changes = [openingChange, closingChange];

        return true;
    }

    /// <summary>
    /// Finds the first token of the line on which the owner of the body starts, as far as only the tokens that may stand
    /// in front of it lead there: the <c>else</c> of an <c>else if</c>, and the closing brace of the other body in front of
    /// an <c>else</c>, each separated from the next token by nothing but whitespace on the same line. The braces of the fix
    /// take the column of this token; anything else in front of it on its line withholds the fix (SHARED-BRACE-003)
    /// </summary>
    /// <param name="ownerKeyword">The <c>if</c> or <c>else</c> keyword of the owner</param>
    /// <returns>The anchor token</returns>
    private static SyntaxToken FindLineAnchor(SyntaxToken ownerKeyword)
    {
        var anchor = ownerKeyword;

        if (anchor.Parent is IfStatementSyntax { Parent: ElseClauseSyntax elseClause }
            && IsSeparatedByWhitespaceOnly(elseClause.ElseKeyword, anchor))
        {
            anchor = elseClause.ElseKeyword;
        }

        if (anchor.Parent is ElseClauseSyntax { Parent: IfStatementSyntax { Statement: BlockSyntax otherBody } }
            && IsSeparatedByWhitespaceOnly(otherBody.CloseBraceToken, anchor))
        {
            anchor = otherBody.CloseBraceToken;
        }

        return anchor;
    }

    /// <summary>
    /// Determines whether two adjacent tokens stand on one line with nothing but whitespace between them
    /// </summary>
    /// <param name="previous">The previous token</param>
    /// <param name="next">The next token</param>
    /// <returns><see langword="true"/> when only whitespace separates them; otherwise, <see langword="false"/></returns>
    private static bool IsSeparatedByWhitespaceOnly(SyntaxToken previous, SyntaxToken next)
    {
        return previous.GetNextToken() == next
               && previous.TrailingTrivia.Concat(next.LeadingTrivia).All(trivia => trivia.IsKind(SyntaxKind.WhitespaceTrivia));
    }

    /// <summary>
    /// Creates the change in front of a body on the line of its header: the comments behind the header stay there, the
    /// opening brace takes its own line, and the body moves onto the line below it, one indentation level deeper
    /// </summary>
    /// <param name="text">Source text</param>
    /// <param name="headerEnd">The token that ends the header</param>
    /// <param name="firstToken">The body's first token</param>
    /// <param name="indentation">The whitespace in front of the braces</param>
    /// <param name="endOfLine">The document's line ending</param>
    /// <returns>The text change</returns>
    private static TextChange CreateOpeningChangeOnHeaderLine(SourceText text, SyntaxToken headerEnd, SyntaxToken firstToken, string indentation, string endOfLine)
    {
        var gap = TextSpan.FromBounds(headerEnd.Span.End, firstToken.SpanStart);
        var headerTail = text.ToString(gap).TrimEnd(' ', '\t');

        return new TextChange(gap, $"{headerTail}{endOfLine}{indentation}{{{endOfLine}{indentation}{IndentationLevel}");
    }

    /// <summary>
    /// Creates the change in front of a body below its header: the blank lines directly below the header's line are
    /// removed, and the opening brace takes the line below it; every later line stays as written
    /// </summary>
    /// <param name="text">Source text</param>
    /// <param name="headerLineBreak">The line break that ends the header's line</param>
    /// <param name="firstToken">The body's first token</param>
    /// <param name="indentation">The whitespace in front of the braces</param>
    /// <param name="endOfLine">The document's line ending</param>
    /// <returns>The text change</returns>
    private static TextChange CreateOpeningChangeBelowHeader(SourceText text, SyntaxTrivia headerLineBreak, SyntaxToken firstToken, string indentation, string endOfLine)
    {
        var start = headerLineBreak.Span.End;
        var end = start;
        var line = text.Lines.GetLineFromPosition(start);

        while (line.End < firstToken.SpanStart
               && string.IsNullOrWhiteSpace(text.ToString(line.Span)))
        {
            end = line.EndIncludingLineBreak;
            line = text.Lines[line.LineNumber + 1];
        }

        return new TextChange(TextSpan.FromBounds(start, end), $"{indentation}{{{endOfLine}");
    }

    /// <summary>
    /// Creates the change behind the body. A body whose line ends behind it, and behind the comments and whitespace that
    /// follow it, gets the closing brace on the line below. Otherwise the closing brace takes the next line, and the code
    /// behind the body follows: the <c>else</c> of the same <c>if</c> statement behind the brace on its line, and any
    /// other code on the line below it
    /// </summary>
    /// <param name="text">Source text</param>
    /// <param name="body">The body</param>
    /// <param name="lastToken">The body's last token</param>
    /// <param name="trailingLineBreakIndex">The index of the line break in the trailing trivia of the last token, or a negative value</param>
    /// <param name="indentation">The whitespace in front of the braces</param>
    /// <param name="endOfLine">The document's line ending</param>
    /// <param name="change">The text change</param>
    /// <returns><see langword="true"/> when the change could be created; otherwise, <see langword="false"/></returns>
    private static bool TryCreateClosingChange(SourceText text, StatementSyntax body, SyntaxToken lastToken, int trailingLineBreakIndex, string indentation, string endOfLine, out TextChange change)
    {
        if (trailingLineBreakIndex >= 0)
        {
            change = new TextChange(new TextSpan(lastToken.TrailingTrivia[trailingLineBreakIndex].Span.End, 0), $"{indentation}}}{endOfLine}");

            return true;
        }

        var nextToken = lastToken.GetNextToken();

        if (nextToken.IsKind(SyntaxKind.None)
            || nextToken.IsKind(SyntaxKind.EndOfFileToken))
        {
            change = default;

            return false;
        }

        var tail = TextSpan.FromBounds(lastToken.Span.End, nextToken.SpanStart);
        var bodyTail = text.ToString(tail).TrimEnd(' ', '\t');
        var isElseOfSameStatement = body.Parent is IfStatementSyntax { Else: { } elseClause }
                                    && elseClause.ElseKeyword == nextToken;

        change = isElseOfSameStatement
                     ? new TextChange(tail, $"{bodyTail}{endOfLine}{indentation}}} ")
                     : new TextChange(tail, $"{bodyTail}{endOfLine}{indentation}}}{endOfLine}{indentation}");

        return true;
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [_diagnosticId];

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    /// <inheritdoc/>
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return;
        }

        var text = await context.Document.GetTextAsync(context.CancellationToken).ConfigureAwait(false);

        foreach (var diagnostic in context.Diagnostics)
        {
            if (TryCreateChanges(root, text, diagnostic.Location.SourceSpan, out _) == false)
            {
                continue;
            }

            context.RegisterCodeFix(CodeAction.Create(_title,
                                                      token => ApplyCodeFixAsync(context.Document, diagnostic.Location.SourceSpan, token),
                                                      GetType().Name),
                                    diagnostic);
        }
    }

    #endregion // CodeFixProvider
}