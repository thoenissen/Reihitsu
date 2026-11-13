using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Organization;

/// <summary>
/// Code fix provider for <see cref="RH7004UsingDeclarationsShouldNotBeUsedAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH7004UsingDeclarationsShouldNotBeUsedCodeFixProvider))]
public class RH7004UsingDeclarationsShouldNotBeUsedCodeFixProvider : CodeFixProvider
{
    #region Constants

    /// <summary>
    /// Number of columns the statements move into the new block: one indentation level
    /// </summary>
    private const int IndentationSize = 4;

    #endregion // Constants

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
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null
            || TryCreateChanges(root, usingDeclaration, out var changes) == false)
        {
            return document;
        }

        var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);

        return document.WithText(text.WithChanges(changes));
    }

    /// <summary>
    /// Creates the text changes that turn the using declaration into a using statement whose block holds every statement
    /// behind it in its block. The declaration becomes the header, <c>using (</c> … <c>)</c>, with the rest of its line as
    /// written; the braces of the new block stand on lines of their own in the column of the declaration, and the
    /// statements between them are moved code: every line of them moves one indentation level deeper and otherwise stays
    /// as written (SHARED-FIX-007), so no blank line is inserted or removed (SHARED-FIX-006). A comment behind the
    /// declaration at the end of its line stays behind the header (RH7004-004). When no statement follows the declaration
    /// and code follows it on its line, the block is empty and the text behind the semicolon stays behind the block. Every
    /// line break the fix writes is the document's line ending (SHARED-FIX-008). The changes are refused when a line of the
    /// statements cannot move, a tab stands in front of the declaration or of a moving line, or the first statement starts
    /// on the line of the declaration (RH7004-007), when the code the fix rewrites, from the declaration through the end of
    /// the statements and the declaration's trailing line break included, holds a line break other than the document's
    /// line ending (SHARED-FIX-008), when a comment or directive stands in front of the semicolon, which would end up on
    /// a line of its own in front of the closing parenthesis, and when code stands in front of the declaration on its
    /// line or the last statement shares its line with the closing brace of the block, where the new braces would need a
    /// line break the author did not write
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <param name="usingDeclaration">Using declaration</param>
    /// <param name="changes">The text changes</param>
    /// <returns><see langword="true"/> if the changes can be created</returns>
    private static bool TryCreateChanges(SyntaxNode root, LocalDeclarationStatementSyntax usingDeclaration, out TextChange[] changes)
    {
        changes = null;

        if (usingDeclaration.Parent is not BlockSyntax parentBlock)
        {
            return false;
        }

        var text = root.SyntaxTree.GetText();
        var lines = text.Lines;
        var statements = parentBlock.Statements;
        var statementIndex = statements.IndexOf(usingDeclaration);
        var declarationLine = lines.GetLineFromPosition(usingDeclaration.SpanStart);
        var previousToken = usingDeclaration.GetFirstToken().GetPreviousToken();
        var movedSpan = statementIndex < statements.Count - 1
                            ? TextSpan.FromBounds(usingDeclaration.FullSpan.End, statements.Last().FullSpan.End)
                            : new TextSpan(usingDeclaration.FullSpan.End, 0);

        if (statementIndex < 0
            || lines.GetLineFromPosition(previousToken.Span.End).LineNumber == declarationLine.LineNumber
            || usingDeclaration.SemicolonToken.LeadingTrivia.Any(SyntaxNodeUtilities.IsCommentOrDirective))
        {
            return false;
        }

        var indentation = text.ToString(TextSpan.FromBounds(declarationLine.Start, declarationLine.Start + text.ToString(declarationLine.Span).TakeWhile(character => character is ' ' or '\t').Count()));

        if (indentation.IndexOf('\t') >= 0)
        {
            return false;
        }

        var endOfLine = LineEndingUtilities.DetectEndOfLine(root);

        // The fix rewrites the declaration and, on either path, everything from it through the end of the moved code,
        // the declaration's trailing line break included
        if (LineEndingUtilities.HoldsOtherLineBreak(text, TextSpan.FromBounds(usingDeclaration.SpanStart, movedSpan.End), endOfLine))
        {
            return false;
        }

        var openParenChange = new TextChange(new TextSpan(usingDeclaration.Declaration.SpanStart, 0), "(");

        if (lines.GetLineFromPosition(movedSpan.Start).Start != movedSpan.Start)
        {
            if (movedSpan.IsEmpty == false)
            {
                return false;
            }

            // No statement follows the declaration, and code follows it on its line: the block stays empty, and the text
            // behind the semicolon stays behind the block's closing brace, in front of that code
            changes = [
                          openParenChange,
                          new TextChange(usingDeclaration.SemicolonToken.Span, $"){endOfLine}{indentation}{{{endOfLine}{indentation}}}")
                      ];

            return true;
        }

        if (lines.GetLineFromPosition(movedSpan.End).Start != movedSpan.End
            || SyntaxIndentationUtilities.HoldsTabInFrontOfMovingLine(root, text, movedSpan)
            || SyntaxIndentationUtilities.TryShiftMovedLines(root, text, movedSpan, IndentationSize, out var movedText) == false)
        {
            return false;
        }

        changes = [
                      openParenChange,
                      new TextChange(usingDeclaration.SemicolonToken.Span, ")"),
                      new TextChange(movedSpan, $"{indentation}{{{endOfLine}{movedText}{indentation}}}{endOfLine}")
                  ];

        return true;
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
                && TryCreateChanges(root, usingDeclaration, out _))
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