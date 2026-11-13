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
/// Code fix provider for <see cref="RH7108EventAccessorsMustFollowOrderAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH7108EventAccessorsMustFollowOrderCodeFixProvider))]
public class RH7108EventAccessorsMustFollowOrderCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Creates the text change that moves the <c>add</c> accessor of an event declaration in front of its <c>remove</c>
    /// accessor, as <see cref="AccessorOrderingUtilities.TryCreateMove"/> performs it
    /// </summary>
    /// <param name="eventDeclaration">Event declaration</param>
    /// <param name="change">The text change that performs the move</param>
    /// <returns><see langword="true"/> if an accessor has to move and the move can be performed; otherwise, <see langword="false"/></returns>
    private static bool TryCreateMove(EventDeclarationSyntax eventDeclaration, out TextChange change)
    {
        if (eventDeclaration.AccessorList == null
            || AccessorOrderingUtilities.TryGetAccessorMove(eventDeclaration.AccessorList,
                                                            SyntaxKind.AddAccessorDeclaration,
                                                            new[] { SyntaxKind.RemoveAccessorDeclaration },
                                                            out var accessorToMove,
                                                            out var targetAccessor) == false)
        {
            change = default;

            return false;
        }

        return AccessorOrderingUtilities.TryCreateMove(eventDeclaration.AccessorList, accessorToMove, targetAccessor, out change);
    }

    /// <summary>
    /// Applies the code fix: the accessor moves, and nothing else of the declaration changes (SHARED-FIX-006,
    /// SHARED-FIX-007)
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="eventDeclaration">Event declaration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, EventDeclarationSyntax eventDeclaration, CancellationToken cancellationToken)
    {
        if (TryCreateMove(eventDeclaration, out var change) == false)
        {
            return document;
        }

        var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);

        return document.WithText(text.WithChanges(change));
    }

    /// <summary>
    /// Determines whether moving the out-of-order accessor preserves the meaning of the code. The move is
    /// refused when it would relocate a preprocessor directive away from the code it governs, since that splits a
    /// conditional-compilation pair; a directive pair contained completely in the moved or the crossed half
    /// travels intact and is allowed
    /// </summary>
    /// <param name="eventDeclaration">Event declaration</param>
    /// <returns><see langword="true"/> if the move is safe to offer</returns>
    private static bool IsMoveSafe(EventDeclarationSyntax eventDeclaration)
    {
        return eventDeclaration.AccessorList != null
               && AccessorOrderingUtilities.TryGetAccessorMove(eventDeclaration.AccessorList,
                                                               SyntaxKind.AddAccessorDeclaration,
                                                               new[] { SyntaxKind.RemoveAccessorDeclaration },
                                                               out var accessorToMove,
                                                               out var targetAccessor)
               && AccessorOrderingUtilities.MoveRangeContainsDirectives(eventDeclaration.AccessorList, accessorToMove, targetAccessor) == false;
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH7108EventAccessorsMustFollowOrderAnalyzer.DiagnosticId];

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
                if (root.FindToken(diagnostic.Location.SourceSpan.Start).Parent?.AncestorsAndSelf().OfType<EventDeclarationSyntax>().FirstOrDefault() is EventDeclarationSyntax eventDeclaration
                    && IsMoveSafe(eventDeclaration)
                    && TryCreateMove(eventDeclaration, out _))
                {
                    context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH7108Title,
                                                              token => ApplyCodeFixAsync(context.Document, eventDeclaration, token),
                                                              nameof(RH7108EventAccessorsMustFollowOrderCodeFixProvider)),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // CodeFixProvider
}