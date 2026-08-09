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
using Reihitsu.Formatter;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Code fix provider for <see cref="RH5417FunctionAndAccessorBodyBracesMustNotShareLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5417FunctionAndAccessorBodyBracesMustNotShareLineCodeFixProvider))]
public class RH5417FunctionAndAccessorBodyBracesMustNotShareLineCodeFixProvider : CodeFixProvider
{
    #region Constants

    /// <summary>
    /// Stable equivalence key retained from the original public provider name
    /// </summary>
    private const string CodeActionEquivalenceKey = "RH5417MemberDeclarationBracesMustNotShareLineCodeFixProvider";

    #endregion // Constants

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

    /// <summary>
    /// Resolves the directly covered member or accessor owner for a reported body brace
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <param name="diagnosticSpan">Diagnostic span</param>
    /// <returns>The member that can be formatted safely, or <see langword="null"/> for local functions, lambdas, and anonymous methods</returns>
    private static MemberDeclarationSyntax GetFormattingMember(SyntaxNode root, TextSpan diagnosticSpan)
    {
        var block = root.FindToken(diagnosticSpan.Start).Parent?.FirstAncestorOrSelf<BlockSyntax>();

        return block?.Parent switch
               {
                   MemberDeclarationSyntax member => member,
                   AccessorDeclarationSyntax accessor => accessor.FirstAncestorOrSelf<MemberDeclarationSyntax>(),
                   _ => null
               };
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
            if (GetFormattingMember(root, diagnostic.Location.SourceSpan) is not { } member)
            {
                continue;
            }

            context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH5417Title,
                                                      token => ApplyCodeFixAsync(context.Document, member, token),
                                                      CodeActionEquivalenceKey),
                                    diagnostic);
        }
    }

    #endregion // CodeFixProvider
}