using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.Core;
using Reihitsu.Formatter;

namespace Reihitsu.Analyzer.Rules.Ordering;

/// <summary>
/// Code fix provider for <see cref="RH0611EventAccessorsMustFollowOrderAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0611EventAccessorsMustFollowOrderCodeFixProvider))]
public class RH0611EventAccessorsMustFollowOrderCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applies the code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="eventDeclaration">Event declaration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, EventDeclarationSyntax eventDeclaration, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null
            || eventDeclaration.AccessorList == null
            || AccessorOrderingUtilities.TryGetAccessorMove(eventDeclaration.AccessorList,
                                                            SyntaxKind.AddAccessorDeclaration,
                                                            new[] { SyntaxKind.RemoveAccessorDeclaration },
                                                            out var accessorToMove,
                                                            out var targetAccessor) == false)
        {
            return document;
        }

        var formattingAnnotation = new SyntaxAnnotation();
        var updatedEventDeclaration = eventDeclaration.WithAccessorList(AccessorOrderingUtilities.MoveAccessorBefore(eventDeclaration.AccessorList, accessorToMove, targetAccessor))
                                                      .WithAdditionalAnnotations(formattingAnnotation);
        var updatedRoot = root.ReplaceNode(eventDeclaration, updatedEventDeclaration);
        var updatedDocument = document.WithSyntaxRoot(updatedRoot);
        var formattedRoot = await updatedDocument.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var formattedEventDeclaration = formattedRoot?.GetAnnotatedNodes(formattingAnnotation).OfType<EventDeclarationSyntax>().FirstOrDefault();

        return formattedEventDeclaration == null
                   ? updatedDocument
                   : await ReihitsuFormatter.FormatNodeInDocumentAsync(updatedDocument, formattedEventDeclaration, cancellationToken).ConfigureAwait(false);
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH0611EventAccessorsMustFollowOrderAnalyzer.DiagnosticId];

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
                if (root.FindToken(diagnostic.Location.SourceSpan.Start).Parent?.AncestorsAndSelf().OfType<EventDeclarationSyntax>().FirstOrDefault() is EventDeclarationSyntax eventDeclaration)
                {
                    context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH0611Title,
                                                              token => ApplyCodeFixAsync(context.Document, eventDeclaration, token),
                                                              nameof(RH0611EventAccessorsMustFollowOrderCodeFixProvider)),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // CodeFixProvider
}