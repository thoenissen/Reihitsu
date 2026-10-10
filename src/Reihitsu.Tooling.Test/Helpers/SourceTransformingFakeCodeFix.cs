using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;

namespace Reihitsu.Tooling.Test.Helpers;

/// <summary>
/// A code fix provider that replaces the fixture source through a caller-provided transformation
/// </summary>
internal sealed class SourceTransformingFakeCodeFix : CodeFixProvider
{
    #region Fields

    /// <summary>
    /// Transformation applied to the fixture source
    /// </summary>
    private readonly Func<string, string> _transform;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="SourceTransformingFakeCodeFix"/> class
    /// </summary>
    /// <param name="transform">Transformation applied to the fixture source</param>
    public SourceTransformingFakeCodeFix(Func<string, string> transform)
    {
        _transform = transform;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Applies the configured source transformation to the document
    /// </summary>
    /// <param name="document">Fixture document</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The transformed document</returns>
    private async Task<Document> TransformAsync(Document document, CancellationToken cancellationToken)
    {
        var text = await document.GetTextAsync(cancellationToken);

        return document.WithText(SourceText.From(_transform(text.ToString())));
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public override ImmutableArray<string> FixableDiagnosticIds => [FakeDiagnostic.Id];

    /// <inheritdoc/>
    public override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        context.RegisterCodeFix(CodeAction.Create("Transform fixture",
                                                  cancellationToken => TransformAsync(context.Document, cancellationToken),
                                                  nameof(SourceTransformingFakeCodeFix)),
                                context.Diagnostics);

        return Task.CompletedTask;
    }

    #endregion // CodeFixProvider
}