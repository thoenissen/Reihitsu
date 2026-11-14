using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Core;
using Reihitsu.Formatter;
using Reihitsu.Formatter.Pipeline.StructuralTransforms.Utilities;

namespace Reihitsu.Analyzer.CodeFixes.Base;

/// <summary>
/// Code fix provider base class for rules whose fix converts an expression body to the block body the formatter produces for the owning node
/// </summary>
/// <typeparam name="TNode">Node type that owns the expression body</typeparam>
public abstract class ExpressionBodyToBlockCodeFixProviderBase<TNode> : CodeFixProvider
    where TNode : SyntaxNode
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
    private protected ExpressionBodyToBlockCodeFixProviderBase(string diagnosticId, string title)
    {
        _diagnosticId = diagnosticId;
        _title = title;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Applying code fix: the expression body is replaced with the block body the formatter produces, and the rest of the
    /// node stays as written (SHARED-BODY-003)
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="node">Node that owns the reported expression body</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    protected virtual Task<Document> ApplyCodeFixAsync(Document document, TNode node, CancellationToken cancellationToken)
    {
        return ExpressionBodyCodeFixUtilities.ConvertBodyAsync(document, node, cancellationToken);
    }

    /// <summary>
    /// Gets the node the formatter receives from <see cref="ApplyCodeFixAsync"/>: the node itself, or the context it is
    /// formatted in, which the fix asks the formatter about when it registers (SHARED-FIX-010)
    /// </summary>
    /// <param name="node">Node that owns the reported expression body</param>
    /// <returns>The node the formatter receives</returns>
    protected virtual SyntaxNode GetFormattingInput(TNode node)
    {
        return node;
    }

    /// <summary>
    /// Determines whether the fix can be delivered for the node beyond the formatter's own refusals. A rule whose fix also
    /// edits code next to the node withholds the fix where that edit cannot be made
    /// </summary>
    /// <param name="root">Syntax root of the document that contains <paramref name="node"/></param>
    /// <param name="node">Node that owns the reported expression body</param>
    /// <returns><see langword="true"/> if the fix can be registered</returns>
    protected virtual bool CanRegisterCodeFix(SyntaxNode root, TNode node)
    {
        return true;
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
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        if (root != null)
        {
            foreach (var diagnostic in context.Diagnostics)
            {
                // The diagnostic covers the arrow clause, so the nearest owner is the member or local function that declares it
                var node = root.FindNode(diagnostic.Location.SourceSpan)
                               .FirstAncestorOrSelf<TNode>();

                // The member stays reported without a fix wherever the fix could not deliver its result: the formatter keeps
                // the expression body of an async member whose task type it cannot see, so delegating to it would change
                // nothing; the formatter refuses input that holds a shape it does not support; and a derived provider can
                // withhold the fix for an input its own conversion cannot handle, such as a statement it would have to move
                // to a column the text does not hold
                if (node != null
                    && ExpressionBodiedTransformUtilities.RequiresAsyncExpressionBodyPreservation(node) == false
                    && ReihitsuFormatter.FindUnsupportedSyntax(GetFormattingInput(node)) == null
                    && CanRegisterCodeFix(root, node))
                {
                    context.RegisterCodeFix(CodeAction.Create(_title,
                                                              cancellationToken => ApplyCodeFixAsync(context.Document, node, cancellationToken),
                                                              GetType().Name),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // CodeFixProvider
}