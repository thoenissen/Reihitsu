using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Formatter;

namespace Reihitsu.Analyzer.CodeFixes.Base;

/// <summary>
/// Base class for code fixes that wrap a statement with braces
/// </summary>
public abstract class StatementBracesCodeFixProviderBase : CodeFixProvider
{
    #region Fields

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    private readonly string _diagnosticId;

    /// <summary>
    /// Code fix title
    /// </summary>
    private readonly string _title;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="diagnosticId">Diagnostic ID</param>
    /// <param name="title">Code fix title</param>
    protected StatementBracesCodeFixProviderBase(string diagnosticId, string title)
    {
        _diagnosticId = diagnosticId;
        _title = title;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Applies the code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="diagnosticSpan">Diagnostic span</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, TextSpan diagnosticSpan, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        var ownerStatement = FindFormattingNode(root, diagnosticSpan);

        if (ownerStatement == null)
        {
            return document;
        }

        // Delegating to the formatter wraps the embedded statement in braces and lays out the control-flow
        // statement, preserving the parent header and indentation regardless of whether the child shares the
        // parent's line. Editing the text manually deletes the header when the child is on the parent's line.
        return (await ReihitsuFormatter.FormatNodeInDocumentAsync(document, ownerStatement, cancellationToken).ConfigureAwait(false)).Output;
    }

    /// <summary>
    /// Finds the node the fix formats. The diagnostic targets the unbraced child statement; the statement that owns it as an
    /// embedded statement is the node the formatter needs to rewrite so the braces are inserted around it
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <param name="diagnosticSpan">The diagnostic's span</param>
    /// <returns>The owning statement, or <see langword="null"/> when there is none</returns>
    private static SyntaxNode FindFormattingNode(SyntaxNode root, TextSpan diagnosticSpan)
    {
        var statement = root.FindNode(diagnosticSpan).FirstAncestorOrSelf<StatementSyntax>();

        return statement?.Parent switch
               {
                   IfStatementSyntax ifStatement => ifStatement,
                   ElseClauseSyntax { Parent: IfStatementSyntax ifStatement } => ifStatement,
                   _ => null,
               };
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

        if (root == null)
        {
            return;
        }

        foreach (var diagnostic in context.Diagnostics)
        {
            if (FindFormattingNode(root, diagnostic.Location.SourceSpan) is { } node
                && ReihitsuFormatter.FindUnsupportedSyntax(node) != null)
            {
                continue;
            }

            context.RegisterCodeFix(CodeAction.Create(_title,
                                                      token => ApplyCodeFixAsync(context.Document, diagnostic.Location.SourceSpan, token),
                                                      GetType().Name),
                                    diagnostic);
        }
    }

    #endregion // CodeFixProvider
}