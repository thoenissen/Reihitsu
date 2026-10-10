using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Reihitsu.Tooling.Test.Helpers;

/// <summary>
/// A code fix provider that removes the fixture document and adds a caller-selected number of replacements,
/// so the fixture project ends up with zero or more than one candidate document
/// </summary>
internal sealed class DocumentCountChangingFakeCodeFix : CodeFixProvider
{
    #region Fields

    /// <summary>
    /// Number of replacement documents added after the original is removed
    /// </summary>
    private readonly int _replacementCount;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentCountChangingFakeCodeFix"/> class
    /// </summary>
    /// <param name="replacementCount">Number of replacement documents added after the original is removed</param>
    public DocumentCountChangingFakeCodeFix(int replacementCount)
    {
        _replacementCount = replacementCount;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Removes the document and adds the configured number of replacements
    /// </summary>
    /// <param name="document">Document to replace</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The solution carrying the replacement documents</returns>
    private async Task<Solution> ReplaceAsync(Document document, CancellationToken cancellationToken)
    {
        var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
        var solution = document.Project.Solution.RemoveDocument(document.Id);

        for (var index = 0; index < _replacementCount; index++)
        {
            var newDocumentId = DocumentId.CreateNewId(document.Project.Id);

            solution = solution.AddDocument(newDocumentId, $"Replacement{index}.cs", text, document.Folders);
        }

        return solution;
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public override ImmutableArray<string> FixableDiagnosticIds => [FakeDiagnostic.Id];

    /// <inheritdoc/>
    public override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        context.RegisterCodeFix(CodeAction.Create("Replace fixture document count",
                                                  cancellationToken => ReplaceAsync(context.Document, cancellationToken),
                                                  nameof(DocumentCountChangingFakeCodeFix)),
                                context.Diagnostics);

        return Task.CompletedTask;
    }

    #endregion // CodeFixProvider
}