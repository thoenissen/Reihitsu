using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
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
    /// Converts the local function by formatting it. A local function that shares its line with a preceding label starts
    /// in the middle of that line, so it is formatted in the context of its outermost label, which anchors the new body at
    /// the statement's column. A local function that starts its own line keeps its own column, labeled or not
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="localFunction">Local function</param>
    /// <param name="element">The list element that ends with the local function</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task<Document> FormatAsync(Document document, LocalFunctionStatementSyntax localFunction, StatementSyntax element, CancellationToken cancellationToken)
    {
        if (element == localFunction
            || ReihitsuFormatterHelpers.StartsOnNewLine(localFunction.GetFirstToken()))
        {
            return await ReihitsuFormatter.FormatNodeInDocumentAsync(document, localFunction, cancellationToken).ConfigureAwait(false);
        }

        return await ReihitsuFormatter.FormatNodeInDocumentWithContextAsync(document, localFunction, element, cancellationToken).ConfigureAwait(false);
    }

    #endregion // Methods

    #region ExpressionBodyToBlockCodeFixProviderBase

    /// <inheritdoc/>
    protected override async Task<Document> ApplyCodeFixAsync(Document document, LocalFunctionStatementSyntax node, CancellationToken cancellationToken)
    {
        // Formatting a single node never decides the gap below it, while the blank-line-after-closing-brace rule does.
        // When the closing brace this conversion creates ends directly above the next statement of the same list, the
        // two are separated by a blank line so the fix does not raise that rule on its own brace.
        var element = GetListElement(node);
        var statements = GetOwningStatements(element);
        var index = statements.IndexOf(element);

        if (index < 0 || index == statements.Count - 1)
        {
            return await FormatAsync(document, node, element, cancellationToken).ConfigureAwait(false);
        }

        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        // The formatter replaces only the local function, so the tracked next statement survives the rewrite unchanged and
        // its predecessor in the list is the element that now ends with the converted body
        var nextStatement = statements[index + 1];
        var trackedDocument = document.WithSyntaxRoot(root.TrackNodes(node, element, nextStatement));
        var trackedRoot = await trackedDocument.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var formattedDocument = await FormatAsync(trackedDocument, trackedRoot.GetCurrentNode(node), trackedRoot.GetCurrentNode(element), cancellationToken).ConfigureAwait(false);
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

        return formattedDocument.WithSyntaxRoot(BlankLineCodeFixUtilities.InsertBlankLineBefore(formattedRoot, formattedNextStatement.GetFirstToken()));
    }

    #endregion // ExpressionBodyToBlockCodeFixProviderBase
}