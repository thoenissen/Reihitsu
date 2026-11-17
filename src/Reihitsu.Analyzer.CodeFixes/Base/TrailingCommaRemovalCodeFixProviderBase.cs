using System.Collections.Immutable;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Core;

namespace Reihitsu.Analyzer.CodeFixes.Base;

/// <summary>
/// Code fix provider base class for rules whose fix removes a final trailing comma
/// </summary>
public abstract class TrailingCommaRemovalCodeFixProviderBase : ReihitsuCodeFixProviderBase
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
    private protected TrailingCommaRemovalCodeFixProviderBase(string diagnosticId, string title)
    {
        _diagnosticId = diagnosticId;
        _title = title;
    }

    #endregion // Constructor

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [_diagnosticId];

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    #endregion // CodeFixProvider

    #region ReihitsuCodeFixProviderBase

    /// <inheritdoc/>
    protected sealed override async Task RegisterCodeFixesCoreAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return;
        }

        foreach (var diagnostic in context.Diagnostics)
        {
            // Another build can compile an element behind a conditional directive or excluded code between the comma and
            // the closing brace, and that element needs the comma, so the fix is withheld there (SHARED-TRAIL-002)
            if (TrailingCommaCodeFixHelper.CommaPrecedesConditionalCompilationBoundary(root, diagnostic.Location.SourceSpan))
            {
                continue;
            }

            context.RegisterCodeFix(CodeAction.Create(_title,
                                                      token => TrailingCommaCodeFixHelper.RemoveTrailingCommaAsync(context.Document, diagnostic.Location.SourceSpan, token),
                                                      GetType().Name),
                                    diagnostic);
        }
    }

    #endregion // ReihitsuCodeFixProviderBase
}