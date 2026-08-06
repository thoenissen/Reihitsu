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

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Core;
using Reihitsu.Formatter;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Code fix provider for <see cref="RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5408SimpleAutoPropertiesShouldBeSingleLinedCodeFixProvider))]
public class RH5408SimpleAutoPropertiesShouldBeSingleLinedCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applies the code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="propertyDeclaration">Property declaration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, PropertyDeclarationSyntax propertyDeclaration, CancellationToken cancellationToken)
    {
        return await ReihitsuFormatter.FormatNodeInDocumentAsync(document, propertyDeclaration, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the first token of the property signature while skipping property-level attributes
    /// </summary>
    /// <param name="propertyDeclaration">The property declaration to inspect</param>
    /// <returns>The first signature token</returns>
    private static SyntaxToken GetSingleLineSignatureStartToken(PropertyDeclarationSyntax propertyDeclaration)
    {
        if (propertyDeclaration.Modifiers.Count > 0)
        {
            return propertyDeclaration.Modifiers[0];
        }

        return propertyDeclaration.Type.GetFirstToken();
    }

    /// <summary>
    /// Determines whether the formatter can safely collapse the auto-property to a single line
    /// </summary>
    /// <param name="propertyDeclaration">The property declaration to inspect</param>
    /// <returns><see langword="true"/> if the code fix can be applied safely; otherwise, <see langword="false"/></returns>
    private static bool CanApplyCodeFix(PropertyDeclarationSyntax propertyDeclaration)
    {
        // The guard is interior-scoped to match the formatter's collapse: a comment trailing the accessor
        // list's closing brace is never crossed, so it must not block the fix (see issue #604).
        if (propertyDeclaration.AccessorList == null || SyntaxNodeUtilities.InteriorContainsCommentOrDirective(propertyDeclaration.AccessorList))
        {
            return false;
        }

        foreach (var accessor in propertyDeclaration.AccessorList.Accessors)
        {
            if (accessor.Body != null || accessor.ExpressionBody != null)
            {
                return false;
            }
        }

        var tokenBeforeOpenBrace = propertyDeclaration.AccessorList.OpenBraceToken.GetPreviousToken();
        var signatureStartToken = GetSingleLineSignatureStartToken(propertyDeclaration);

        if (signatureStartToken == default
            || signatureStartToken.IsKind(SyntaxKind.None)
            || tokenBeforeOpenBrace == default
            || tokenBeforeOpenBrace.IsKind(SyntaxKind.None))
        {
            return false;
        }

        // A comment or directive in the gap between the signature and the accessor brace makes the formatter
        // refuse the collapse, so registering the fix here would produce a no-op action. Guard the same gap.
        if (SyntaxTriviaUtilities.WouldJoinAcrossUnjoinableTrivia(tokenBeforeOpenBrace, propertyDeclaration.AccessorList.OpenBraceToken))
        {
            return false;
        }

        if (SyntaxNodeUtilities.IsSingleLineSpan(propertyDeclaration.SyntaxTree, TextSpan.FromBounds(signatureStartToken.SpanStart, tokenBeforeOpenBrace.Span.End)) == false)
        {
            return false;
        }

        if (propertyDeclaration.Initializer != null)
        {
            if (SyntaxNodeUtilities.ContainsCommentOrDirective(propertyDeclaration.Initializer))
            {
                return false;
            }

            // A comment or directive between the accessor list and an initializer that starts on a later line makes
            // the formatter refuse to join the initializer, so registering the fix here would produce a no-op
            // action. Guard the same gap, and only when the gap actually spans lines.
            var initializerGapSpan = TextSpan.FromBounds(propertyDeclaration.AccessorList.CloseBraceToken.Span.End, propertyDeclaration.Initializer.EqualsToken.SpanStart);

            if (SyntaxNodeUtilities.IsSingleLineSpan(propertyDeclaration.SyntaxTree, initializerGapSpan) == false
                && SyntaxTriviaUtilities.WouldJoinAcrossUnjoinableTrivia(propertyDeclaration.AccessorList.CloseBraceToken, propertyDeclaration.Initializer.EqualsToken))
            {
                return false;
            }

            if (SyntaxNodeUtilities.IsSingleLineSpan(propertyDeclaration.SyntaxTree, propertyDeclaration.Initializer.Value.Span) == false)
            {
                return false;
            }

            // A comment or directive between the initializer value and a terminating semicolon that sits on its own
            // line makes the formatter refuse to join the semicolon, so registering the fix here would produce a
            // no-op action. Guard the same gap, and only when the gap actually spans lines.
            var tokenBeforeSemicolon = propertyDeclaration.SemicolonToken.GetPreviousToken();

            if (propertyDeclaration.SemicolonToken.IsMissing == false
                && tokenBeforeSemicolon.IsKind(SyntaxKind.None) == false
                && SyntaxNodeUtilities.IsSingleLineSpan(propertyDeclaration.SyntaxTree, TextSpan.FromBounds(tokenBeforeSemicolon.Span.End, propertyDeclaration.SemicolonToken.SpanStart)) == false
                && SyntaxTriviaUtilities.WouldJoinAcrossUnjoinableTrivia(tokenBeforeSemicolon, propertyDeclaration.SemicolonToken))
            {
                return false;
            }
        }

        return true;
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzer.DiagnosticId];

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
            if (root.FindNode(diagnostic.Location.SourceSpan) is PropertyDeclarationSyntax propertyDeclaration
                && CanApplyCodeFix(propertyDeclaration))
            {
                context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH5408Title,
                                                          token => ApplyCodeFixAsync(context.Document, propertyDeclaration, token),
                                                          nameof(RH5408SimpleAutoPropertiesShouldBeSingleLinedCodeFixProvider)),
                                        diagnostic);
            }
        }
    }

    #endregion // CodeFixProvider
}