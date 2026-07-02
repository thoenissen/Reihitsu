using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Core;
using Reihitsu.Formatter;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Code fix provider for <see cref="RH5113DeclarationSemicolonMustStayOnDeclarationLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5113DeclarationSemicolonMustStayOnDeclarationLineCodeFixProvider))]
public class RH5113DeclarationSemicolonMustStayOnDeclarationLineCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applies the code fix by delegating layout to the formatter
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="declarationNode">Declaration node containing the stray semicolon</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, SyntaxNode declarationNode, CancellationToken cancellationToken)
    {
        return await ReihitsuFormatter.FormatNodeInDocumentAsync(document, declarationNode, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the declaration node that owns the stray semicolon
    /// </summary>
    /// <param name="diagnosticNode">Diagnostic node</param>
    /// <returns>Declaration node, or <see langword="null"/> when no supported declaration is found</returns>
    private static SyntaxNode GetDeclarationNode(SyntaxNode diagnosticNode)
    {
        return (diagnosticNode.FirstAncestorOrSelf<FieldDeclarationSyntax>()
                    ?? (SyntaxNode)diagnosticNode.FirstAncestorOrSelf<EventFieldDeclarationSyntax>())
                   ?? diagnosticNode.FirstAncestorOrSelf<DelegateDeclarationSyntax>();
    }

    /// <summary>
    /// Gets the terminating semicolon for a supported declaration kind
    /// </summary>
    /// <param name="declarationNode">Declaration node</param>
    /// <returns>The semicolon token, or <see langword="default"/> when the declaration kind is not supported</returns>
    private static SyntaxToken GetSemicolonToken(SyntaxNode declarationNode)
    {
        return declarationNode switch
               {
                   FieldDeclarationSyntax field => field.SemicolonToken,
                   EventFieldDeclarationSyntax eventField => eventField.SemicolonToken,
                   DelegateDeclarationSyntax delegateDeclaration => delegateDeclaration.SemicolonToken,
                   _ => default,
               };
    }

    /// <summary>
    /// Determines whether the formatter can safely collapse the stray semicolon. The fix is not offered
    /// when a comment sits between the previous token and the semicolon, because joining the lines would
    /// absorb the semicolon into the comment. Mirrors the per-token guard used by the formatter's
    /// line-break rewriter so the fix is offered whenever the formatter would also collapse the join
    /// </summary>
    /// <param name="semicolonToken">Semicolon token</param>
    /// <returns><see langword="true"/> if the code fix can be applied safely; otherwise, <see langword="false"/></returns>
    private static bool CanApplyCodeFix(SyntaxToken semicolonToken)
    {
        if (semicolonToken.IsKind(SyntaxKind.None) || semicolonToken.IsMissing)
        {
            return false;
        }

        var previousToken = semicolonToken.GetPreviousToken();

        if (previousToken.IsKind(SyntaxKind.None))
        {
            return false;
        }

        return SyntaxNodeUtilities.GapContainsComment(previousToken, semicolonToken) == false;
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH5113DeclarationSemicolonMustStayOnDeclarationLineAnalyzer.DiagnosticId];

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
            var diagnosticNode = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
            var declarationNode = GetDeclarationNode(diagnosticNode);

            if (declarationNode == null)
            {
                continue;
            }

            var semicolonToken = GetSemicolonToken(declarationNode);

            if (CanApplyCodeFix(semicolonToken) == false)
            {
                continue;
            }

            context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH5113Title,
                                                      token => ApplyCodeFixAsync(context.Document, declarationNode, token),
                                                      nameof(RH5113DeclarationSemicolonMustStayOnDeclarationLineCodeFixProvider)),
                                    diagnostic);
        }
    }

    #endregion // CodeFixProvider
}