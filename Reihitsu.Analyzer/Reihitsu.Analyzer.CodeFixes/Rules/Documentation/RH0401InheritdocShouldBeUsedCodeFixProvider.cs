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

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// Providing fixes for <see cref="RH0401InheritdocShouldBeUsedAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0401InheritdocShouldBeUsedCodeFixProvider))]
public class RH0401InheritdocShouldBeUsedCodeFixProvider : CodeFixProvider
{
    #region Fields

    /// <summary>
    /// &lt;inheritdoc/&gt; trivia
    /// </summary>
    private static readonly SyntaxTrivia _inheritdocTrivia;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    static RH0401InheritdocShouldBeUsedCodeFixProvider()
    {
        _inheritdocTrivia = Trivia(DocumentationCommentTrivia(SyntaxKind.SingleLineDocumentationCommentTrivia,
                                                              List(new XmlNodeSyntax[]
                                                                   {
                                                                       XmlText()
                                                                           .WithTextTokens(TokenList(XmlTextLiteral(TriviaList(DocumentationCommentExterior("///")), " ", " ", TriviaList()))),
                                                                       XmlNullKeywordElement()
                                                                           .WithName(XmlName(Identifier("inheritdoc")))
                                                                           .WithAttributes(List<XmlAttributeSyntax>()),
                                                                       XmlText()
                                                                           .WithTextTokens(TokenList(XmlTextNewLine(TriviaList(), Environment.NewLine, Environment.NewLine, TriviaList())))
                                                                   })));
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Replacing the <see cref="SyntaxKind.SingleLineDocumentationCommentTrivia"/> with a &amp;lt;inheritdoc/&amp;gt; trivia
    /// </summary>
    /// <param name="triviaList">List of trivia elements</param>
    /// <returns>List with replaced element</returns>
    private IEnumerable<SyntaxTrivia> ReplaceDocumentation(SyntaxTriviaList triviaList)
    {
        return triviaList.Select(trivia => trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)
                                               ? _inheritdocTrivia
                                               : trivia);
    }

    /// <summary>
    /// Applies the code fix by replacing <see cref="SyntaxKind.SingleLineDocumentationCommentTrivia"/>
    /// with a &lt;inheritdoc/&gt; trivia in the leading trivia of the specified <see cref="MemberDeclarationSyntax"/>.
    /// </summary>
    /// <param name="document">The <see cref="Document"/> to apply the fix to.</param>
    /// <param name="memberDeclaration">The <see cref="MemberDeclarationSyntax"/> to fix.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The updated <see cref="Document"/> with the code fix applied.</returns>
    private async Task<Document> ApplyCodeFixAsync(Document document, MemberDeclarationSyntax memberDeclaration, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken);
        if (root != null)
        {
            var updatedMemberDeclaration = memberDeclaration.WithLeadingTrivia(TriviaList(ReplaceDocumentation(memberDeclaration.GetLeadingTrivia())));

            var newRoot = root.ReplaceNode(memberDeclaration, updatedMemberDeclaration);

            document = document.WithSyntaxRoot(newRoot);
        }

        return document;
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(RH0401InheritdocShouldBeUsedAnalyzer.DiagnosticId);

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root != null)
        {
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var declaration = root.FindToken(diagnosticSpan.Start).Parent?.AncestorsAndSelf().OfType<MemberDeclarationSyntax>().FirstOrDefault();

            if (declaration != null)
            {
                context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH0401Title,
                                                          c => ApplyCodeFixAsync(context.Document, declaration, c),
                                                          nameof(CodeFixResources.RH0401Title)),
                                        diagnostic);
            }
        }
    }

    #endregion // CodeFixProvider
}