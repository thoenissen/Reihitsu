using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Reihitsu.Tooling.Test.Helpers;

/// <summary>
/// A code fix provider that removes the fixture document and adds a replacement under a caller-selected name,
/// mirroring the document-identity shape of a rename-style code fix such as RH4001's
/// </summary>
internal sealed class DocumentReplacingFakeCodeFix : CodeFixProvider
{
    #region Fields

    /// <summary>
    /// Selects the replacement document name from the current document name
    /// </summary>
    private readonly Func<string, string> _nextName;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentReplacingFakeCodeFix"/> class
    /// </summary>
    /// <param name="nextName">Selects the replacement document name from the current document name</param>
    public DocumentReplacingFakeCodeFix(Func<string, string> nextName)
    {
        _nextName = nextName;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Removes the document and adds its replacement, keeping the same folders and text
    /// </summary>
    /// <param name="document">Document to replace</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The solution carrying the replacement document</returns>
    private async Task<Solution> ReplaceAsync(Document document, CancellationToken cancellationToken)
    {
        var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
        var solution = document.Project.Solution.RemoveDocument(document.Id);
        var newDocumentId = DocumentId.CreateNewId(document.Project.Id);

        return solution.AddDocument(newDocumentId, _nextName(document.Name), text, document.Folders);
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