using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Core;
using Reihitsu.Formatter.Utilities;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Code fix provider for <see cref="RH5011BreakStatementsShouldBeFollowedByABlankLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5011BreakStatementsShouldBeFollowedByABlankLineCodeFixProvider))]
public class RH5011BreakStatementsShouldBeFollowedByABlankLineCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Finds the trivia that ends the line on which the comments behind the <c>break</c> statement's semicolon end: the first
    /// trivia in the gap up to the next token that ends its line (<see cref="SyntaxTriviaUtilities.EndsLine"/>). A line break
    /// inside the text of a multi-line comment does not end that line, so a comment that spans the end of the <c>break</c>
    /// line moves it down to the line on which the comment ends (RH5011-007)
    /// </summary>
    /// <param name="breakStatement">Break statement</param>
    /// <param name="nextToken">The token behind the break statement</param>
    /// <returns>The trivia that ends that line, or a trivia of kind <see cref="SyntaxKind.None"/> when no line break outside a comment stands in the gap</returns>
    private static SyntaxTrivia FindLineEndingTrivia(BreakStatementSyntax breakStatement, SyntaxToken nextToken)
    {
        return breakStatement.SemicolonToken.TrailingTrivia
                             .Concat(nextToken.LeadingTrivia)
                             .FirstOrDefault(SyntaxTriviaUtilities.EndsLine);
    }

    /// <summary>
    /// Applies the code fix. When a line break outside any comment follows the <c>break</c> statement, one empty line is
    /// inserted directly below the line that break ends, with that line's line break (RH5011-004, RH5011-007); when the next
    /// statement starts on the <c>break</c> line, it is moved onto a line of its own below an empty line
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="breakStatement">Break statement to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, BreakStatementSyntax breakStatement, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
        var breakLine = sourceText.Lines.GetLineFromPosition(breakStatement.SpanStart);
        var nextToken = breakStatement.SemicolonToken.GetNextToken(includeZeroWidth: true, includeSkipped: true);
        var lineEndingTrivia = FindLineEndingTrivia(breakStatement, nextToken);

        if (lineEndingTrivia.IsKind(SyntaxKind.None) == false)
        {
            var insertPosition = lineEndingTrivia.FullSpan.End;
            var endedLine = sourceText.Lines.GetLineFromPosition(insertPosition - 1);
            var lineBreak = sourceText.ToString(TextSpan.FromBounds(endedLine.End, endedLine.EndIncludingLineBreak));

            return document.WithText(sourceText.Replace(new TextSpan(insertPosition, 0), lineBreak));
        }

        if (nextToken.IsKind(SyntaxKind.None)
            || sourceText.Lines.GetLineFromPosition(nextToken.SpanStart).LineNumber != breakLine.LineNumber)
        {
            return document;
        }

        var nextStatement = nextToken.Parent?.FirstAncestorOrSelf<StatementSyntax>();

        if (nextStatement == null)
        {
            return document;
        }

        var endOfLine = ReihitsuFormatterHelpers.DetectEndOfLine(root);
        var indentation = FormattingTextAnalysisUtilities.GetLeadingWhitespace(FormattingTextAnalysisUtilities.GetLineText(sourceText, breakLine));
        var gap = TextSpan.FromBounds(breakStatement.SemicolonToken.Span.End, nextToken.SpanStart);

        return document.WithText(sourceText.Replace(gap, $"{endOfLine}{endOfLine}{indentation}"));
    }

    /// <summary>
    /// Determines whether the code fix can be offered. It is offered when a line break outside any comment follows the
    /// <c>break</c> statement, and when the next statement starts on the <c>break</c> line behind nothing but whitespace.
    /// When every line break in front of the next token lies inside a comment, that token starts behind the comment on its
    /// last line, and the fix is withheld (RH5011-007)
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <param name="breakStatement">Break statement</param>
    /// <returns><see langword="true"/> when the action can preserve every token and trivia item; otherwise, <see langword="false"/></returns>
    private static bool CanRegisterCodeFix(SyntaxNode root, BreakStatementSyntax breakStatement)
    {
        var nextToken = breakStatement.SemicolonToken.GetNextToken(includeZeroWidth: true, includeSkipped: true);

        if (nextToken.IsKind(SyntaxKind.None)
            || FindLineEndingTrivia(breakStatement, nextToken).IsKind(SyntaxKind.None) == false)
        {
            return true;
        }

        var lineSpan = root.SyntaxTree.GetLineSpan(TextSpan.FromBounds(breakStatement.SpanStart, nextToken.SpanStart));

        if (lineSpan.StartLinePosition.Line != lineSpan.EndLinePosition.Line)
        {
            return false;
        }

        var gap = TextSpan.FromBounds(breakStatement.SemicolonToken.Span.End, nextToken.SpanStart);

        return nextToken.Parent?.FirstAncestorOrSelf<StatementSyntax>() != null
               && root.SyntaxTree.GetDiagnostics().Any(diagnostic => diagnostic.Location.SourceSpan.IntersectsWith(gap)) == false
               && root.SyntaxTree.GetText()
                                 .ToString(gap)
                                 .All(static character => character is ' ' or '\t');
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH5011BreakStatementsShouldBeFollowedByABlankLineAnalyzer.DiagnosticId];

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

        foreach (var diagnostic in context.Diagnostics)
        {
            var breakStatement = root.FindNode(diagnostic.Location.SourceSpan).FirstAncestorOrSelf<BreakStatementSyntax>();

            if (breakStatement != null && CanRegisterCodeFix(root, breakStatement))
            {
                context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH5011Title,
                                                          token => ApplyCodeFixAsync(context.Document, breakStatement, token),
                                                          nameof(RH5011BreakStatementsShouldBeFollowedByABlankLineCodeFixProvider)),
                                        diagnostic);
            }
        }
    }

    #endregion // CodeFixProvider
}