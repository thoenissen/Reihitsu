using System;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Formatter;
using Reihitsu.Formatter.Utilities;

namespace Reihitsu.Analyzer.CodeFixes.Base;

/// <summary>
/// The base of every code-fix provider and the one owner of whether a document lets a fix be offered at all: it decides it
/// before any fix registers, and a Fix All that does not register its fixes asks it for each document through
/// <see cref="CreateFixAllProvider"/>. No fix is offered in a document that holds code another build configuration hides
/// (SHARED-FIX-012), and no fix whose result the formatter lays out is offered in a document the formatter declines because of
/// a syntax error (SHARED-FIX-011). Both answers are the formatter's own
/// </summary>
public abstract class ReihitsuCodeFixProviderBase : CodeFixProvider
{
    #region Fields

    /// <summary>
    /// Whether a syntax tree holds a hiding carrier, computed once per tree: the batch Fix All registers once per diagnostic
    /// </summary>
    private static readonly ConditionalWeakTable<SyntaxTree, StrongBox<bool>> _hidingCarrierVerdicts = new();

    /// <summary>
    /// Whether a syntax tree holds a syntax error, computed once per tree
    /// </summary>
    private static readonly ConditionalWeakTable<SyntaxTree, StrongBox<bool>> _syntaxErrorVerdicts = new();

    #endregion // Fields

    #region Properties

    /// <summary>
    /// Is the fix's result laid out by a formatter entry point that returns its input unchanged for a syntax tree with a syntax
    /// error (FMT-PIPE-005), so that the fix is not offered in such a document (SHARED-FIX-011)?
    /// </summary>
    protected virtual bool FormatsThroughFormatter => false;

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Determines whether a document holds a hiding carrier as FMT-PIPE-033 defines it, so that no fix that changes the
    /// document may be offered (SHARED-FIX-012). A fix that changes other documents than its diagnostic's own asks it about
    /// each of them
    /// </summary>
    /// <param name="document">The document</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> if the document holds a hiding carrier; otherwise, <see langword="false"/></returns>
    internal static async Task<bool> HoldsHidingCarrierAsync(Document document, CancellationToken cancellationToken)
    {
        var syntaxTree = await document.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);

        return syntaxTree != null
               && HoldsHidingCarrier(syntaxTree, cancellationToken);
    }

    /// <summary>
    /// Creates a Fix All provider that applies the provider's fixes to one document at a time without registering them, and
    /// leaves a document in which no fix may be offered unchanged (SHARED-FIX-011, SHARED-FIX-012)
    /// </summary>
    /// <param name="fixAllAsync">Applies every fix of the given diagnostics to a document in which a fix may be offered</param>
    /// <returns>The Fix All provider</returns>
    protected FixAllProvider CreateFixAllProvider(Func<FixAllContext, Document, ImmutableArray<Diagnostic>, Task<Document>> fixAllAsync)
    {
        return FixAllProvider.Create(async (context, document, diagnostics) =>
                                     {
                                         if (await MayOfferFixesAsync(document, context.CancellationToken).ConfigureAwait(false) == false)
                                         {
                                             return document;
                                         }

                                         return await fixAllAsync(context, document, diagnostics).ConfigureAwait(false);
                                     });
    }

    /// <summary>
    /// Registers the provider's fixes for the context's diagnostics. It is called only for a document in which a fix may be
    /// offered
    /// </summary>
    /// <param name="context">The code-fix context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    protected abstract Task RegisterCodeFixesCoreAsync(CodeFixContext context);

    /// <summary>
    /// Determines whether a syntax tree holds a hiding carrier, reusing the verdict for a tree that was asked before
    /// </summary>
    /// <param name="syntaxTree">The syntax tree</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> if the tree holds a hiding carrier; otherwise, <see langword="false"/></returns>
    private static bool HoldsHidingCarrier(SyntaxTree syntaxTree, CancellationToken cancellationToken)
    {
        return _hidingCarrierVerdicts.GetValue(syntaxTree, tree => new StrongBox<bool>(ReihitsuFormatter.HoldsHidingCarrier(tree, cancellationToken))).Value;
    }

    /// <summary>
    /// Determines whether a syntax tree holds a syntax error, for which the formatter returns its input unchanged (FMT-PIPE-005),
    /// reusing the verdict for a tree that was asked before
    /// </summary>
    /// <param name="syntaxTree">The syntax tree</param>
    /// <returns><see langword="true"/> if the tree holds a syntax error; otherwise, <see langword="false"/></returns>
    private static bool HasSyntaxErrors(SyntaxTree syntaxTree)
    {
        return _syntaxErrorVerdicts.GetValue(syntaxTree, tree => new StrongBox<bool>(ReihitsuFormatterHelpers.HasSyntaxErrors(tree))).Value;
    }

    /// <summary>
    /// Determines whether the provider's fixes may be offered in a document: not in a document that holds a hiding carrier
    /// (SHARED-FIX-012), and, for a fix whose result the formatter lays out, not in a document with a syntax error
    /// (SHARED-FIX-011)
    /// </summary>
    /// <param name="document">The document</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> if a fix may be offered in the document; otherwise, <see langword="false"/></returns>
    private async Task<bool> MayOfferFixesAsync(Document document, CancellationToken cancellationToken)
    {
        var syntaxTree = await document.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);

        return syntaxTree != null
               && HoldsHidingCarrier(syntaxTree, cancellationToken) == false
               && (FormatsThroughFormatter == false || HasSyntaxErrors(syntaxTree) == false);
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    /// <remarks>
    /// Registers nothing in a document that holds a hiding carrier (SHARED-FIX-012), or, for a fix whose result the formatter
    /// lays out, in a document with a syntax error (SHARED-FIX-011); the diagnostics stay reported
    /// </remarks>
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        if (await MayOfferFixesAsync(context.Document, context.CancellationToken).ConfigureAwait(false))
        {
            await RegisterCodeFixesCoreAsync(context).ConfigureAwait(false);
        }
    }

    #endregion // CodeFixProvider
}