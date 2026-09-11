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

using Reihitsu.Analyzer.Base;
using Reihitsu.Core;
using Reihitsu.Formatter.Utilities;

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

        if (syntaxRoot == null)
        {
            return document;
        }

        var endOfLine = ReihitsuFormatterHelpers.DetectEndOfLine(syntaxRoot);
        var previousToken = token.GetPreviousToken();

        if (previousToken.IsKind(SyntaxKind.None) == false
            && TokenGapAnalysis.Between(previousToken, token).RequiredLineBreakCountForBlankLine == 2)
        {
            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);

            return ReplaceFormattingOnlyGap(document, syntaxRoot, sourceText, previousToken, token, endOfLine);
        }

        return InsertLeadingLineBreak(document, syntaxRoot, token, endOfLine);
    }

    /// <summary>
    /// Inserts a line break before a token when the existing gap cannot be rebuilt directly
    /// </summary>
    /// <param name="document">Document being updated</param>
    /// <param name="syntaxRoot">Document syntax root</param>
    /// <param name="token">Target token</param>
    /// <param name="endOfLine">Line-ending sequence</param>
    /// <returns>The updated document</returns>
    private static Document InsertLeadingLineBreak(Document document, SyntaxNode syntaxRoot, SyntaxToken token, string endOfLine)
    {
        var newToken = token.WithLeadingTrivia(token.LeadingTrivia.Insert(0, SyntaxFactory.EndOfLine(endOfLine)));

        return document.WithSyntaxRoot(syntaxRoot.ReplaceToken(token, newToken));
    }

    /// <summary>
    /// Rebuilds a formatting-only same-line gap as a blank line with the target indentation
    /// </summary>
    /// <param name="document">Document being updated</param>
    /// <param name="syntaxRoot">Document syntax root</param>
    /// <param name="sourceText">Document source text</param>
    /// <param name="previousToken">Token before the target</param>
    /// <param name="token">Target token</param>
    /// <param name="endOfLine">Line-ending sequence</param>
    /// <returns>The updated document</returns>
    private static Document ReplaceFormattingOnlyGap(Document document,
                                                     SyntaxNode syntaxRoot,
                                                     SourceText sourceText,
                                                     SyntaxToken previousToken,
                                                     SyntaxToken token,
                                                     string endOfLine)
    {
        var newPreviousToken = previousToken.WithTrailingTrivia(TrimTrailingWhitespace(previousToken.TrailingTrivia));
        var updatedToken = token.WithLeadingTrivia(CreateBlankLineLeadingTrivia(token, sourceText, previousToken, endOfLine));
        var updatedRoot = syntaxRoot.ReplaceTokens([previousToken, token],
                                                   (originalToken, _) => originalToken == previousToken
                                                                             ? newPreviousToken
                                                                             : updatedToken);

        return document.WithSyntaxRoot(updatedRoot);
    }

    /// <summary>
    /// Builds the target token's leading trivia after inserting a blank line
    /// </summary>
    /// <param name="token">Target token</param>
    /// <param name="sourceText">Document source text</param>
    /// <param name="previousToken">Token before the target, whose line supplies the indentation</param>
    /// <param name="endOfLine">Line-ending sequence</param>
    /// <returns>The rebuilt leading trivia</returns>
    private static SyntaxTriviaList CreateBlankLineLeadingTrivia(SyntaxToken token, SourceText sourceText, SyntaxToken previousToken, string endOfLine)
    {
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
        var indentation = GetIndentation(sourceText, token, previousToken);

        if (indentation.Length > 0)
        {
            newLeadingTriviaItems.Add(SyntaxFactory.Whitespace(indentation));
        }

        newLeadingTriviaItems.AddRange(targetLeadingTrivia.Skip(suffixStart));

        return SyntaxFactory.TriviaList(newLeadingTriviaItems);
    }

    /// <summary>
    /// Removes horizontal whitespace from the end of a trivia list
    /// </summary>
    /// <param name="trivia">Trivia to trim</param>
    /// <returns>The trimmed trivia</returns>
    private static SyntaxTriviaList TrimTrailingWhitespace(SyntaxTriviaList trivia)
    {
        while (trivia.Count > 0 && trivia[trivia.Count - 1].IsKind(SyntaxKind.WhitespaceTrivia))
        {
            trivia = trivia.RemoveAt(trivia.Count - 1);
        }

        return trivia;
    }

    /// <summary>
    /// Gets the indentation to apply after moving the target token onto its own line. Normally this is the
    /// target statement's syntactic nesting depth, which is canonical: it self-corrects a stray extra space or
    /// tab on the previous line instead of propagating it. That canonical value does not exist when an object
    /// initializer or anonymous object sits between the statement and its nearest brace scope, because neither
    /// is a level - <see cref="SyntaxIndentationUtilities.ComputeStatementIndentLevel"/> has no way to turn "one
    /// more initializer" into the anchor-derived column the formatter's own alignment contributors would place
    /// it at (issue #748). In that situation the previous token's line supplies the column instead, since that
    /// line has not moved and its leading whitespace is still correct
    /// </summary>
    /// <param name="sourceText">Document source text</param>
    /// <param name="token">Diagnostic target token</param>
    /// <param name="previousToken">Token before the diagnostic target</param>
    /// <returns>Indentation to apply to the target after moving it to its own line</returns>
    private static string GetIndentation(SourceText sourceText, SyntaxToken token, SyntaxToken previousToken)
    {
        var targetStatement = token.Parent?.FirstAncestorOrSelf<StatementSyntax>();

        if (targetStatement == null)
        {
            return string.Empty;
        }

        if (SyntaxIndentationUtilities.HasAnchorScopeAncestor(targetStatement))
        {
            var previousLine = sourceText.Lines.GetLineFromPosition(previousToken.SpanStart);

            return FormattingTextAnalysisUtilities.GetLeadingWhitespace(FormattingTextAnalysisUtilities.GetLineText(sourceText, previousLine));
        }

        return new string(' ', SyntaxIndentationUtilities.ComputeStatementIndentLevel(targetStatement) * SyntaxIndentationUtilities.IndentSize);
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