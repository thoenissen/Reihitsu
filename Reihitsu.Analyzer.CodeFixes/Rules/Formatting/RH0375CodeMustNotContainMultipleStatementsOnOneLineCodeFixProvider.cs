using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// Code fix provider for <see cref="RH0375CodeMustNotContainMultipleStatementsOnOneLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0375CodeMustNotContainMultipleStatementsOnOneLineCodeFixProvider))]
public class RH0375CodeMustNotContainMultipleStatementsOnOneLineCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applies the code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="statement">Statement to move</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, StatementSyntax statement, CancellationToken cancellationToken)
    {
        var previousStatement = GetPreviousStatement(statement);

        if (previousStatement == null)
        {
            return document;
        }

        var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
        var indentationColumn = previousStatement.GetLocation().GetLineSpan().StartLinePosition.Character;
        var replacementText = Environment.NewLine + new string(' ', indentationColumn);
        var replacementSpan = TextSpan.FromBounds(previousStatement.Span.End, statement.Span.Start);

        return document.WithText(sourceText.Replace(replacementSpan, replacementText));
    }

    /// <summary>
    /// Gets the previous sibling statement
    /// </summary>
    /// <param name="statement">Current statement</param>
    /// <returns>The previous statement or <see langword="null"/></returns>
    private static StatementSyntax GetPreviousStatement(StatementSyntax statement)
    {
        if (statement.Parent is BlockSyntax block)
        {
            return block.Statements.TakeWhile(currentStatement => currentStatement != statement)
                                   .LastOrDefault(currentStatement => currentStatement is EmptyStatementSyntax == false);
        }

        if (statement.Parent is SwitchSectionSyntax switchSection)
        {
            return switchSection.Statements.TakeWhile(currentStatement => currentStatement != statement)
                                           .LastOrDefault(currentStatement => currentStatement is EmptyStatementSyntax == false);
        }

        return null;
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH0375CodeMustNotContainMultipleStatementsOnOneLineAnalyzer.DiagnosticId];

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
            var statement = root.FindNode(diagnostic.Location.SourceSpan).FirstAncestorOrSelf<StatementSyntax>();

            if (statement != null)
            {
                context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH0375Title,
                                                          token => ApplyCodeFixAsync(context.Document, statement, token),
                                                          nameof(RH0375CodeMustNotContainMultipleStatementsOnOneLineCodeFixProvider)),
                                        diagnostic);
            }
        }
    }

    #endregion // CodeFixProvider
}