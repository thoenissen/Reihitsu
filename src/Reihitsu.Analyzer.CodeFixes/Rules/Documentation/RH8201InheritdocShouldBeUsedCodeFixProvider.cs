using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Core;
using Reihitsu.Formatter.Utilities;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Documentation;

/// <summary>
/// Providing fixes for <see cref="RH8201InheritdocShouldBeUsedAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH8201InheritdocShouldBeUsedCodeFixProvider))]
public class RH8201InheritdocShouldBeUsedCodeFixProvider : ReihitsuCodeFixProviderBase
{
    #region Methods

    /// <summary>
    /// Determines whether the interfaces of the type may differ between build configurations
    /// </summary>
    /// <param name="typeSymbol">Type symbol</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> if a file declaring the type or one of its source-declared interfaces contains an <c>#if</c> directive</returns>
    /// <remarks>
    /// The check covers whole files rather than type headers, because a conditional region can enclose a complete
    /// declaration, supply alternative headers, or change the members and base lists of the interfaces themselves
    /// </remarks>
    private static bool HasConditionalInterfaces(INamedTypeSymbol typeSymbol, CancellationToken cancellationToken)
    {
        var syntaxTrees = typeSymbol.DeclaringSyntaxReferences
                                    .Concat(typeSymbol.AllInterfaces.SelectMany(interfaceType => interfaceType.DeclaringSyntaxReferences))
                                    .Select(reference => reference.SyntaxTree)
                                    .Distinct();

        foreach (var syntaxTree in syntaxTrees)
        {
            var root = syntaxTree.GetRoot(cancellationToken);

            if (root.ContainsDirectives
                && root.GetFirstDirective(directive => directive.IsKind(SyntaxKind.IfDirectiveTrivia)) != null)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether the fix is withheld for the member: it inherits its documentation only by implicitly
    /// implementing an interface member, and an <c>#if</c> directive in a file that declares its type or one of the
    /// type's interfaces may let another build configuration compile it as implementing nothing, where the replaced
    /// documentation would be lost (RH8201-003). An override and an explicit implementation implement their member in
    /// every configuration in which they compile
    /// </summary>
    /// <param name="declaration">Member declaration</param>
    /// <param name="semanticModel">Semantic model</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> if the fix is withheld</returns>
    private static bool IsImplicitImplementationWithConditionalInterfaces(MemberDeclarationSyntax declaration, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        if (declaration.Modifiers.Any(SyntaxKind.OverrideKeyword)
            || DocumentationAnalysisUtilities.IsExplicitInterfaceImplementation(declaration))
        {
            return false;
        }

        return declaration.Parent is not TypeDeclarationSyntax typeDeclaration
               || semanticModel.GetDeclaredSymbol(typeDeclaration, cancellationToken) is not INamedTypeSymbol typeSymbol
               || HasConditionalInterfaces(typeSymbol, cancellationToken);
    }

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
        // Only the flagged (first) documentation comment is replaced. Replacing every documentation comment would
        // emit multiple <inheritdoc/> lines when a member carries more than one documentation comment
        var commentIndex = FindFirstDocumentationComment(triviaList);

        if (commentIndex < 0)
        {
            return triviaList;
        }

        var result = new List<SyntaxTrivia>(triviaList.Count + 1);

        result.AddRange(triviaList.Take(commentIndex));
        result.Add(CreateInheritdocTrivia(endOfLine));

        var restIndex = commentIndex + 1;

        // A /** ... */ comment does not carry its own line break, while the synthesized <inheritdoc/> trivia
        // does. The line break that terminated the replaced comment is therefore dropped so the fix does not
        // introduce a blank line before the member
        if (triviaList[commentIndex].IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia))
        {
            var commentIndentation = commentIndex > 0 ? triviaList[commentIndex - 1] : default;

            restIndex = DropLineBreakBehindComment(triviaList, restIndex, commentIndentation, result);
        }

        result.AddRange(triviaList.Skip(restIndex));

        return result;
    }

    /// <summary>
    /// Finds the first <see cref="SyntaxKind.SingleLineDocumentationCommentTrivia"/> or
    /// <see cref="SyntaxKind.MultiLineDocumentationCommentTrivia"/> in a trivia list
    /// </summary>
    /// <param name="triviaList">List of trivia elements</param>
    /// <returns>The index of the documentation comment, or <c>-1</c> when the list holds none</returns>
    private static int FindFirstDocumentationComment(SyntaxTriviaList triviaList)
    {
        for (var index = 0; index < triviaList.Count; index++)
        {
            if (SyntaxTriviaUtilities.IsDocumentationCommentTrivia(triviaList[index]))
            {
                return index;
            }
        }

        return -1;
    }

    /// <summary>
    /// Drops the line break that ends the line of a replaced <c>/** ... */</c> comment, together with the whitespace in
    /// front of it. When an ordinary comment or the member's first token followed the <c>*/</c> on its line, there is no
    /// such line break: the <c>&lt;inheritdoc/&gt;</c> line ends with a line break, so what followed starts the next line,
    /// in the column the replaced comment started in
    /// </summary>
    /// <param name="triviaList">List of trivia elements</param>
    /// <param name="index">Index of the trivia behind the replaced comment</param>
    /// <param name="commentIndentation">The trivia in front of the replaced comment, its indentation when it is whitespace</param>
    /// <param name="result">Receives the indentation of the next line, when one is written</param>
    /// <returns>The index of the first trivia that is kept as written</returns>
    private static int DropLineBreakBehindComment(SyntaxTriviaList triviaList, int index, SyntaxTrivia commentIndentation, List<SyntaxTrivia> result)
    {
        while (index < triviaList.Count
               && triviaList[index].IsKind(SyntaxKind.WhitespaceTrivia))
        {
            index++;
        }

        if (index < triviaList.Count
            && triviaList[index].IsKind(SyntaxKind.EndOfLineTrivia))
        {
            return index + 1;
        }

        if (commentIndentation.IsKind(SyntaxKind.WhitespaceTrivia))
        {
            result.Add(commentIndentation);
        }

        return index;
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

    #endregion // CodeFixProvider

    #region ReihitsuCodeFixProviderBase

    /// <inheritdoc/>
    protected sealed override async Task RegisterCodeFixesCoreAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);

        if (root == null
            || semanticModel == null)
        {
            return;
        }

        foreach (var diagnostic in context.Diagnostics)
        {
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var declaration = root.FindToken(diagnosticSpan.Start).Parent?.AncestorsAndSelf().OfType<MemberDeclarationSyntax>().FirstOrDefault();

            if (declaration != null
                && IsImplicitImplementationWithConditionalInterfaces(declaration, semanticModel, context.CancellationToken) == false)
            {
                context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH8201Title,
                                                          cancellationToken => ApplyCodeFixAsync(context.Document, declaration, cancellationToken),
                                                          nameof(RH8201InheritdocShouldBeUsedCodeFixProvider)),
                                        diagnostic);
            }
        }
    }

    #endregion // ReihitsuCodeFixProviderBase
}