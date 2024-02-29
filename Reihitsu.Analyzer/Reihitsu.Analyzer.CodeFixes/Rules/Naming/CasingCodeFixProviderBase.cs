using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Rename;

namespace Reihitsu.Analyzer.Rules.Naming;

/// <summary>
/// Code fix provider for casing rules
/// </summary>
/// <typeparam name="T">Node type</typeparam>
public abstract class CasingCodeFixProviderBase<T> : CodeFixProvider where T : SyntaxNode
{
    #region Fields

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    private string _diagnosticId;

    /// <summary>
    /// Title
    /// </summary>
    private string _title;

    /// <summary>
    /// Casing conversion
    /// </summary>
    private Func<string, string> _casingConversion;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="diagnosticId">Diagnostic ID</param>
    /// <param name="title">Title</param>
    /// <param name="casingConversion">Casing conversion</param>
    protected CasingCodeFixProviderBase(string diagnosticId, string title, Func<string, string> casingConversion)
    {
        _diagnosticId = diagnosticId;
        _title = title;
        _casingConversion = casingConversion;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Replace the identifier with new one
    /// </summary>
    /// <param name="node">Node</param>
    /// <param name="identifier">Fixed identifier value text</param>
    /// <returns>Fixed identifier</returns>
    protected abstract SyntaxNode ReplaceIdentifier(T node, string identifier);

    /// <summary>
    /// Get identifier value text
    /// </summary>
    /// <param name="node">Node</param>
    /// <returns>Identifier value text</returns>
    protected abstract string GetIdentifier(T node);

    /// <summary>
    /// Applying the code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="node">Node</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated <see cref="Document"/> with the code fix applied.</returns>
    private async Task<Solution> ApplyCodeFixAsync(Document document, T node, CancellationToken cancellationToken)
    {
        var solution = document.Project.Solution;

        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root != null)
        {
            var identifier = GetIdentifier(node);

            identifier = _casingConversion(identifier);

            var model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            if (model != null)
            {
                var symbol = model.GetDeclaredSymbol(node);

                if (symbol != null)
                {
                    solution = await Renamer.RenameSymbolAsync(document.Project.Solution, symbol, default, identifier, cancellationToken).ConfigureAwait(false);
                }
            }

            root = root.ReplaceNode(node, ReplaceIdentifier(node, identifier));

            solution = solution.WithDocumentSyntaxRoot(document.Id, root);
        }

        return solution;
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(_diagnosticId);

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root != null)
        {
            foreach (var diagnostic in context.Diagnostics)
            {
                var diagnosticSpan = diagnostic.Location.SourceSpan;

                if (root.FindNode(diagnosticSpan) is T node)
                {
                    context.RegisterCodeFix(CodeAction.Create(_title,
                                                              c => ApplyCodeFixAsync(context.Document, node, c),
                                                              GetType().Name),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // CodeFixProvider
}