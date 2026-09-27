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

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Code fix provider for <see cref="RH5114UsingDirectivesMustBePlacedOnSeparateLinesAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5114UsingDirectivesMustBePlacedOnSeparateLinesCodeFixProvider))]
public class RH5114UsingDirectivesMustBePlacedOnSeparateLinesCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applies the code fix. Only the gap between the two directives is rewritten: comments that trail the
    /// previous directive stay on its line, the whitespace behind them is replaced by a line break and the
    /// scope's indentation, and any whitespace that leads the moved directive is dropped. The directives
    /// are neither reordered nor separated by a blank line
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="previousDirective">Using directive whose line the moved directive shares</param>
    /// <param name="directive">Using directive to move</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, UsingDirectiveSyntax previousDirective, UsingDirectiveSyntax directive, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
        var previousTrailingTrivia = previousDirective.GetTrailingTrivia().Reverse().SkipWhile(IsWhitespace).Reverse();
        var directiveLeadingTrivia = directive.GetLeadingTrivia().SkipWhile(IsWhitespace);
        var indentation = UsingDirectiveOrderingUtilities.GetLineIndentation(UsingDirectiveOrderingUtilities.GetUsings(directive.Parent)[0]);
        var replacementText = $"{SyntaxFactory.TriviaList(previousTrailingTrivia).ToFullString()}{LineEndingUtilities.DetectEndOfLine(root)}{indentation}{SyntaxFactory.TriviaList(directiveLeadingTrivia).ToFullString()}";
        var replacementSpan = TextSpan.FromBounds(previousDirective.Span.End, directive.Span.Start);

        return document.WithText(sourceText.Replace(replacementSpan, replacementText));
    }

    /// <summary>
    /// Determines whether a trivia is whitespace
    /// </summary>
    /// <param name="trivia">Trivia</param>
    /// <returns><see langword="true"/> if the trivia is whitespace; otherwise, <see langword="false"/></returns>
    private static bool IsWhitespace(SyntaxTrivia trivia)
    {
        return trivia.IsKind(SyntaxKind.WhitespaceTrivia);
    }

    /// <summary>
    /// Gets the using directive that precedes the given directive in its list when the gap between them
    /// can be rewritten. The fix is withheld when the directives no longer share a line, when a
    /// documentation comment leads the moved directive — moving it to the start of a line would turn it
    /// into a misplaced documentation comment — or when a syntax error touches the gap
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <param name="directive">Reported using directive</param>
    /// <returns>The preceding using directive, or <see langword="null"/> if the gap cannot be rewritten</returns>
    private static UsingDirectiveSyntax GetEditablePreviousDirective(SyntaxNode root, UsingDirectiveSyntax directive)
    {
        var usingDirectives = UsingDirectiveOrderingUtilities.GetUsings(directive.Parent);
        var directiveIndex = usingDirectives.IndexOf(directive);

        if (directiveIndex < 1)
        {
            return null;
        }

        var previousDirective = usingDirectives[directiveIndex - 1];

        if (RH5114UsingDirectivesMustBePlacedOnSeparateLinesAnalyzer.StartsOnLineOfPreviousDirective(previousDirective, directive) == false)
        {
            return null;
        }

        if (directive.GetLeadingTrivia().Any(static trivia => trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia)
                                                              || trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)))
        {
            return null;
        }

        var gap = TextSpan.FromBounds(previousDirective.Span.End, directive.Span.Start);

        return root.SyntaxTree.GetDiagnostics().Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error
                                                                  && diagnostic.Location.SourceSpan.IntersectsWith(gap))
                   ? null
                   : previousDirective;
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH5114UsingDirectivesMustBePlacedOnSeparateLinesAnalyzer.DiagnosticId];

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
            var directive = root.FindNode(diagnostic.Location.SourceSpan).FirstAncestorOrSelf<UsingDirectiveSyntax>();

            if (directive == null)
            {
                continue;
            }

            var previousDirective = GetEditablePreviousDirective(root, directive);

            if (previousDirective == null)
            {
                continue;
            }

            context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH5114Title,
                                                      token => ApplyCodeFixAsync(context.Document, previousDirective, directive, token),
                                                      nameof(RH5114UsingDirectivesMustBePlacedOnSeparateLinesCodeFixProvider)),
                                    diagnostic);
        }
    }

    #endregion // CodeFixProvider
}