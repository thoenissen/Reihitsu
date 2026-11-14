using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.CodeFixes.Core;
using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Core;
using Reihitsu.Formatter;
using Reihitsu.Formatter.Utilities;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Clarity;

/// <summary>
/// Code fix provider for <see cref="RH3208ExpressionStyleLocalFunctionsShouldNotBeUsedAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH3208ExpressionStyleLocalFunctionsShouldNotBeUsedCodeFixProvider))]
public class RH3208ExpressionStyleLocalFunctionsShouldNotBeUsedCodeFixProvider : ExpressionBodyToBlockCodeFixProviderBase<LocalFunctionStatementSyntax>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH3208ExpressionStyleLocalFunctionsShouldNotBeUsedCodeFixProvider()
        : base(RH3208ExpressionStyleLocalFunctionsShouldNotBeUsedAnalyzer.DiagnosticId, CodeFixResources.RH3208Title)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Gets the statement that represents the local function in its owning statement list. A labeled local function is
    /// represented by its outermost label, whose last token is still the local function's own last token
    /// </summary>
    /// <param name="localFunction">Local function</param>
    /// <returns>The list element that ends with the local function</returns>
    private static StatementSyntax GetListElement(LocalFunctionStatementSyntax localFunction)
    {
        StatementSyntax element = localFunction;

        while (element.Parent is LabeledStatementSyntax labeledStatement)
        {
            element = labeledStatement;
        }

        return element;
    }

    /// <summary>
    /// Gets the statement list that directly owns the statement
    /// </summary>
    /// <param name="statement">Statement</param>
    /// <returns>The owning statement list, or an empty list when the statement is not owned by a block or switch section</returns>
    private static SyntaxList<StatementSyntax> GetOwningStatements(StatementSyntax statement)
    {
        return statement.Parent switch
               {
                   BlockSyntax block => block.Statements,
                   SwitchSectionSyntax switchSection => switchSection.Statements,
                   _ => default
               };
    }

    /// <summary>
    /// Gets the label that starts the local function's line. Labels are walked outward only while the current node does
    /// not start its own line, so an outer label written on an earlier line never becomes the anchor
    /// </summary>
    /// <param name="localFunction">Local function that shares its line with at least one preceding label</param>
    /// <returns>The outermost label on the local function's line</returns>
    private static StatementSyntax GetLineStartingLabel(LocalFunctionStatementSyntax localFunction)
    {
        StatementSyntax node = localFunction;

        while (node.Parent is LabeledStatementSyntax labeledStatement
               && SyntaxTokenPositionUtilities.IsFirstOnLine(node.GetFirstToken()) == false)
        {
            node = labeledStatement;
        }

        return node;
    }

    /// <summary>
    /// Converts the local function's expression body to the block body the formatter produces for it; the rest of the
    /// local function stays as written (SHARED-BODY-003). A local function that shares its line with a preceding label
    /// starts in the middle of that line, so it is formatted in the context of the label that starts the line, which
    /// anchors the new body at that line's column. A local function that starts its own line keeps its own column, labeled
    /// or not
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="localFunction">Local function</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static Task<Document> FormatAsync(Document document, LocalFunctionStatementSyntax localFunction, CancellationToken cancellationToken)
    {
        return ExpressionBodyCodeFixUtilities.ConvertBodyAsync(document, localFunction, FormatLocalFunctionAsync, cancellationToken);
    }

    /// <summary>
    /// Formats the local function, in the context of the label that starts its line when it does not start the line itself
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="node">Local function</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The document with the formatted local function</returns>
    private static async Task<Document> FormatLocalFunctionAsync(Document document, SyntaxNode node, CancellationToken cancellationToken)
    {
        var localFunction = (LocalFunctionStatementSyntax)node;
        var contextNode = GetLineStartingLabel(localFunction);

        if (contextNode == localFunction)
        {
            return (await ReihitsuFormatter.FormatNodeInDocumentAsync(document, localFunction, cancellationToken).ConfigureAwait(false)).Output;
        }

        return (await ReihitsuFormatter.FormatNodeInDocumentWithContextAsync(document, localFunction, contextNode, cancellationToken).ConfigureAwait(false)).Output;
    }

    /// <summary>
    /// Gets the statement that follows the local function in its owning statement list
    /// </summary>
    /// <param name="element">The list element that ends with the local function</param>
    /// <returns>The next statement, or <see langword="null"/> when the local function is the last statement or not in a list</returns>
    private static StatementSyntax GetNextStatement(StatementSyntax element)
    {
        var statements = GetOwningStatements(element);
        var index = statements.IndexOf(element);

        return index < 0 || index == statements.Count - 1
                   ? null
                   : statements[index + 1];
    }

    #endregion // Methods

    #region ExpressionBodyToBlockCodeFixProviderBase

    /// <inheritdoc/>
    /// <remarks>
    /// The fix puts a blank line between the new closing brace and the next statement, and moves that statement below a
    /// comment that holds every line break of the gap (SHARED-AFTERBRACE-002). It is withheld where that statement would be
    /// moved and the owner of the blank line finds no column for it in the text. The local function keeps its first line
    /// when it is converted, so the owner's decision is taken on the original text: the gap below the local function
    /// already spans lines, the next statement is not the terminal <c>break</c> that needs no blank line behind a closing
    /// brace, and the owner cannot place the statement
    /// </remarks>
    protected override bool CanRegisterCodeFix(SyntaxNode root, LocalFunctionStatementSyntax node)
    {
        var element = GetListElement(node);
        var nextStatement = GetNextStatement(element);

        if (nextStatement == null
            || BlankLineSpacingPolicy.RequiresBlankLineBehindClosingBrace(SyntaxKind.CloseBraceToken, BlankLineSpacingPolicy.IsTerminalDirectSwitchSectionBreak(nextStatement)) == false)
        {
            return true;
        }

        var lastLine = element.GetLastToken().GetLocation().GetLineSpan().EndLinePosition.Line;
        var nextLine = nextStatement.GetFirstToken().GetLocation().GetLineSpan().StartLinePosition.Line;

        return lastLine == nextLine
               || BlankLineCodeFixUtilities.CanInsertBlankLineBefore(root, nextStatement.GetFirstToken());
    }

    /// <inheritdoc/>
    protected override SyntaxNode GetFormattingInput(LocalFunctionStatementSyntax node)
    {
        return GetLineStartingLabel(node);
    }

    /// <inheritdoc/>
    protected override async Task<Document> ApplyCodeFixAsync(Document document, LocalFunctionStatementSyntax node, CancellationToken cancellationToken)
    {
        // Formatting a single node never decides the gap below it, while the blank-line-after-closing-brace rule does.
        // When the closing brace this conversion creates ends directly above the next statement of the same list, the
        // two are separated by a blank line so the fix does not raise that rule on its own brace.
        var nextStatement = GetNextStatement(GetListElement(node));

        if (nextStatement == null)
        {
            return await FormatAsync(document, node, cancellationToken).ConfigureAwait(false);
        }

        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        // The formatter replaces only the local function, so the tracked next statement survives the rewrite unchanged and
        // its predecessor in the list is the element that now ends with the converted body
        var trackedDocument = document.WithSyntaxRoot(root.TrackNodes(node, nextStatement));
        var trackedRoot = await trackedDocument.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var formattedDocument = await FormatAsync(trackedDocument, trackedRoot.GetCurrentNode(node), cancellationToken).ConfigureAwait(false);
        var formattedRoot = await formattedDocument.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var formattedNextStatement = formattedRoot?.GetCurrentNode(nextStatement);

        if (formattedNextStatement == null)
        {
            return formattedDocument;
        }

        var formattedStatements = GetOwningStatements(formattedNextStatement);
        var formattedIndex = formattedStatements.IndexOf(formattedNextStatement);

        if (formattedIndex < 1
            || BlankLineSpacingPolicy.RequiresBlankLineAfterClosingBrace(formattedStatements[formattedIndex - 1], formattedNextStatement) == false)
        {
            return formattedDocument;
        }

        // Where the owner finds no column for the moved statement, CanRegisterCodeFix has already withheld the fix
        return BlankLineCodeFixUtilities.TryInsertBlankLineBefore(formattedRoot, formattedNextStatement.GetFirstToken(), behindLeadingDirectives: true, out var updatedRoot)
                   ? formattedDocument.WithSyntaxRoot(updatedRoot)
                   : document;
    }

    #endregion // ExpressionBodyToBlockCodeFixProviderBase
}