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
/// Code fix provider for <see cref="RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5204IndentationMustUseFourSpacesPerScopeLevelCodeFixProvider))]
public class RH5204IndentationMustUseFourSpacesPerScopeLevelCodeFixProvider : CodeFixProvider
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

        if (TryGetDirectiveTrivia(root, diagnostic, out _))
        {
            return await ApplyDirectiveIndentationFormattingAsync(document, root, diagnosticLine, cancellationToken).ConfigureAwait(false);
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
    /// Applies the analyzer-computed directive indentation to the reported line
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="root">Syntax root</param>
    /// <param name="diagnosticLine">Reported line number</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated document</returns>
    private static async Task<Document> ApplyDirectiveIndentationFormattingAsync(Document document, SyntaxNode root, int diagnosticLine, CancellationToken cancellationToken)
    {
        // The analyzer owns the single indentation policy. Reusing its expected-indentation map guarantees the fix
        // targets exactly the column the analyzer expects, so directives inside switch sections, switch expressions
        // and collection expressions converge instead of being re-indented to a diverging column
        var expectedIndentationByLine = RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer.BuildExpectedIndentationMap(root);

        if (expectedIndentationByLine.TryGetValue(diagnosticLine, out var expectation) == false)
        {
            return document;
        }

        var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
        var line = sourceText.Lines[diagnosticLine];
        var lineText = sourceText.ToString(line.Span);
        var firstNonWhitespaceIndex = 0;

        while (firstNonWhitespaceIndex < lineText.Length && char.IsWhiteSpace(lineText[firstNonWhitespaceIndex]))
        {
            firstNonWhitespaceIndex++;
        }

        var replacementSpan = TextSpan.FromBounds(line.Start, line.Start + firstNonWhitespaceIndex);

        return document.WithText(sourceText.Replace(replacementSpan, new string(' ', expectation.Indentation)));
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

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer.DiagnosticId];

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
            context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH5204Title,
                                                      token => ApplyCodeFixAsync(context.Document, diagnostic, token),
                                                      nameof(RH5204IndentationMustUseFourSpacesPerScopeLevelCodeFixProvider)),
                                    diagnostic);
        }

        return Task.CompletedTask;
    }

    #endregion // CodeFixProvider
}