using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;
using Reihitsu.Core;
using Reihitsu.Formatter;

namespace Reihitsu.Analyzer.CodeFixes.Base;

/// <summary>
/// Code fix provider base class for rules based on <see cref="StatementShouldBePrecededByABlankLineAnalyzerBase{TStatement}"/>
/// </summary>
public abstract class StatementShouldBePrecededByABlankLineCodeFixProviderBase : CodeFixProvider
{
    #region Fields

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    private readonly string _diagnosticId;

    /// <summary>
    /// Title
    /// </summary>
    private readonly string _title;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="diagnosticId">Diagnostic ID</param>
    /// <param name="title">Title</param>
    private protected StatementShouldBePrecededByABlankLineCodeFixProviderBase(string diagnosticId, string title)
    {
        _diagnosticId = diagnosticId;
        _title = title;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Applying code fix by inserting a blank line before the statement
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="token">Token at the diagnostic location</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, SyntaxToken token, CancellationToken cancellationToken)
    {
        var syntaxRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (syntaxRoot != null)
        {
            var endOfLine = ReihitsuFormatterHelpers.DetectEndOfLine(syntaxRoot);
            var previousToken = token.GetPreviousToken();

            if (previousToken.IsKind(SyntaxKind.None) == false
                && TokenGapAnalysis.Between(previousToken, token).HasLineBreak == false)
            {
                var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
                var indentation = GetIndentation(previousToken, sourceText);
                var targetLeadingTrivia = token.LeadingTrivia;
                var suffixStart = 0;

                while (suffixStart < targetLeadingTrivia.Count && targetLeadingTrivia[suffixStart].IsKind(SyntaxKind.WhitespaceTrivia))
                {
                    suffixStart++;
                }

                var newLeadingTriviaItems = new List<SyntaxTrivia>(targetLeadingTrivia.Count - suffixStart + 3)
                                            {
                                                SyntaxFactory.EndOfLine(endOfLine),
                                                SyntaxFactory.EndOfLine(endOfLine)
                                            };

                if (indentation.Length > 0)
                {
                    newLeadingTriviaItems.Add(SyntaxFactory.Whitespace(indentation));
                }

                for (var triviaIndex = suffixStart; triviaIndex < targetLeadingTrivia.Count; triviaIndex++)
                {
                    newLeadingTriviaItems.Add(targetLeadingTrivia[triviaIndex]);
                }

                var previousTrailingTrivia = previousToken.TrailingTrivia;

                while (previousTrailingTrivia.Count > 0
                       && previousTrailingTrivia[previousTrailingTrivia.Count - 1].IsKind(SyntaxKind.WhitespaceTrivia))
                {
                    previousTrailingTrivia = previousTrailingTrivia.RemoveAt(previousTrailingTrivia.Count - 1);
                }

                var newPreviousToken = previousToken.WithTrailingTrivia(previousTrailingTrivia);
                var updatedToken = token.WithLeadingTrivia(SyntaxFactory.TriviaList(newLeadingTriviaItems));

                syntaxRoot = syntaxRoot.ReplaceTokens([previousToken, token],
                                                      (originalToken, _) => originalToken == previousToken
                                                                                ? newPreviousToken
                                                                                : updatedToken);

                return document.WithSyntaxRoot(syntaxRoot);
            }

            var leadingTrivia = token.LeadingTrivia;
            var newLeadingTrivia = leadingTrivia.Insert(0, SyntaxFactory.EndOfLine(endOfLine));
            var newToken = token.WithLeadingTrivia(newLeadingTrivia);

            syntaxRoot = syntaxRoot.ReplaceToken(token, newToken);

            document = document.WithSyntaxRoot(syntaxRoot);
        }

        return document;
    }

    /// <summary>
    /// Gets the indentation of the statement that precedes a same-line diagnostic target
    /// </summary>
    /// <param name="previousToken">Token before the diagnostic target</param>
    /// <param name="sourceText">Document source text</param>
    /// <returns>Indentation to apply to the target after moving it to its own line</returns>
    private static string GetIndentation(SyntaxToken previousToken, SourceText sourceText)
    {
        var previousStatement = previousToken.Parent?.FirstAncestorOrSelf<StatementSyntax>();
        var position = previousStatement?.SpanStart ?? previousToken.SpanStart;
        var line = sourceText.Lines.GetLineFromPosition(position);

        return FormattingTextAnalysisUtilities.GetLeadingWhitespace(FormattingTextAnalysisUtilities.GetLineText(sourceText, line));
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

        if (root != null)
        {
            foreach (var diagnostic in context.Diagnostics)
            {
                var token = root.FindToken(diagnostic.Location.SourceSpan.Start);

                context.RegisterCodeFix(CodeAction.Create(_title,
                                                          cancellationToken => ApplyCodeFixAsync(context.Document, token, cancellationToken),
                                                          GetType().Name),
                                        diagnostic);
            }
        }
    }

    #endregion // CodeFixProvider
}