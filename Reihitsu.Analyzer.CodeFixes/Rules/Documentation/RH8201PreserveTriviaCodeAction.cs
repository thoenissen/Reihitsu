using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Documentation;

/// <summary>
/// RH8201 code action that preserves the provider's trivia without formatting adjacent member boundaries
/// </summary>
internal sealed class RH8201PreserveTriviaCodeAction : CodeAction
{
    #region Fields

    /// <summary>
    /// Callback that creates the changed document
    /// </summary>
    private readonly Func<CancellationToken, Task<Document>> _createChangedDocument;

    #endregion // Fields

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="RH8201PreserveTriviaCodeAction"/> class
    /// </summary>
    /// <param name="title">Code action title</param>
    /// <param name="createChangedDocument">Callback that creates the changed document</param>
    /// <param name="equivalenceKey">Code action equivalence key</param>
    public RH8201PreserveTriviaCodeAction(string title,
                                          Func<CancellationToken, Task<Document>> createChangedDocument,
                                          string equivalenceKey)
    {
        Title = title;
        _createChangedDocument = createChangedDocument;
        EquivalenceKey = equivalenceKey;
    }

    #endregion // Constructors

    #region CodeAction

    /// <inheritdoc/>
    public override string EquivalenceKey { get; }

    /// <inheritdoc/>
    public override string Title { get; }

    /// <inheritdoc/>
    protected override Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
    {
        return _createChangedDocument(cancellationToken);
    }

    /// <inheritdoc/>
    protected override Task<Document> PostProcessChangesAsync(Document document, CancellationToken cancellationToken)
    {
        return Task.FromResult(document);
    }

    #endregion // CodeAction
}