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
using Reihitsu.Formatter;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Code fix provider for <see cref="RH5111AssignmentsMustHaveProperLineBreaksAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5111AssignmentsMustHaveProperLineBreaksCodeFixProvider))]
public class RH5111AssignmentsMustHaveProperLineBreaksCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applies the code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="diagnosticSpan">Diagnostic span</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, TextSpan diagnosticSpan, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        var diagnosticNode = root.FindNode(diagnosticSpan, getInnermostNodeForTie: true);
        var assignmentNode = GetFormattingNode(diagnosticNode);

        return assignmentNode == null
                   ? document
                   : await ReihitsuFormatter.FormatNodeInDocumentAsync(document, assignmentNode, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the narrowest formatting node that can still fix the assignment
    /// </summary>
    /// <param name="diagnosticNode">Diagnostic node</param>
    /// <returns>Formatting node</returns>
    private static SyntaxNode GetFormattingNode(SyntaxNode diagnosticNode)
    {
        var assignmentExpression = diagnosticNode.FirstAncestorOrSelf<AssignmentExpressionSyntax>();

        if (assignmentExpression != null)
        {
            return assignmentExpression;
        }

        var variableDeclarator = diagnosticNode.FirstAncestorOrSelf<VariableDeclaratorSyntax>();

        if (variableDeclarator != null)
        {
            return (SyntaxNode)variableDeclarator.FirstAncestorOrSelf<LocalDeclarationStatementSyntax>()
                       ?? (SyntaxNode)variableDeclarator.FirstAncestorOrSelf<FieldDeclarationSyntax>()
                       ?? (SyntaxNode)variableDeclarator.FirstAncestorOrSelf<EventFieldDeclarationSyntax>()
                       ?? variableDeclarator;
        }

        return diagnosticNode.FirstAncestorOrSelf<PropertyDeclarationSyntax>();
    }

    /// <summary>
    /// Determines whether the formatting node carries comments or directives. A line join across a
    /// comment would move code into the comment, so the fix is not offered in that case (issue #226)
    /// </summary>
    /// <param name="node">The node to inspect</param>
    /// <returns><see langword="true"/> if comments or directives are present; otherwise, <see langword="false"/></returns>
    private static bool HasCommentsOrDirectives(SyntaxNode node)
    {
        foreach (var trivia in node.DescendantTrivia(descendIntoTrivia: true))
        {
            if (trivia.IsDirective
                || trivia.IsKind(SyntaxKind.SingleLineCommentTrivia)
                || trivia.IsKind(SyntaxKind.MultiLineCommentTrivia)
                || trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)
                || trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia))
            {
                return true;
            }
        }

        return false;
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId];

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
            var assignmentNode = GetFormattingNode(diagnosticNode);

            if (assignmentNode == null || HasCommentsOrDirectives(assignmentNode))
            {
                continue;
            }

            context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH5111Title,
                                                      token => ApplyCodeFixAsync(context.Document, diagnostic.Location.SourceSpan, token),
                                                      nameof(RH5111AssignmentsMustHaveProperLineBreaksCodeFixProvider)),
                                    diagnostic);
        }
    }

    #endregion // CodeFixProvider
}