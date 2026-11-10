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
using Reihitsu.Core;
using Reihitsu.Formatter.Utilities;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Documentation;

/// <summary>
/// Providing fixes for <see cref="RH8201InheritdocShouldBeUsedAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH8201InheritdocShouldBeUsedCodeFixProvider))]
public class RH8201InheritdocShouldBeUsedCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Creates the &lt;inheritdoc/&gt; trivia using the document's detected end-of-line sequence
    /// </summary>
    /// <param name="endOfLine">End-of-line sequence to use for the trailing line break</param>
    /// <returns>The &lt;inheritdoc/&gt; trivia</returns>
    /// <remarks>
    /// The trivia is parsed from text rather than built from syntax factory tokens, because those tokens carry elastic
    /// trivia, and the code action's post-processing re-formats the lines around elastic trivia: it re-indents the
    /// comments, directives and the member around the replaced comment and rewrites their line endings
    /// </remarks>
    private static SyntaxTrivia CreateInheritdocTrivia(string endOfLine)
    {
        return SyntaxFactory.ParseLeadingTrivia("/// <inheritdoc/>" + endOfLine)[0];
    }

    /// <summary>
    /// Replacing the first <see cref="SyntaxKind.SingleLineDocumentationCommentTrivia"/> or
    /// <see cref="SyntaxKind.MultiLineDocumentationCommentTrivia"/> with a &amp;lt;inheritdoc/&amp;gt; trivia
    /// </summary>
    /// <param name="triviaList">List of trivia elements</param>
    /// <param name="endOfLine">End-of-line sequence to use for the trailing line break</param>
    /// <returns>List with replaced element</returns>
    /// <remarks>
    /// Every other trivia is kept as written, so the comments, directives, blank lines and the member keep their lines,
    /// columns and line endings. The &lt;inheritdoc/&gt; line starts where the replaced comment started, because the
    /// indentation in front of the comment is kept
    /// </remarks>
    private static IEnumerable<SyntaxTrivia> ReplaceDocumentation(SyntaxTriviaList triviaList, string endOfLine)
    {
        var replaced = false;
        var isLineBreakPending = false;
        var commentIndentation = default(SyntaxTrivia);
        var previousTrivia = default(SyntaxTrivia);

        foreach (var trivia in triviaList)
        {
            if (isLineBreakPending)
            {
                // Whitespace between the replaced comment and its line break is dropped along with the line break
                if (trivia.IsKind(SyntaxKind.WhitespaceTrivia))
                {
                    continue;
                }

                isLineBreakPending = false;

                if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
                {
                    continue;
                }

                // An ordinary comment followed the */ on its line. The <inheritdoc/> line ends with a line break, so the
                // comment starts the next line in the column the replaced comment started in
                if (commentIndentation.IsKind(SyntaxKind.WhitespaceTrivia))
                {
                    yield return commentIndentation;
                }
            }

            // Only the flagged (first) documentation comment is replaced. Replacing every documentation comment would
            // emit multiple <inheritdoc/> lines when a member carries more than one documentation comment
            if (replaced == false
                && SyntaxTriviaUtilities.IsDocumentationCommentTrivia(trivia))
            {
                replaced = true;

                // A /** ... */ comment does not carry its own line break, while the synthesized <inheritdoc/> trivia
                // does. The line break that terminated the replaced comment is therefore dropped so the fix does not
                // introduce a blank line before the member
                isLineBreakPending = trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia);
                commentIndentation = previousTrivia;

                yield return CreateInheritdocTrivia(endOfLine);

                previousTrivia = trivia;

                continue;
            }

            previousTrivia = trivia;

            yield return trivia;
        }

        // The member's first token followed the */ on its line, so it starts the line after <inheritdoc/>, in the column
        // the replaced comment started in
        if (isLineBreakPending
            && commentIndentation.IsKind(SyntaxKind.WhitespaceTrivia))
        {
            yield return commentIndentation;
        }
    }

    /// <summary>
    /// Applies the code fix by replacing <see cref="SyntaxKind.SingleLineDocumentationCommentTrivia"/> or
    /// <see cref="SyntaxKind.MultiLineDocumentationCommentTrivia"/> with a &lt;inheritdoc/&gt; trivia in the
    /// leading trivia of the specified <see cref="MemberDeclarationSyntax"/>
    /// </summary>
    /// <param name="document">The <see cref="Document"/> to apply the fix to</param>
    /// <param name="memberDeclaration">The <see cref="MemberDeclarationSyntax"/> to fix</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete</param>
    /// <returns>The updated <see cref="Document"/> with the code fix applied</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, MemberDeclarationSyntax memberDeclaration, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root != null)
        {
            var endOfLine = ReihitsuFormatterHelpers.DetectEndOfLine(root);
            var updatedMemberDeclaration = memberDeclaration.WithLeadingTrivia(SyntaxFactory.TriviaList(ReplaceDocumentation(memberDeclaration.GetLeadingTrivia(), endOfLine)));

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
                                                          cancellationToken => ApplyCodeFixAsync(context.Document, declaration, cancellationToken),
                                                          nameof(RH8201InheritdocShouldBeUsedCodeFixProvider)),
                                        diagnostic);
            }
        }
    }

    #endregion // CodeFixProvider
}