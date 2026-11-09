using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Core;
using Reihitsu.Formatter;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Organization;

/// <summary>
/// Code fix provider for <see cref="RH7004UsingDeclarationsShouldNotBeUsedAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH7004UsingDeclarationsShouldNotBeUsedCodeFixProvider))]
public class RH7004UsingDeclarationsShouldNotBeUsedCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applies the code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="usingDeclaration">Using declaration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, LocalDeclarationStatementSyntax usingDeclaration, CancellationToken cancellationToken)
    {
        if (usingDeclaration.Parent is not BlockSyntax parentBlock)
        {
            return document;
        }

        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        var statementIndex = parentBlock.Statements.IndexOf(usingDeclaration);

        if (statementIndex < 0)
        {
            return document;
        }

        var formattingAnnotation = new SyntaxAnnotation();
        var newUsingStatement = CreateUsingStatement(parentBlock, usingDeclaration, statementIndex).WithAdditionalAnnotations(formattingAnnotation);
        var updatedStatements = SyntaxFactory.List(parentBlock.Statements.Take(statementIndex).Concat([newUsingStatement]));
        var updatedBlock = parentBlock.WithStatements(updatedStatements);
        var updatedRoot = root.ReplaceNode(parentBlock, updatedBlock);
        var updatedDocument = document.WithSyntaxRoot(updatedRoot);
        var formattedRoot = await updatedDocument.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var formattedUsingStatement = formattedRoot?.GetAnnotatedNodes(formattingAnnotation).OfType<UsingStatementSyntax>().FirstOrDefault();

        return formattedUsingStatement == null
                   ? updatedDocument
                   : (await ReihitsuFormatter.FormatNodeInDocumentAsync(updatedDocument, formattedUsingStatement, cancellationToken).ConfigureAwait(false)).Output;
    }

    /// <summary>
    /// Creates the using statement the fix replaces the using declaration with, before the formatter lays it out: the
    /// declaration's resource, and a block that holds every statement behind it in its block
    /// </summary>
    /// <param name="parentBlock">The block that holds the using declaration</param>
    /// <param name="usingDeclaration">The using declaration</param>
    /// <param name="statementIndex">The index of the using declaration in the block</param>
    /// <returns>The using statement</returns>
    private static UsingStatementSyntax CreateUsingStatement(BlockSyntax parentBlock, LocalDeclarationStatementSyntax usingDeclaration, int statementIndex)
    {
        var followingStatements = parentBlock.Statements.Skip(statementIndex + 1);
        var semicolonToken = usingDeclaration.SemicolonToken;
        var closeParenToken = SyntaxFactory.Token(SyntaxKind.CloseParenToken)
                                           .WithLeadingTrivia(semicolonToken.LeadingTrivia);
        var statementTrailingTrivia = usingDeclaration.GetTrailingTrivia();

        if (semicolonToken.TrailingTrivia.Any(SyntaxTriviaUtilities.IsCommentTrivia)
            && semicolonToken.TrailingTrivia.Last().IsKind(SyntaxKind.EndOfLineTrivia))
        {
            // A comment behind the declaration at the end of its line describes the resource, so it stays behind the
            // header of the new statement, on the header's line, together with the line break that ends that line. A
            // comment that code follows on its line stays behind the new block, with the code it stands in front of
            closeParenToken = closeParenToken.WithTrailingTrivia(semicolonToken.TrailingTrivia);
            statementTrailingTrivia = SyntaxFactory.TriviaList(semicolonToken.TrailingTrivia.Where(trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia)).Take(1));
        }

        return SyntaxFactory.UsingStatement(usingDeclaration.Declaration, null, SyntaxFactory.Block(followingStatements))
                            .WithAwaitKeyword(usingDeclaration.AwaitKeyword)
                            .WithUsingKeyword(usingDeclaration.UsingKeyword)
                            .WithCloseParenToken(closeParenToken)
                            .WithLeadingTrivia(usingDeclaration.GetLeadingTrivia())
                            .WithTrailingTrivia(statementTrailingTrivia);
    }

    /// <summary>
    /// Determines whether the statements following the using declaration prevent a safe conversion
    /// </summary>
    /// <param name="parentBlock">Block that contains the using declaration</param>
    /// <param name="usingDeclaration">Using declaration</param>
    /// <returns><see langword="true"/> when the following statements prevent a safe conversion; otherwise <see langword="false"/></returns>
    /// <remarks>
    /// Wrapping the following statements into the new using body introduces a nested scope. Local functions and labels
    /// declared after the using declaration would no longer be visible to earlier statements, producing non-compiling
    /// code (CS0103 for local-function calls, CS0159 for <c>goto</c> targets), so the conversion is not offered when any
    /// such statement follows the using declaration
    /// </remarks>
    private static bool HasFollowingStatementsThatPreventConversion(BlockSyntax parentBlock, LocalDeclarationStatementSyntax usingDeclaration)
    {
        var statementIndex = parentBlock.Statements.IndexOf(usingDeclaration);
        var followingStatements = parentBlock.Statements.Skip(statementIndex + 1);

        return followingStatements.Any(static statement => statement is LocalFunctionStatementSyntax or LabeledStatementSyntax);
    }

    /// <summary>
    /// Determines whether the new block would separate a directive from the code it governs. The block opens behind
    /// the using declaration and closes in front of the trivia of the enclosing block's closing brace, so it must not
    /// put one brace into the code a directive pair governs and the other outside it (SHARED-FIX-003). A conditional
    /// directive in front of the enclosing block's closing brace withholds the fix as well when its <c>#if</c> lies
    /// behind the declaration: in another build configuration, the code of that group follows the declaration and
    /// would end up behind the new block. A group whose <c>#if</c> lies in front of the declaration holds the
    /// declaration in one of its branches, so no configuration compiles another of its branches together with it
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <param name="parentBlock">Block that contains the using declaration</param>
    /// <param name="usingDeclaration">Using declaration</param>
    /// <returns><see langword="true"/> when the new block would separate a directive from its code; otherwise <see langword="false"/></returns>
    private static bool WouldSeparateDirective(SyntaxNode root, BlockSyntax parentBlock, LocalDeclarationStatementSyntax usingDeclaration)
    {
        var closeBraceToken = parentBlock.CloseBraceToken;
        var declarationEnd = usingDeclaration.Span.End;

        return SyntaxTriviaUtilities.SplitsDirectivePair(root, usingDeclaration.FullSpan.End, closeBraceToken.FullSpan.Start)
               || closeBraceToken.LeadingTrivia.Any(trivia => trivia.GetStructure() is ConditionalDirectiveTriviaSyntax or ElseDirectiveTriviaSyntax or EndIfDirectiveTriviaSyntax
                                                              && trivia.GetStructure() is DirectiveTriviaSyntax directive
                                                              && directive.GetRelatedDirectives().Any(related => related.IsKind(SyntaxKind.IfDirectiveTrivia) && related.SpanStart > declarationEnd));
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH7004UsingDeclarationsShouldNotBeUsedAnalyzer.DiagnosticId];

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
            var usingDeclaration = root.FindToken(diagnostic.Location.SourceSpan.Start)
                                       .Parent
                                       ?.AncestorsAndSelf()
                                       .OfType<LocalDeclarationStatementSyntax>()
                                       .FirstOrDefault();

            if (usingDeclaration?.Parent is BlockSyntax parentBlock
                && HasFollowingStatementsThatPreventConversion(parentBlock, usingDeclaration) == false
                && WouldSeparateDirective(root, parentBlock, usingDeclaration) == false
                && ReihitsuFormatter.FindUnsupportedSyntax(CreateUsingStatement(parentBlock, usingDeclaration, parentBlock.Statements.IndexOf(usingDeclaration))) == null)
            {
                context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH7004Title,
                                                          token => ApplyCodeFixAsync(context.Document, usingDeclaration, token),
                                                          nameof(RH7004UsingDeclarationsShouldNotBeUsedCodeFixProvider)),
                                        diagnostic);
            }
        }
    }

    #endregion // CodeFixProvider
}