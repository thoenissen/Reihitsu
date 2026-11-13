using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.CodeFixes.Core;
using Reihitsu.Core;
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
    /// Applies the code fix: the reported body is wrapped in a block whose braces start their own lines in the column of the
    /// line that holds the keyword owning the body, and the block is laid out by the formatter; the condition, the other
    /// bodies of the chain, and the code around the statement stay as written (SHARED-BRACE-002)
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

        var body = FindReportedBody(root, diagnosticSpan);

        if (body == null)
        {
            return document;
        }

        // The body keeps its trailing trivia, a comment behind it included, when that trivia ends its line; otherwise the
        // closing brace takes over what stood behind the body
        var endOfLine = SyntaxFactory.EndOfLine(LineEndingUtilities.DetectEndOfLine(root));
        var bodyTrailingTrivia = body.GetTrailingTrivia();
        var endsLine = bodyTrailingTrivia.Any(SyntaxKind.EndOfLineTrivia);
        var block = SyntaxFactory.Block(endsLine ? body : body.WithTrailingTrivia(endOfLine))
                                 .WithOpenBraceToken(SyntaxFactory.Token(SyntaxKind.OpenBraceToken).WithTrailingTrivia(endOfLine))
                                 .WithCloseBraceToken(SyntaxFactory.Token(SyntaxKind.CloseBraceToken).WithTrailingTrivia(endsLine ? SyntaxFactory.TriviaList(endOfLine) : bodyTrailingTrivia));

        return await LineLayoutCodeFixUtilities.ReplaceWithBlockOnOwnLinesAsync(document, body, block, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Finds the reported body: the unbraced statement that an <c>if</c> statement or an <c>else</c> clause owns
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <param name="diagnosticSpan">The diagnostic's span</param>
    /// <returns>The body, or <see langword="null"/> when the reported statement is not such a body</returns>
    private static StatementSyntax FindReportedBody(SyntaxNode root, TextSpan diagnosticSpan)
    {
        var statement = root.FindNode(diagnosticSpan).FirstAncestorOrSelf<StatementSyntax>();

        return statement?.Parent is IfStatementSyntax or ElseClauseSyntax
                   ? statement
                   : null;
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
            if (FindReportedBody(root, diagnostic.Location.SourceSpan) is { } body
                && ReihitsuFormatter.FindUnsupportedSyntax(body) != null)
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