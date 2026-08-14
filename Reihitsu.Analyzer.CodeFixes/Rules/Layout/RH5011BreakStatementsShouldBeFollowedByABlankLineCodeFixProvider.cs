using System.Collections.Immutable;
using System.Composition;
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
    /// Applies the code fix
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

        if (nextToken.IsKind(SyntaxKind.None) == false
            && sourceText.Lines.GetLineFromPosition(nextToken.SpanStart).LineNumber == breakLine.LineNumber)
        {
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

        var lineBreak = breakLine.EndIncludingLineBreak > breakLine.End
                            ? sourceText.ToString(TextSpan.FromBounds(breakLine.End, breakLine.EndIncludingLineBreak))
                            : ReihitsuFormatterHelpers.DetectEndOfLine(root);

        return document.WithText(sourceText.Replace(new TextSpan(breakLine.EndIncludingLineBreak, 0), lineBreak));
    }

    /// <summary>
    /// Determines whether the gap after a same-line break statement contains only formatting trivia
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <param name="breakStatement">Break statement</param>
    /// <returns><see langword="true"/> when the action can preserve every token and trivia item; otherwise, <see langword="false"/></returns>
    private static bool CanRegisterCodeFix(SyntaxNode root, BreakStatementSyntax breakStatement)
    {
        var nextToken = breakStatement.SemicolonToken.GetNextToken(includeZeroWidth: true, includeSkipped: true);

        if (nextToken.IsKind(SyntaxKind.None))
        {
            return true;
        }

        var lineSpan = root.SyntaxTree.GetLineSpan(TextSpan.FromBounds(breakStatement.SpanStart, nextToken.SpanStart));

        if (lineSpan.StartLinePosition.Line != lineSpan.EndLinePosition.Line)
        {
            return true;
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