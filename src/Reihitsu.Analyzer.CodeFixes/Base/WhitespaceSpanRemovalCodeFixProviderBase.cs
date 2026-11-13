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
/// Code fix provider base class for rules whose fix removes the reported whitespace (SHARED-WS-001). The guard
/// inspects the reported diagnostic span and withholds the fix unless that span is whitespace-only, so a future
/// analyzer reporting a non-whitespace span degrades to no fix being offered instead of deleting code. The removed
/// span is the reported span unless a derived rule widens it through <see cref="GetRemovalSpan"/>, which may add
/// only whitespace-only lines, so the deletion stays whitespace-only whenever the guard lets it through. RH6004
/// derives from this class directly; blank-line rules derive from it through
/// <see cref="BlankLineSpanRemovalCodeFixProviderBase"/>, which widens the span to the whole run of blank lines for
/// the rules that name the run
/// <para>
/// Choose this base when the reported span is already whitespace-only by construction and the fix must stay
/// offered for every input the analyzer can report. Choose <see cref="RemoveWhitespaceRunCodeFixProviderBase"/>
/// instead when the deletion also needs to refuse a comment or directive in the *surrounding* token gap, not
/// only inside the reported span itself
/// </para>
/// </summary>
public abstract class WhitespaceSpanRemovalCodeFixProviderBase : CodeFixProvider
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
    private protected WhitespaceSpanRemovalCodeFixProviderBase(string diagnosticId, string title)
    {
        _diagnosticId = diagnosticId;
        _title = title;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Gets the span the fix removes for a reported span. By default it is the reported span itself; a derived rule may
    /// widen it to more whitespace around it, so the removed span stays whitespace-only
    /// </summary>
    /// <param name="sourceText">Source text</param>
    /// <param name="diagnosticSpan">Diagnostic span, which holds only whitespace</param>
    /// <returns>The span to remove</returns>
    private protected virtual TextSpan GetRemovalSpan(SourceText sourceText, TextSpan diagnosticSpan)
    {
        return diagnosticSpan;
    }

    /// <summary>
    /// Applies the code fix by removing the span <see cref="GetRemovalSpan"/> gives for the diagnostic span
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="diagnosticSpan">Diagnostic span</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    private async Task<Document> ApplyCodeFixAsync(Document document, TextSpan diagnosticSpan, CancellationToken cancellationToken)
    {
        var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);

        return document.WithText(sourceText.Replace(GetRemovalSpan(sourceText, diagnosticSpan), string.Empty));
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
        var sourceText = await context.Document.GetTextAsync(context.CancellationToken).ConfigureAwait(false);

        foreach (var diagnostic in context.Diagnostics)
        {
            var span = diagnostic.Location.SourceSpan;

            if (FormattingTextAnalysisUtilities.IsWhitespaceOnly(sourceText, span) == false)
            {
                continue;
            }

            context.RegisterCodeFix(CodeAction.Create(_title,
                                                      token => ApplyCodeFixAsync(context.Document, span, token),
                                                      GetType().Name),
                                    diagnostic);
        }
    }

    #endregion // CodeFixProvider
}