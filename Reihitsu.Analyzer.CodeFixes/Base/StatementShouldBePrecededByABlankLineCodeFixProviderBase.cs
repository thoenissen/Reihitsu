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

        return previousToken.IsKind(SyntaxKind.None) == false
               && TokenGapAnalysis.Between(previousToken, token).RequiredLineBreakCountForBlankLine == 2
                   ? ReplaceFormattingOnlyGap(document, syntaxRoot, previousToken, token, endOfLine)
                   : InsertLeadingLineBreak(document, syntaxRoot, token, endOfLine);
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
    /// <param name="previousToken">Token before the target</param>
    /// <param name="token">Target token</param>
    /// <param name="endOfLine">Line-ending sequence</param>
    /// <returns>The updated document</returns>
    private static Document ReplaceFormattingOnlyGap(Document document,
                                                     SyntaxNode syntaxRoot,
                                                     SyntaxToken previousToken,
                                                     SyntaxToken token,
                                                     string endOfLine)
    {
        var newPreviousToken = previousToken.WithTrailingTrivia(TrimTrailingWhitespace(previousToken.TrailingTrivia));
        var updatedToken = token.WithLeadingTrivia(CreateBlankLineLeadingTrivia(token, endOfLine));
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
    /// <param name="endOfLine">Line-ending sequence</param>
    /// <returns>The rebuilt leading trivia</returns>
    private static SyntaxTriviaList CreateBlankLineLeadingTrivia(SyntaxToken token, string endOfLine)
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
        var indentation = GetIndentation(token);

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
    /// Gets the syntax-derived indentation of a same-line diagnostic target
    /// </summary>
    /// <param name="token">First token of the diagnostic target</param>
    /// <returns>Indentation to apply to the target after moving it to its own line</returns>
    private static string GetIndentation(SyntaxToken token)
    {
        var targetStatement = token.Parent?.FirstAncestorOrSelf<StatementSyntax>();

        return targetStatement == null
                   ? string.Empty
                   : new string(' ', SyntaxIndentationUtilities.ComputeStatementIndentLevel(targetStatement) * SyntaxIndentationUtilities.IndentSize);
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