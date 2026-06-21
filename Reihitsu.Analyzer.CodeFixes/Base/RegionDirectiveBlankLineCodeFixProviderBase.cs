using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Core;
using Reihitsu.Formatter;

namespace Reihitsu.Analyzer.CodeFixes.Base;

/// <summary>
/// Code fix provider base class for rules based on <c>RegionDirectiveBlankLineAnalyzerBase</c>
/// </summary>
public abstract class RegionDirectiveBlankLineCodeFixProviderBase : CodeFixProvider
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

    /// <summary>
    /// Whether the fix inserts a blank line preceding the directive (otherwise a blank line following it)
    /// </summary>
    private readonly bool _insertPrecedingBlankLine;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="diagnosticId">Diagnostic ID</param>
    /// <param name="title">Title</param>
    /// <param name="insertPrecedingBlankLine">Whether the fix inserts a blank line preceding the directive</param>
    private protected RegionDirectiveBlankLineCodeFixProviderBase(string diagnosticId, string title, bool insertPrecedingBlankLine)
    {
        _diagnosticId = diagnosticId;
        _title = title;
        _insertPrecedingBlankLine = insertPrecedingBlankLine;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Applies the code fix by inserting the missing blank line on the relevant side of the directive
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="diagnosticSpan">Diagnostic span</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    private async Task<Document> ApplyCodeFixAsync(Document document, TextSpan diagnosticSpan, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
        var endOfLine = ReihitsuFormatterHelpers.DetectEndOfLine(root);
        var directiveLineIndex = sourceText.Lines.GetLineFromPosition(diagnosticSpan.Start).LineNumber;
        var nonFormattableLineIndices = FormattingTextAnalysisUtilities.GetNonFormattableLineIndices(root, sourceText);

        if (RegionDirectiveBlankLineUtilities.IsAdjacentToNonFormattableLine(directiveLineIndex, sourceText.Lines.Count, nonFormattableLineIndices))
        {
            return document;
        }

        if (_insertPrecedingBlankLine)
        {
            var openBraceEndLineIndices = FormattingTextAnalysisUtilities.GetLineIndicesEndingWithToken(root,
                                                                                                        sourceText,
                                                                                                        static token => token.IsKind(SyntaxKind.OpenBraceToken));

            return RegionDirectiveBlankLineUtilities.IsMissingRequiredBlankLineBefore(sourceText, directiveLineIndex, openBraceEndLineIndices)
                       ? document.WithText(sourceText.Replace(new TextSpan(sourceText.Lines[directiveLineIndex].Start, 0), endOfLine))
                       : document;
        }

        var closeBraceStartLineIndices = FormattingTextAnalysisUtilities.GetLineIndicesBeginningWithToken(root,
                                                                                                          sourceText,
                                                                                                          static token => token.IsKind(SyntaxKind.CloseBraceToken));

        return RegionDirectiveBlankLineUtilities.IsMissingRequiredBlankLineAfter(sourceText, directiveLineIndex, closeBraceStartLineIndices)
                   ? document.WithText(sourceText.Replace(new TextSpan(sourceText.Lines[directiveLineIndex + 1].Start, 0), endOfLine))
                   : document;
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