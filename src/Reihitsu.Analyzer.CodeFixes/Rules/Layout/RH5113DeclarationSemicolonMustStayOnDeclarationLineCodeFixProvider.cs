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

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.CodeFixes.Core;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Core;
using Reihitsu.Formatter;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Code fix provider for <see cref="RH5113DeclarationSemicolonMustStayOnDeclarationLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5113DeclarationSemicolonMustStayOnDeclarationLineCodeFixProvider))]
public class RH5113DeclarationSemicolonMustStayOnDeclarationLineCodeFixProvider : ReihitsuCodeFixProviderBase
{
    #region Methods

    /// <summary>
    /// Applies the code fix. A declaration that shares its line with another member is first separated from it, so that both
    /// stand on lines of their own (SHARED-FIX-007); then the semicolon is moved directly behind the end of the declaration,
    /// and the rest of the declaration stays as written (RH5113-003)
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="declarationNode">Declaration node containing the stray semicolon</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    private static Task<Document> ApplyCodeFixAsync(Document document, MemberDeclarationSyntax declarationNode, CancellationToken cancellationToken)
    {
        return BlankLineCodeFixUtilities.FormatMemberOnLinesOfItsOwnAsync(document, declarationNode, MoveSemicolonAsync, cancellationToken);
    }

    /// <summary>
    /// Moves the declaration's semicolon directly behind the token in front of it, as the formatter joins it (FMT-BREAK-009).
    /// The gap holds nothing but whitespace and line breaks, which the fix registers only for (SHARED-JOIN-001)
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="declarationNode">Declaration node containing the stray semicolon, which belongs to <paramref name="document"/></param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    private static async Task<Document> MoveSemicolonAsync(Document document, SyntaxNode declarationNode, CancellationToken cancellationToken)
    {
        var semicolonToken = GetSemicolonToken(declarationNode);
        var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);

        return document.WithText(sourceText.Replace(TextSpan.FromBounds(semicolonToken.GetPreviousToken().Span.End, semicolonToken.SpanStart), string.Empty));
    }

    /// <summary>
    /// Gets the declaration node that owns the stray semicolon
    /// </summary>
    /// <param name="diagnosticNode">Diagnostic node</param>
    /// <returns>Declaration node, or <see langword="null"/> when no supported declaration is found</returns>
    private static MemberDeclarationSyntax GetDeclarationNode(SyntaxNode diagnosticNode)
    {
        return ((diagnosticNode.FirstAncestorOrSelf<FieldDeclarationSyntax>()
                     ?? (MemberDeclarationSyntax)diagnosticNode.FirstAncestorOrSelf<EventFieldDeclarationSyntax>())
                    ?? diagnosticNode.FirstAncestorOrSelf<DelegateDeclarationSyntax>())
                   ?? diagnosticNode.FirstAncestorOrSelf<PropertyDeclarationSyntax>();
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
                   PropertyDeclarationSyntax { Initializer: not null } propertyDeclaration => propertyDeclaration.SemicolonToken,
                   _ => default,
               };
    }

    /// <summary>
    /// Determines whether the stray semicolon can be moved behind the token in front of it. The fix is not offered when a
    /// comment sits between the previous token and the semicolon, because removing the gap would absorb the semicolon into
    /// the comment (SHARED-FIX-003)
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

    #endregion // CodeFixProvider

    #region ReihitsuCodeFixProviderBase

    /// <inheritdoc/>
    /// <remarks>
    /// The fix moves only the semicolon and lays out nothing through the formatter (RH5113-003), but it is still withheld in a
    /// document with a syntax error, as a decision of its own (RH5113-004)
    /// </remarks>
    protected override bool FormatsThroughFormatter => true;

    /// <inheritdoc/>
    protected sealed override async Task RegisterCodeFixesCoreAsync(CodeFixContext context)
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

            // A declaration that holds a shape the formatter refuses is not fixed either, although no formatter lays it out (RH5113-004)
            if (CanApplyCodeFix(semicolonToken) == false
                || ReihitsuFormatter.FindUnsupportedSyntax(declarationNode) != null)
            {
                continue;
            }

            context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH5113Title,
                                                      token => ApplyCodeFixAsync(context.Document, declarationNode, token),
                                                      nameof(RH5113DeclarationSemicolonMustStayOnDeclarationLineCodeFixProvider)),
                                    diagnostic);
        }
    }

    #endregion // ReihitsuCodeFixProviderBase
}