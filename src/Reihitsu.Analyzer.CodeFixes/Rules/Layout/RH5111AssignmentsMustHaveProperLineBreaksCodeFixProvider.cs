using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Core;
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
        var formattingNode = GetFormattingNode(diagnosticNode);

        if (formattingNode == null)
        {
            return document;
        }

        var contextNode = GetFormattingContextNode(formattingNode);

        return contextNode == formattingNode
                   ? await ReihitsuFormatter.FormatNodeInDocumentAsync(document, formattingNode, cancellationToken).ConfigureAwait(false)
                   : await ReihitsuFormatter.FormatNodeInDocumentWithContextAsync(document, formattingNode, contextNode, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the containing node whose tokens provide positional context while formatting
    /// </summary>
    /// <param name="formattingNode">The node whose formatted result will be applied</param>
    /// <returns>The node to use as formatting context</returns>
    private static SyntaxNode GetFormattingContextNode(SyntaxNode formattingNode)
    {
        return formattingNode switch
               {
                   VariableDeclaratorSyntax variableDeclarator => variableDeclarator.Parent?.Parent ?? formattingNode,
                   ParameterSyntax parameter => parameter.Parent ?? formattingNode,
                   EnumMemberDeclarationSyntax enumMember => enumMember.Parent ?? formattingNode,
                   _ => formattingNode
               };
    }

    /// <summary>
    /// Gets the narrowest formatting node that can still fix the assignment
    /// </summary>
    /// <param name="diagnosticNode">Diagnostic node</param>
    /// <returns>Formatting node</returns>
    private static SyntaxNode GetFormattingNode(SyntaxNode diagnosticNode)
    {
        return diagnosticNode.FirstAncestorOrSelf<AssignmentExpressionSyntax>()
                   ?? (SyntaxNode)diagnosticNode.FirstAncestorOrSelf<VariableDeclaratorSyntax>()
                   ?? diagnosticNode.FirstAncestorOrSelf<PropertyDeclarationSyntax>()
                   ?? (SyntaxNode)diagnosticNode.FirstAncestorOrSelf<ParameterSyntax>()
                   ?? diagnosticNode.FirstAncestorOrSelf<EnumMemberDeclarationSyntax>();
    }

    /// <summary>
    /// Determines whether the formatting node carries trivia that blocks the line join. A variable declarator
    /// is inspected with its full trivia, because the declaration's own comment sits inside the join. An
    /// assignment expression or a property declaration is inspected by its own span instead: the member's
    /// documentation comment precedes it and the join never reaches it, so counting it would withhold a fix
    /// from a documented member that an undocumented one still gets
    /// </summary>
    /// <param name="node">The formatting node to inspect</param>
    /// <returns><see langword="true"/> if the join is blocked; otherwise, <see langword="false"/></returns>
    private static bool ContainsJoinBlockingTrivia(SyntaxNode node)
    {
        return node is VariableDeclaratorSyntax
                   ? SyntaxNodeUtilities.ContainsCommentOrDirective(node)
                   : SyntaxNodeUtilities.InteriorContainsCommentOrDirective(node);
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

            if (assignmentNode == null || ContainsJoinBlockingTrivia(assignmentNode))
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