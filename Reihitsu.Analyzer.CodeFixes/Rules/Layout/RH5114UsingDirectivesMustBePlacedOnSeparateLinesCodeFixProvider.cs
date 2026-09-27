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
    /// scope's indentation, and a comment that leads the moved directive moves along with it. The
    /// directives are neither reordered nor separated by a blank line
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
        var previousTrailingText = previousDirective.GetTrailingTrivia().ToFullString().TrimEnd(' ', '\t');
        var directiveLeadingText = directive.GetLeadingTrivia().ToFullString().TrimStart(' ', '\t');
        var replacementText = $"{previousTrailingText}{LineEndingUtilities.DetectEndOfLine(root)}{GetScopeIndentation(sourceText, directive)}{directiveLeadingText}";
        var replacementSpan = TextSpan.FromBounds(previousDirective.Span.End, directive.Span.Start);

        return document.WithText(sourceText.Replace(replacementSpan, replacementText));
    }

    /// <summary>
    /// Gets the indentation of the scope's using directives: the leading whitespace of the line of the
    /// scope's first directive when that directive is the first token on its line, and no indentation
    /// otherwise
    /// </summary>
    /// <param name="sourceText">Source text</param>
    /// <param name="directive">Using directive to move</param>
    /// <returns>The indentation to place before the moved directive</returns>
    private static string GetScopeIndentation(SourceText sourceText, UsingDirectiveSyntax directive)
    {
        var firstDirectiveStart = GetUsingDirectives(directive)[0].SpanStart;
        var firstDirectiveLine = sourceText.Lines.GetLineFromPosition(firstDirectiveStart);
        var indentation = FormattingTextAnalysisUtilities.GetLeadingWhitespace(FormattingTextAnalysisUtilities.GetLineText(sourceText, firstDirectiveLine));

        return firstDirectiveLine.Start + indentation.Length == firstDirectiveStart
                   ? indentation
                   : string.Empty;
    }

    /// <summary>
    /// Gets the using directive list that contains the given directive
    /// </summary>
    /// <param name="directive">Using directive</param>
    /// <returns>The containing using directive list</returns>
    private static SyntaxList<UsingDirectiveSyntax> GetUsingDirectives(UsingDirectiveSyntax directive)
    {
        return directive.Parent switch
               {
                   CompilationUnitSyntax compilationUnit => compilationUnit.Usings,
                   BaseNamespaceDeclarationSyntax namespaceDeclaration => namespaceDeclaration.Usings,
                   _ => default
               };
    }

    /// <summary>
    /// Gets the using directive that precedes the given directive in its list when both still share a line
    /// and only trivia without syntax errors separates them
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <param name="directive">Reported using directive</param>
    /// <returns>The preceding using directive, or <see langword="null"/> if the gap cannot be rewritten</returns>
    private static UsingDirectiveSyntax GetEditablePreviousDirective(SyntaxNode root, UsingDirectiveSyntax directive)
    {
        var usingDirectives = GetUsingDirectives(directive);
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

        var gap = TextSpan.FromBounds(previousDirective.Span.End, directive.Span.Start);

        return root.SyntaxTree.GetDiagnostics().Any(diagnostic => diagnostic.Location.SourceSpan.IntersectsWith(gap))
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