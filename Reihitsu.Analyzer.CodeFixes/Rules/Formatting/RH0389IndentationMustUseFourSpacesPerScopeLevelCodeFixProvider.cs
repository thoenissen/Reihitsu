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
/// Code fix provider for <see cref="RH0389IndentationMustUseFourSpacesPerScopeLevelAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0389IndentationMustUseFourSpacesPerScopeLevelCodeFixProvider))]
public class RH0389IndentationMustUseFourSpacesPerScopeLevelCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applies the code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="diagnostic">Diagnostic</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var diagnosticLine = diagnostic.Location.GetLineSpan().StartLinePosition.Line;

        if (root == null)
        {
            return document;
        }

        if (TryGetDirectiveTrivia(root, diagnostic, out var directiveTrivia))
        {
            var directiveOwner = directiveTrivia.Token.Parent;

            if (directiveOwner == null)
            {
                return document;
            }

            return await ApplyDirectiveIndentationFormattingAsync(document, directiveOwner, diagnosticLine, cancellationToken).ConfigureAwait(false);
        }

        if (TryGetFormattingScope(root, diagnostic, out var scope) == false)
        {
            return document;
        }

        return await ApplyIndentationFormattingAsync(document, scope, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Applies formatter indentation logic to a scope node and writes it back into the document
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="scope">Scope node</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated document</returns>
    private static Task<Document> ApplyIndentationFormattingAsync(Document document, SyntaxNode scope, CancellationToken cancellationToken)
    {
        return ReihitsuFormatter.FormatNodeInDocumentAsync(document, scope, cancellationToken);
    }

    /// <summary>
    /// Applies formatter-computed directive indentation to the reported line
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="anchorNode">Node whose formatter layout should anchor the directive</param>
    /// <param name="diagnosticLine">Reported line number</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated document</returns>
    private static async Task<Document> ApplyDirectiveIndentationFormattingAsync(Document document, SyntaxNode anchorNode, int diagnosticLine, CancellationToken cancellationToken)
    {
        var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
        var line = sourceText.Lines[diagnosticLine];
        var lineText = sourceText.ToString(line.Span);
        var firstNonWhitespaceIndex = 0;
        var directiveColumn = ComputeBaseIndentLevel(anchorNode) * 4;

        while (firstNonWhitespaceIndex < lineText.Length && char.IsWhiteSpace(lineText[firstNonWhitespaceIndex]))
        {
            firstNonWhitespaceIndex++;
        }

        var replacementSpan = TextSpan.FromBounds(line.Start, line.Start + firstNonWhitespaceIndex);

        return document.WithText(sourceText.Replace(replacementSpan, new string(' ', directiveColumn)));
    }

    /// <summary>
    /// Gets the start line of a token
    /// </summary>
    /// <param name="token">Token</param>
    /// <returns>0-based line number</returns>
    private static int GetStartLine(SyntaxToken token)
    {
        return token.GetLocation().GetLineSpan().StartLinePosition.Line;
    }

    /// <summary>
    /// Tries to locate a directive trivia at the diagnostic position
    /// </summary>
    /// <param name="root">Root node</param>
    /// <param name="diagnostic">Diagnostic</param>
    /// <param name="directiveTrivia">Directive trivia</param>
    /// <returns><see langword="true"/> if a directive trivia was found</returns>
    private static bool TryGetDirectiveTrivia(SyntaxNode root, Diagnostic diagnostic, out SyntaxTrivia directiveTrivia)
    {
        var position = diagnostic.Location.SourceSpan.Start;
        var token = root.FindToken(position, findInsideTrivia: true);
        var directiveSyntax = token.Parent?.AncestorsAndSelf().OfType<DirectiveTriviaSyntax>().FirstOrDefault();

        if (directiveSyntax != null)
        {
            directiveTrivia = directiveSyntax.ParentTrivia;

            return true;
        }

        directiveTrivia = token.LeadingTrivia.Concat(token.TrailingTrivia)
                                             .FirstOrDefault(obj => obj.HasStructure
                                                                    && obj.GetStructure() is DirectiveTriviaSyntax
                                                                    && obj.FullSpan.Contains(position));

        return directiveTrivia != default;
    }

    /// <summary>
    /// Finds the smallest formatting scope whose anchor line differs from the reported line
    /// </summary>
    /// <param name="root">Root node</param>
    /// <param name="diagnostic">Diagnostic</param>
    /// <param name="scope">Scope to format</param>
    /// <returns><see langword="true"/> if a scope was found</returns>
    private static bool TryGetFormattingScope(SyntaxNode root, Diagnostic diagnostic, out SyntaxNode scope)
    {
        var diagnosticTrivia = root.FindTrivia(diagnostic.Location.SourceSpan.Start, findInsideTrivia: true);
        var token = root.FindToken(diagnostic.Location.SourceSpan.Start, findInsideTrivia: true);
        var diagnosticNode = token.Parent;

        if (diagnosticNode == null)
        {
            scope = null;

            return false;
        }

        var diagnosticLine = diagnostic.Location.GetLineSpan().StartLinePosition.Line;
        var allowFollowingAnchor = IsCommentTrivia(diagnosticTrivia);

        for (var current = diagnosticNode; current != null; current = current.Parent)
        {
            var startLine = GetStartLine(current.GetFirstToken());

            if ((allowFollowingAnchor && startLine != diagnosticLine)
                || (allowFollowingAnchor == false && startLine < diagnosticLine))
            {
                scope = current;

                return true;
            }
        }

        scope = diagnosticNode;

        return true;
    }

    /// <summary>
    /// Determines whether the trivia is a comment
    /// </summary>
    /// <param name="trivia">Trivia</param>
    /// <returns><see langword="true"/> if the trivia is a comment</returns>
    private static bool IsCommentTrivia(SyntaxTrivia trivia)
    {
        return trivia.IsKind(SyntaxKind.SingleLineCommentTrivia)
               || trivia.IsKind(SyntaxKind.MultiLineCommentTrivia)
               || trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)
               || trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia);
    }

    /// <summary>
    /// Computes the base indentation level for a node from its containing constructs
    /// </summary>
    /// <param name="node">Node</param>
    /// <returns>Indentation level</returns>
    private static int ComputeBaseIndentLevel(SyntaxNode node)
    {
        var spanStart = node.SpanStart;
        var level = 0;

        for (var current = node.Parent; current != null; current = current.Parent)
        {
            if (IsIndentingAncestor(current, spanStart))
            {
                level++;
            }
        }

        return level;
    }

    /// <summary>
    /// Determines whether an ancestor contributes one indentation level
    /// </summary>
    /// <param name="node">Ancestor node</param>
    /// <param name="spanStart">Position being inspected</param>
    /// <returns><see langword="true"/> if the ancestor indents the position</returns>
    private static bool IsIndentingAncestor(SyntaxNode node, int spanStart)
    {
        return node switch
               {
                   BlockSyntax block => IsBetweenBraces(spanStart, block.OpenBraceToken, block.CloseBraceToken),
                   TypeDeclarationSyntax typeDeclaration => IsBetweenBraces(spanStart, typeDeclaration.OpenBraceToken, typeDeclaration.CloseBraceToken),
                   NamespaceDeclarationSyntax namespaceDeclaration => IsBetweenBraces(spanStart, namespaceDeclaration.OpenBraceToken, namespaceDeclaration.CloseBraceToken),
                   EnumDeclarationSyntax enumDeclaration => IsBetweenBraces(spanStart, enumDeclaration.OpenBraceToken, enumDeclaration.CloseBraceToken),
                   SwitchStatementSyntax switchStatement => IsBetweenBraces(spanStart, switchStatement.OpenBraceToken, switchStatement.CloseBraceToken),
                   AccessorListSyntax accessorList => IsBetweenBraces(spanStart, accessorList.OpenBraceToken, accessorList.CloseBraceToken),
                   InitializerExpressionSyntax initializer => IsBetweenBraces(spanStart, initializer.OpenBraceToken, initializer.CloseBraceToken),
                   AnonymousObjectCreationExpressionSyntax anonymousObjectCreation => IsBetweenBraces(spanStart, anonymousObjectCreation.OpenBraceToken, anonymousObjectCreation.CloseBraceToken),
                   _ => false,
               };
    }

    /// <summary>
    /// Determines whether a position lies between an opening and closing brace
    /// </summary>
    /// <param name="spanStart">Position to inspect</param>
    /// <param name="openBrace">Opening brace token</param>
    /// <param name="closeBrace">Closing brace token</param>
    /// <returns><see langword="true"/> if the position is between the braces</returns>
    private static bool IsBetweenBraces(int spanStart, SyntaxToken openBrace, SyntaxToken closeBrace)
    {
        if (openBrace.IsMissing || closeBrace.IsMissing)
        {
            return false;
        }

        return spanStart > openBrace.SpanStart && spanStart < closeBrace.SpanStart;
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH0389IndentationMustUseFourSpacesPerScopeLevelAnalyzer.DiagnosticId];

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    /// <inheritdoc/>
    public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        foreach (var diagnostic in context.Diagnostics)
        {
            context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH0389Title,
                                                      token => ApplyCodeFixAsync(context.Document, diagnostic, token),
                                                      nameof(RH0389IndentationMustUseFourSpacesPerScopeLevelCodeFixProvider)),
                                    diagnostic);
        }

        return Task.CompletedTask;
    }

    #endregion // CodeFixProvider
}