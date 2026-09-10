using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;

namespace Reihitsu.Tooling.Test.Helpers;

/// <summary>
/// A code fix provider that removes the fixture document and adds a replacement under a caller-selected name,
/// mirroring the document-identity shape of a rename-style code fix such as RH4001's. It can optionally change
/// the text at the same time, so a caller can prove that the runner reads the replacement document's own text
/// rather than carrying the pre-fix text forward
/// </summary>
internal sealed class DocumentReplacingFakeCodeFix : CodeFixProvider
{
    #region Fields

    /// <summary>
    /// Selects the replacement document name from the current document name
    /// </summary>
    private readonly Func<string, string> _nextName;

    /// <summary>
    /// Transforms the fixture text for the replacement document
    /// </summary>
    private readonly Func<string, string> _transformText;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentReplacingFakeCodeFix"/> class
    /// </summary>
    /// <param name="nextName">Selects the replacement document name from the current document name</param>
    /// <param name="transformText">
    /// Transforms the fixture text for the replacement document. Defaults to leaving
    /// the text unchanged, so a rename-only shape stays the default
    /// </param>
    public DocumentReplacingFakeCodeFix(Func<string, string> nextName, Func<string, string> transformText = null)
    {
        _nextName = nextName;
        _transformText = transformText ?? (text => text);
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Removes the document and adds its replacement under the selected name and text
    /// </summary>
    /// <param name="document">Document to replace</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The solution carrying the replacement document</returns>
    private async Task<Solution> ReplaceAsync(Document document, CancellationToken cancellationToken)
    {
        var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
        var solution = document.Project.Solution.RemoveDocument(document.Id);
        var newDocumentId = DocumentId.CreateNewId(document.Project.Id);
        var newText = SourceText.From(_transformText(text.ToString()));

        return solution.AddDocument(newDocumentId, _nextName(document.Name), newText, document.Folders);
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public override ImmutableArray<string> FixableDiagnosticIds => [FakeDiagnostic.Id];

    /// <inheritdoc/>
    public override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        context.RegisterCodeFix(CodeAction.Create("Replace fixture document",
                                                  cancellationToken => ReplaceAsync(context.Document, cancellationToken),
                                                  nameof(DocumentReplacingFakeCodeFix)),
                                context.Diagnostics);

        return Task.CompletedTask;
    }

    #endregion // CodeFixProvider
}