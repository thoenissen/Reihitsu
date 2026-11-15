using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.CodeFixes.Core;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Formatter;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Code fix provider for <see cref="RH5110GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5110GenericTypeConstraintsShouldBeOnTheirOwnLineCodeFixProvider))]
public class RH5110GenericTypeConstraintsShouldBeOnTheirOwnLineCodeFixProvider : ReihitsuCodeFixProviderBase
{
    #region Methods

    /// <summary>
    /// Finds the declaration the fix formats: the declaration that owns the reported constraint clause
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <param name="diagnosticSpan">The diagnostic's span</param>
    /// <returns>The declaration, or <see langword="null"/> when there is none</returns>
    private static SyntaxNode FindDeclaration(SyntaxNode root, TextSpan diagnosticSpan)
    {
        return root.FindToken(diagnosticSpan.Start).Parent?.FirstAncestorOrSelf<TypeParameterConstraintClauseSyntax>()?.Parent;
    }

    /// <summary>
    /// Applies the code fix. A member that shares its line with another member is first separated from it, so that both stand
    /// on lines of their own (SHARED-FIX-007); then the declaration is formatted through the formatter
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

        return FindDeclaration(root, diagnosticSpan) switch
               {
                   null => document,
                   MemberDeclarationSyntax member => await BlankLineCodeFixUtilities.FormatMemberOnLinesOfItsOwnAsync(document, member, cancellationToken).ConfigureAwait(false),
                   var declaration => (await ReihitsuFormatter.FormatNodeInDocumentAsync(document, declaration, cancellationToken).ConfigureAwait(false)).Output
               };
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH5110GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzer.DiagnosticId];

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider()
    {
        return DocumentOrderFixAllProvider.Create(new RH5110GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzer());
    }

    #endregion // CodeFixProvider

    #region ReihitsuCodeFixProviderBase

    /// <inheritdoc/>
    protected override bool FormatsThroughFormatter => true;

    /// <inheritdoc/>
    protected sealed override async Task RegisterCodeFixesCoreAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return;
        }

        foreach (var diagnostic in context.Diagnostics)
        {
            if (FindDeclaration(root, diagnostic.Location.SourceSpan) is { } declaration
                && ReihitsuFormatter.FindUnsupportedSyntax(declaration) != null)
            {
                continue;
            }

            context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH5110Title,
                                                      token => ApplyCodeFixAsync(context.Document, diagnostic.Location.SourceSpan, token),
                                                      nameof(RH5110GenericTypeConstraintsShouldBeOnTheirOwnLineCodeFixProvider)),
                                    diagnostic);
        }
    }

    #endregion // ReihitsuCodeFixProviderBase
}