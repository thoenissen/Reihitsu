using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Organization;

/// <summary>
/// Code fix provider for <see cref="RH7107PropertyAccessorsMustFollowOrderAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH7107PropertyAccessorsMustFollowOrderCodeFixProvider))]
public class RH7107PropertyAccessorsMustFollowOrderCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Creates the text change that moves the <c>get</c> accessor of a property or indexer in front of its <c>set</c> or
    /// <c>init</c> accessor, as <see cref="AccessorOrderingUtilities.TryCreateMove"/> performs it
    /// </summary>
    /// <param name="memberDeclaration">Property or indexer declaration</param>
    /// <param name="change">The text change that performs the move</param>
    /// <returns><see langword="true"/> if an accessor has to move and the move can be performed; otherwise, <see langword="false"/></returns>
    private static bool TryCreateMove(MemberDeclarationSyntax memberDeclaration, out TextChange change)
    {
        var accessorList = TryGetAccessorList(memberDeclaration);

        if (accessorList == null
            || AccessorOrderingUtilities.TryGetAccessorMove(accessorList,
                                                            SyntaxKind.GetAccessorDeclaration,
                                                            new[] { SyntaxKind.SetAccessorDeclaration, SyntaxKind.InitAccessorDeclaration },
                                                            out var accessorToMove,
                                                            out var targetAccessor) == false)
        {
            change = default;

            return false;
        }

        return AccessorOrderingUtilities.TryCreateMove(accessorList, accessorToMove, targetAccessor, out change);
    }

    /// <summary>
    /// Applies the code fix: the accessor moves, and nothing else of the declaration changes (SHARED-FIX-006,
    /// SHARED-FIX-007)
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="memberDeclaration">Member declaration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, MemberDeclarationSyntax memberDeclaration, CancellationToken cancellationToken)
    {
        if (TryCreateMove(memberDeclaration, out var change) == false)
        {
            return document;
        }

        var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);

        return document.WithText(text.WithChanges(change));
    }

    /// <summary>
    /// Tries to find the member declaration
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

    /// <summary>
    /// Gets the accessor list of the member declaration
    /// </summary>
    /// <param name="memberDeclaration">Member declaration</param>
    /// <returns>The accessor list, or <see langword="null"/> if the member declaration does not have one</returns>
    private static AccessorListSyntax TryGetAccessorList(MemberDeclarationSyntax memberDeclaration)
    {
        return memberDeclaration switch
               {
                   PropertyDeclarationSyntax propertyDeclaration => propertyDeclaration.AccessorList,
                   IndexerDeclarationSyntax indexerDeclaration => indexerDeclaration.AccessorList,
                   _ => null,
               };
    }

    /// <summary>
    /// Determines whether moving the out-of-order accessor preserves the meaning of the code. The move is
    /// refused when it would relocate a preprocessor directive away from the code it governs, since that splits a
    /// conditional-compilation pair; a directive pair contained completely in the moved or the crossed half
    /// travels intact and is allowed
    /// </summary>
    /// <param name="memberDeclaration">Member declaration</param>
    /// <returns><see langword="true"/> if the move is safe to offer</returns>
    private static bool IsMoveSafe(MemberDeclarationSyntax memberDeclaration)
    {
        var accessorList = TryGetAccessorList(memberDeclaration);

        return accessorList != null
               && AccessorOrderingUtilities.TryGetAccessorMove(accessorList,
                                                               SyntaxKind.GetAccessorDeclaration,
                                                               new[] { SyntaxKind.SetAccessorDeclaration, SyntaxKind.InitAccessorDeclaration },
                                                               out var accessorToMove,
                                                               out var targetAccessor)
               && AccessorOrderingUtilities.MoveRangeContainsDirectives(accessorList, accessorToMove, targetAccessor) == false;
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH7107PropertyAccessorsMustFollowOrderAnalyzer.DiagnosticId];

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
                if (TryGetMemberDeclaration(root, diagnostic, out var memberDeclaration)
                    && IsMoveSafe(memberDeclaration)
                    && TryCreateMove(memberDeclaration, out _))
                {
                    context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH7107Title,
                                                              token => ApplyCodeFixAsync(context.Document, memberDeclaration, token),
                                                              nameof(RH7107PropertyAccessorsMustFollowOrderCodeFixProvider)),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // CodeFixProvider
}