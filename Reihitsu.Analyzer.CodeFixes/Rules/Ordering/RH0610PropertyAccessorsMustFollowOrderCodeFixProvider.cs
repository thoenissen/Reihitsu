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
/// Code fix provider for <see cref="RH0610PropertyAccessorsMustFollowOrderAnalyzer"/>.
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0610PropertyAccessorsMustFollowOrderCodeFixProvider))]
public class RH0610PropertyAccessorsMustFollowOrderCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applies the code fix.
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="memberDeclaration">Member declaration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, MemberDeclarationSyntax memberDeclaration, CancellationToken cancellationToken)
    {
        var accessorList = memberDeclaration switch
                           {
                               PropertyDeclarationSyntax propertyDeclaration => propertyDeclaration.AccessorList,
                               IndexerDeclarationSyntax indexerDeclaration => indexerDeclaration.AccessorList,
                               _ => null,
                           };
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (accessorList == null
            || root == null
            || AccessorOrderingUtilities.TryGetAccessorMove(accessorList,
                                                            SyntaxKind.GetAccessorDeclaration,
                                                            new[] { SyntaxKind.SetAccessorDeclaration, SyntaxKind.InitAccessorDeclaration },
                                                            out var accessorToMove,
                                                            out var targetAccessor) == false)
        {
            return document;
        }

        var updatedAccessorList = AccessorOrderingUtilities.MoveAccessorBefore(accessorList, accessorToMove, targetAccessor);
        var formattingAnnotation = new SyntaxAnnotation();
        var updatedMemberDeclaration = memberDeclaration switch
                                       {
                                           PropertyDeclarationSyntax propertyDeclaration => propertyDeclaration.WithAccessorList(updatedAccessorList),
                                           IndexerDeclarationSyntax indexerDeclaration => indexerDeclaration.WithAccessorList(updatedAccessorList),
                                           _ => memberDeclaration,
                                       };
        updatedMemberDeclaration = updatedMemberDeclaration.WithAdditionalAnnotations(formattingAnnotation);

        var updatedRoot = root.ReplaceNode(memberDeclaration, updatedMemberDeclaration);
        var updatedDocument = document.WithSyntaxRoot(updatedRoot);
        var formattedRoot = await updatedDocument.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var formattedMemberDeclaration = formattedRoot?.GetAnnotatedNodes(formattingAnnotation).OfType<MemberDeclarationSyntax>().FirstOrDefault();

        return formattedMemberDeclaration == null
                   ? updatedDocument
                   : await ReihitsuFormatter.FormatNodeInDocumentAsync(updatedDocument, formattedMemberDeclaration, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Tries to find the member declaration.
    /// </summary>
    /// <param name="root">Root</param>
    /// <param name="diagnostic">Diagnostic</param>
    /// <param name="memberDeclaration">Member declaration</param>
    /// <returns><see langword="true"/> if the declaration was found</returns>
    private static bool TryGetMemberDeclaration(SyntaxNode root, Diagnostic diagnostic, out MemberDeclarationSyntax memberDeclaration)
    {
        var diagnosticNode = root.FindToken(diagnostic.Location.SourceSpan.Start).Parent;

        memberDeclaration = diagnosticNode?.AncestorsAndSelf()
                                          .OfType<MemberDeclarationSyntax>()
                                          .FirstOrDefault(obj => obj is PropertyDeclarationSyntax or IndexerDeclarationSyntax);

        return memberDeclaration != null;
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH0610PropertyAccessorsMustFollowOrderAnalyzer.DiagnosticId];

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
                if (TryGetMemberDeclaration(root, diagnostic, out var memberDeclaration))
                {
                    context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH0610Title,
                                                              token => ApplyCodeFixAsync(context.Document, memberDeclaration, token),
                                                              nameof(RH0610PropertyAccessorsMustFollowOrderCodeFixProvider)),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // CodeFixProvider
}