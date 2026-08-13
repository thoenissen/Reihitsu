using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Formatter.Utilities;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Code fix provider for <see cref="RH5604CodeMustNotContainMixedLineEndingsAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5604CodeMustNotContainMixedLineEndingsCodeFixProvider))]
public class RH5604CodeMustNotContainMixedLineEndingsCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applies the code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        var endOfLine = ReihitsuFormatterHelpers.DetectEndOfLine(root);
        var documentationEndOfLinesToReplace = root.DescendantTokens(descendIntoTrivia: true)
                                                   .Where(token => token.IsKind(SyntaxKind.XmlTextLiteralNewLineToken) && token.Text != endOfLine)
                                                   .ToArray();
        var normalizedRoot = documentationEndOfLinesToReplace.Length == 0
                                 ? root
                                 : root.ReplaceTokens(documentationEndOfLinesToReplace,
                                                      (original, _) => original.CopyAnnotationsTo(SyntaxFactory.XmlTextNewLine(original.LeadingTrivia,
                                                                                                                               endOfLine,
                                                                                                                               original.ValueText,
                                                                                                                               original.TrailingTrivia)));
        var endOfLinesToReplace = normalizedRoot.DescendantTrivia(descendIntoTrivia: true)
                                                .Where(trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia) && trivia.ToString() != endOfLine)
                                                .ToArray();

        if (documentationEndOfLinesToReplace.Length == 0 && endOfLinesToReplace.Length == 0)
        {
            return document;
        }

        if (endOfLinesToReplace.Length == 0)
        {
            return document.WithSyntaxRoot(normalizedRoot);
        }

        return document.WithSyntaxRoot(normalizedRoot.ReplaceTrivia(endOfLinesToReplace, (_, _) => SyntaxFactory.EndOfLine(endOfLine)));
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH5604CodeMustNotContainMixedLineEndingsAnalyzer.DiagnosticId];

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
            context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH5604Title,
                                                      token => ApplyCodeFixAsync(context.Document, token),
                                                      nameof(RH5604CodeMustNotContainMixedLineEndingsCodeFixProvider)),
                                    diagnostic);
        }

        return Task.CompletedTask;
    }

    #endregion // CodeFixProvider
}