using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Formatter;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// Code fix provider for <see cref="RH0396UsingDeclarationsShouldNotBeUsedAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0396UsingDeclarationsShouldNotBeUsedCodeFixProvider))]
public class RH0396UsingDeclarationsShouldNotBeUsedCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applies the code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="usingDeclaration">Using declaration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, LocalDeclarationStatementSyntax usingDeclaration, CancellationToken cancellationToken)
    {
        if (usingDeclaration.Parent is not BlockSyntax parentBlock)
        {
            return document;
        }

        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        var statementIndex = parentBlock.Statements.IndexOf(usingDeclaration);

        if (statementIndex < 0)
        {
            return document;
        }

        var followingStatements = parentBlock.Statements.Skip(statementIndex + 1);
        var formattingAnnotation = new SyntaxAnnotation();
        var newUsingStatement = SyntaxFactory.UsingStatement(usingDeclaration.Declaration, null, SyntaxFactory.Block(followingStatements))
                                             .WithAwaitKeyword(usingDeclaration.AwaitKeyword)
                                             .WithLeadingTrivia(usingDeclaration.GetLeadingTrivia())
                                             .WithTrailingTrivia(usingDeclaration.GetTrailingTrivia())
                                             .WithAdditionalAnnotations(formattingAnnotation);
        var updatedStatements = SyntaxFactory.List(parentBlock.Statements.Take(statementIndex).Concat([newUsingStatement]));
        var updatedBlock = parentBlock.WithStatements(updatedStatements);
        var updatedRoot = root.ReplaceNode(parentBlock, updatedBlock);
        var updatedDocument = document.WithSyntaxRoot(updatedRoot);
        var formattedRoot = await updatedDocument.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var formattedUsingStatement = formattedRoot?.GetAnnotatedNodes(formattingAnnotation).OfType<UsingStatementSyntax>().FirstOrDefault();

        return formattedUsingStatement == null
                   ? updatedDocument
                   : await ReihitsuFormatter.FormatNodeInDocumentAsync(updatedDocument, formattedUsingStatement, cancellationToken).ConfigureAwait(false);
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH0396UsingDeclarationsShouldNotBeUsedAnalyzer.DiagnosticId];

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
            var usingDeclaration = root.FindToken(diagnostic.Location.SourceSpan.Start).Parent?.AncestorsAndSelf()
                                       .OfType<LocalDeclarationStatementSyntax>()
                                       .FirstOrDefault();

            if (usingDeclaration?.Parent is BlockSyntax)
            {
                context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH0396Title,
                                                          token => ApplyCodeFixAsync(context.Document, usingDeclaration, token),
                                                          nameof(RH0396UsingDeclarationsShouldNotBeUsedCodeFixProvider)),
                                        diagnostic);
            }
        }
    }

    #endregion // CodeFixProvider
}