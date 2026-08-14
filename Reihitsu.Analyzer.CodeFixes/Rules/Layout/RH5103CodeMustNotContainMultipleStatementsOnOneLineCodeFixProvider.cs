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
using Reihitsu.Core;
using Reihitsu.Formatter.Utilities;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Code fix provider for <see cref="RH5103CodeMustNotContainMultipleStatementsOnOneLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5103CodeMustNotContainMultipleStatementsOnOneLineCodeFixProvider))]
public class RH5103CodeMustNotContainMultipleStatementsOnOneLineCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applies the code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="statement">Statement to move</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, StatementSyntax statement, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
        var previousToken = statement.GetFirstToken().GetPreviousToken();
        var previousStatement = GetPreviousStatement(statement);

        if (previousStatement == null)
        {
            return document;
        }

        var replacementText = ReihitsuFormatterHelpers.DetectEndOfLine(root) + GetIndentation(sourceText, statement, previousStatement);
        var replacementSpan = TextSpan.FromBounds(previousToken.Span.End, statement.Span.Start);

        return document.WithText(sourceText.Replace(replacementSpan, replacementText));
    }

    /// <summary>
    /// Gets the exact indentation from the containing statement list when it is available
    /// </summary>
    /// <param name="sourceText">Source text</param>
    /// <param name="statement">Statement to move</param>
    /// <param name="previousStatement">Previous statement in the containing list</param>
    /// <returns>The indentation to place before the moved statement</returns>
    private static string GetIndentation(SourceText sourceText, StatementSyntax statement, StatementSyntax previousStatement)
    {
        var previousLine = sourceText.Lines.GetLineFromPosition(previousStatement.SpanStart);
        var indentation = FormattingTextAnalysisUtilities.GetLeadingWhitespace(FormattingTextAnalysisUtilities.GetLineText(sourceText, previousLine));

        if (statement.Parent is BlockSyntax
            || previousLine.Start + indentation.Length == previousStatement.SpanStart)
        {
            return indentation;
        }

        var indentationColumn = SyntaxIndentationUtilities.ComputeStatementIndentLevel(statement) * SyntaxIndentationUtilities.IndentSize;

        return new string(' ', indentationColumn);
    }

    /// <summary>
    /// Gets the previous sibling statement
    /// </summary>
    /// <param name="statement">Current statement</param>
    /// <returns>The previous statement or <see langword="null"/></returns>
    private static StatementSyntax GetPreviousStatement(StatementSyntax statement)
    {
        if (statement.Parent is BlockSyntax block)
        {
            return block.Statements.TakeWhile(currentStatement => currentStatement != statement)
                                   .LastOrDefault(currentStatement => currentStatement is EmptyStatementSyntax == false);
        }

        if (statement.Parent is SwitchSectionSyntax switchSection)
        {
            return switchSection.Statements.TakeWhile(currentStatement => currentStatement != statement)
                                           .LastOrDefault(currentStatement => currentStatement is EmptyStatementSyntax == false);
        }

        return null;
    }

    /// <summary>
    /// Determines whether the selected statement has a directly editable leading gap
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <param name="statement">Selected statement</param>
    /// <returns><see langword="true"/> if only formatting trivia separates the statements; otherwise, <see langword="false"/></returns>
    private static bool HasEditableLeadingGap(SyntaxNode root, StatementSyntax statement)
    {
        var previousStatement = GetPreviousStatement(statement);

        if (previousStatement == null)
        {
            return false;
        }

        var previousToken = statement.GetFirstToken().GetPreviousToken(includeZeroWidth: true, includeSkipped: true);

        if (previousToken.Parent?.FirstAncestorOrSelf<StatementSyntax>() != previousStatement)
        {
            return false;
        }

        var gap = TextSpan.FromBounds(previousStatement.Span.End, statement.Span.Start);

        return root.SyntaxTree.GetDiagnostics().Any(diagnostic => diagnostic.Location.SourceSpan.IntersectsWith(gap)) == false
               && root.SyntaxTree.GetText()
                                 .ToString(gap)
                                 .All(static character => character is ' ' or '\t');
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH5103CodeMustNotContainMultipleStatementsOnOneLineAnalyzer.DiagnosticId];

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
            var statement = root.FindNode(diagnostic.Location.SourceSpan).FirstAncestorOrSelf<StatementSyntax>();

            if (statement == null)
            {
                continue;
            }

            if (HasEditableLeadingGap(root, statement) == false)
            {
                continue;
            }

            context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH5103Title,
                                                      token => ApplyCodeFixAsync(context.Document, statement, token),
                                                      nameof(RH5103CodeMustNotContainMultipleStatementsOnOneLineCodeFixProvider)),
                                    diagnostic);
        }
    }

    #endregion // CodeFixProvider
}