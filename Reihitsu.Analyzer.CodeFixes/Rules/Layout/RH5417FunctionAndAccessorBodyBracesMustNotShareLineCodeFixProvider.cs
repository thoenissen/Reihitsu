using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Formatter;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Code fix provider for <see cref="RH5417FunctionAndAccessorBodyBracesMustNotShareLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5417FunctionAndAccessorBodyBracesMustNotShareLineCodeFixProvider))]
public class RH5417FunctionAndAccessorBodyBracesMustNotShareLineCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applies the code fix to a member declaration whose body can be formatted safely as a unit
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="member">Member declaration whose braces share a line</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, MemberDeclarationSyntax member, CancellationToken cancellationToken)
    {
        return await ReihitsuFormatter.FormatNodeInDocumentAsync(document, member, cancellationToken).ConfigureAwait(false);
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH5417FunctionAndAccessorBodyBracesMustNotShareLineAnalyzer.DiagnosticId];

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
            if (root.FindToken(diagnostic.Location.SourceSpan.Start).Parent?.FirstAncestorOrSelf<MemberDeclarationSyntax>() is not { } member)
            {
                continue;
            }

            context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH5417Title,
                                                      token => ApplyCodeFixAsync(context.Document, member, token),
                                                      nameof(RH5417FunctionAndAccessorBodyBracesMustNotShareLineCodeFixProvider)),
                                    diagnostic);
        }
    }

    #endregion // CodeFixProvider
}