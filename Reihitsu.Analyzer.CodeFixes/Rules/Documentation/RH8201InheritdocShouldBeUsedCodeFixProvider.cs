using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.Rules.Documentation;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Documentation;

/// <summary>
/// Providing fixes for <see cref="RH8201InheritdocShouldBeUsedAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH8201InheritdocShouldBeUsedCodeFixProvider))]
public class RH8201InheritdocShouldBeUsedCodeFixProvider : CodeFixProvider
{
    #region Fields

    /// <summary>
    /// &lt;inheritdoc/&gt; trivia
    /// </summary>
    private static readonly SyntaxTrivia _inheritdocTrivia = SyntaxFactory.Trivia(SyntaxFactory.DocumentationCommentTrivia(SyntaxKind.SingleLineDocumentationCommentTrivia,
                                                                                                                           SyntaxFactory.List(new XmlNodeSyntax[]
                                                                                                                                              {
                                                                                                                                                  SyntaxFactory.XmlText().WithTextTokens(SyntaxFactory.TokenList(SyntaxFactory.XmlTextLiteral(SyntaxFactory.TriviaList(SyntaxFactory.DocumentationCommentExterior("///")), " ", " ", SyntaxFactory.TriviaList()))),
                                                                                                                                                  SyntaxFactory.XmlNullKeywordElement()
                                                                                                                                                               .WithName(SyntaxFactory.XmlName(SyntaxFactory.Identifier("inheritdoc")))
                                                                                                                                                               .WithAttributes(SyntaxFactory.List<XmlAttributeSyntax>()),
                                                                                                                                                  SyntaxFactory.XmlText().WithTextTokens(SyntaxFactory.TokenList(SyntaxFactory.XmlTextNewLine(SyntaxFactory.TriviaList(), Environment.NewLine, Environment.NewLine, SyntaxFactory.TriviaList())))
                                                                                                                                              })));

    #endregion // Fields

    #region Methods

    /// <summary>
    /// Replacing the first <see cref="SyntaxKind.SingleLineDocumentationCommentTrivia"/> with a &amp;lt;inheritdoc/&amp;gt; trivia
    /// </summary>
    /// <param name="triviaList">List of trivia elements</param>
    /// <returns>List with replaced element</returns>
    private IEnumerable<SyntaxTrivia> ReplaceDocumentation(SyntaxTriviaList triviaList)
    {
        var replaced = false;

        foreach (var trivia in triviaList)
        {
            // Only the flagged (first) documentation comment is replaced. Replacing every documentation comment would
            // emit multiple <inheritdoc/> lines when a member carries more than one documentation comment
            if (replaced == false
                && trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia))
            {
                replaced = true;

                yield return _inheritdocTrivia;

                continue;
            }

            yield return trivia;
        }
    }

    /// <summary>
    /// Applies the code fix by replacing <see cref="SyntaxKind.SingleLineDocumentationCommentTrivia"/>
    /// with a &lt;inheritdoc/&gt; trivia in the leading trivia of the specified <see cref="MemberDeclarationSyntax"/>
    /// </summary>
    /// <param name="document">The <see cref="Document"/> to apply the fix to</param>
    /// <param name="memberDeclaration">The <see cref="MemberDeclarationSyntax"/> to fix</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete</param>
    /// <returns>The updated <see cref="Document"/> with the code fix applied</returns>
    private async Task<Document> ApplyCodeFixAsync(Document document, MemberDeclarationSyntax memberDeclaration, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken);

        if (root != null)
        {
            var updatedMemberDeclaration = memberDeclaration.WithLeadingTrivia(SyntaxFactory.TriviaList(ReplaceDocumentation(memberDeclaration.GetLeadingTrivia())));

            var newRoot = root.ReplaceNode(memberDeclaration, updatedMemberDeclaration);

            document = document.WithSyntaxRoot(newRoot);
        }

        return document;
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId];

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
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var declaration = root.FindToken(diagnosticSpan.Start).Parent?.AncestorsAndSelf().OfType<MemberDeclarationSyntax>().FirstOrDefault();

            if (declaration != null)
            {
                context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH8201Title,
                                                          c => ApplyCodeFixAsync(context.Document, declaration, c),
                                                          nameof(RH8201InheritdocShouldBeUsedCodeFixProvider)),
                                        diagnostic);
            }
        }
    }

    #endregion // CodeFixProvider
}