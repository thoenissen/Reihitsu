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

using Reihitsu.Formatter;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// Code fix provider for <see cref="RH0810SimpleAutoPropertiesShouldBeSingleLinedAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0810SimpleAutoPropertiesShouldBeSingleLinedCodeFixProvider))]
public class RH0810SimpleAutoPropertiesShouldBeSingleLinedCodeFixProvider : CodeFixProvider
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
    /// Determines whether the given node contains comments or directives
    /// </summary>
    /// <param name="node">The node to inspect</param>
    /// <returns><see langword="true"/> if comments or directives are present; otherwise, <see langword="false"/></returns>
    private static bool HasCommentsOrDirectives(SyntaxNode node)
    {
        foreach (var trivia in node.DescendantTrivia(descendIntoTrivia: true))
        {
            if (trivia.IsDirective || trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) || trivia.IsKind(SyntaxKind.MultiLineCommentTrivia))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether the given text span occupies a single line
    /// </summary>
    /// <param name="syntaxTree">Syntax tree</param>
    /// <param name="span">Text span</param>
    /// <returns><see langword="true"/> if the span occupies a single line; otherwise, <see langword="false"/></returns>
    private static bool IsSingleLineSpan(SyntaxTree syntaxTree, TextSpan span)
    {
        var lineSpan = syntaxTree.GetLineSpan(span);

        return lineSpan.StartLinePosition.Line == lineSpan.EndLinePosition.Line;
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
        if (propertyDeclaration.AccessorList == null || HasCommentsOrDirectives(propertyDeclaration.AccessorList))
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

        if (IsSingleLineSpan(propertyDeclaration.SyntaxTree, TextSpan.FromBounds(signatureStartToken.SpanStart, tokenBeforeOpenBrace.Span.End)) == false)
        {
            return false;
        }

        if (propertyDeclaration.Initializer != null)
        {
            if (HasCommentsOrDirectives(propertyDeclaration.Initializer))
            {
                return false;
            }

            if (IsSingleLineSpan(propertyDeclaration.SyntaxTree, propertyDeclaration.Initializer.Value.Span) == false)
            {
                return false;
            }
        }

        return true;
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH0810SimpleAutoPropertiesShouldBeSingleLinedAnalyzer.DiagnosticId];

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
                context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH0810Title,
                                                          token => ApplyCodeFixAsync(context.Document, propertyDeclaration, token),
                                                          nameof(RH0810SimpleAutoPropertiesShouldBeSingleLinedCodeFixProvider)),
                                        diagnostic);
            }
        }
    }

    #endregion // CodeFixProvider
}