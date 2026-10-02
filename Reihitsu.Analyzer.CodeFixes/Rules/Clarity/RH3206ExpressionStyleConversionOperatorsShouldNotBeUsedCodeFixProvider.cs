using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Formatter;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Clarity;

/// <summary>
/// Code fix provider for <see cref="RH3206ExpressionStyleConversionOperatorsShouldNotBeUsedAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH3206ExpressionStyleConversionOperatorsShouldNotBeUsedCodeFixProvider))]
public class RH3206ExpressionStyleConversionOperatorsShouldNotBeUsedCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applying code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="conversionOperatorDeclaration">Conversion operator declaration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, ConversionOperatorDeclarationSyntax conversionOperatorDeclaration, CancellationToken cancellationToken)
    {
        return await ReihitsuFormatter.FormatNodeInDocumentAsync(document, conversionOperatorDeclaration, cancellationToken).ConfigureAwait(false);
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH3206ExpressionStyleConversionOperatorsShouldNotBeUsedAnalyzer.DiagnosticId];

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    /// <inheritdoc/>
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        if (root != null)
        {
            foreach (var diagnostic in context.Diagnostics)
            {
                var conversionOperatorDeclaration = root.FindNode(diagnostic.Location.SourceSpan)
                                                        .FirstAncestorOrSelf<ConversionOperatorDeclarationSyntax>();

                if (conversionOperatorDeclaration != null)
                {
                    context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH3206Title,
                                                              cancellationToken => ApplyCodeFixAsync(context.Document, conversionOperatorDeclaration, cancellationToken),
                                                              nameof(RH3206ExpressionStyleConversionOperatorsShouldNotBeUsedCodeFixProvider)),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // CodeFixProvider
}