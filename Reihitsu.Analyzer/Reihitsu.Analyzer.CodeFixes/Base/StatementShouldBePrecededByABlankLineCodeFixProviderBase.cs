using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;

namespace Reihitsu.Analyzer.Base;

/// <summary>
/// Code fix provider base class for rules based on <see cref="StatementShouldBePrecededByABlankLineAnalyzerBase{TStatement,TAnalyzer}"/>
/// </summary>
public abstract class StatementShouldBePrecededByABlankLineCodeFixProviderBase : CodeFixProvider
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
    private protected StatementShouldBePrecededByABlankLineCodeFixProviderBase(string diagnosticId, string title)
    {
        _diagnosticId = diagnosticId;
        _title = title;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Applying code fix by inserting a blank line before the statement
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="token">Token at the diagnostic location</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, SyntaxToken token, CancellationToken cancellationToken)
    {
        var syntaxRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (syntaxRoot != null)
        {
            var leadingTrivia = token.LeadingTrivia;
            var newLeadingTrivia = leadingTrivia.Insert(0, SyntaxFactory.EndOfLine(Environment.NewLine));
            var newToken = token.WithLeadingTrivia(newLeadingTrivia);

            syntaxRoot = syntaxRoot.ReplaceToken(token, newToken);

            document = document.WithSyntaxRoot(syntaxRoot);
        }

        return document;
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
                var token = root.FindToken(diagnostic.Location.SourceSpan.Start);

                context.RegisterCodeFix(CodeAction.Create(_title,
                                                          c => ApplyCodeFixAsync(context.Document, token, c),
                                                          GetType().Name),
                                        diagnostic);
            }
        }
    }

    #endregion // CodeFixProvider
}
