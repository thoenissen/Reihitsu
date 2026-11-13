using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Core;

namespace Reihitsu.Analyzer.CodeFixes.Base;

/// <summary>
/// Base class for code fixes that normalize the spacing around an operator token
/// </summary>
public abstract class OperatorSpacingCodeFixProviderBase : CodeFixProvider
{
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
    protected OperatorSpacingCodeFixProviderBase(string diagnosticId, string title)
    {
        _diagnosticId = diagnosticId;
        _title = title;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Applies the code fix: each gap next to the operator that holds nothing but whitespace becomes exactly one space; a
    /// gap that holds a line break, a comment, or a directive stays as written, and nothing else changes
    /// (SHARED-OPSPACE-004)
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="diagnosticSpan">Diagnostic span</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, TextSpan diagnosticSpan, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        var operatorToken = root.FindToken(diagnosticSpan.Start);
        var previousToken = operatorToken.GetPreviousToken();
        var nextToken = operatorToken.GetNextToken();

        if (previousToken.RawKind == 0
            || nextToken.RawKind == 0)
        {
            return document;
        }

        var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);

        // The analyzer's gap decision tells which gap the fix corrects, so the fix changes exactly the reported gaps
        TextSpan[] gaps = [TextSpan.FromBounds(previousToken.Span.End, operatorToken.SpanStart), TextSpan.FromBounds(operatorToken.Span.End, nextToken.SpanStart)];
        var changes = gaps.Where(gap => FormattingTextAnalysisUtilities.IsOperatorGapViolation(sourceText, gap.Start, gap.End))
                          .Select(gap => new TextChange(gap, " "));

        return document.WithText(sourceText.WithChanges(changes));
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
    public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        foreach (var diagnostic in context.Diagnostics)
        {
            context.RegisterCodeFix(CodeAction.Create(_title,
                                                      token => ApplyCodeFixAsync(context.Document, diagnostic.Location.SourceSpan, token),
                                                      GetType().Name),
                                    diagnostic);
        }

        return Task.CompletedTask;
    }

    #endregion // CodeFixProvider
}