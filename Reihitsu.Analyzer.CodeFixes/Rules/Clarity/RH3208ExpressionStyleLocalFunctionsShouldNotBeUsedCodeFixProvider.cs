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
    /// Gets the statement list that directly owns the local function
    /// </summary>
    /// <param name="localFunction">Local function</param>
    /// <returns>The owning statement list, or an empty list when the local function is not owned by a block or switch section</returns>
    private static SyntaxList<StatementSyntax> GetOwningStatements(SyntaxNode localFunction)
    {
        return localFunction.Parent switch
               {
                   BlockSyntax block => block.Statements,
                   SwitchSectionSyntax switchSection => switchSection.Statements,
                   _ => default
               };
    }

    #endregion // Methods

    #region ExpressionBodyToBlockCodeFixProviderBase

    /// <inheritdoc/>
    protected override async Task<Document> ApplyCodeFixAsync(Document document, LocalFunctionStatementSyntax node, CancellationToken cancellationToken)
    {
        // Formatting a single node never decides the gap below it, while the blank-line-after-closing-brace rule does.
        // When the closing brace this conversion creates ends directly above the next statement of the same list, the
        // two are separated by a blank line so the fix does not raise that rule on its own brace.
        var statements = GetOwningStatements(node);
        var index = statements.IndexOf(node);

        if (index < 0 || index == statements.Count - 1)
        {
            return await base.ApplyCodeFixAsync(document, node, cancellationToken).ConfigureAwait(false);
        }

        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        // The formatter replaces only the local function, so the tracked next statement survives the rewrite unchanged
        var nextStatement = statements[index + 1];
        var trackedDocument = document.WithSyntaxRoot(root.TrackNodes(node, nextStatement));
        var trackedRoot = await trackedDocument.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var formattedDocument = await base.ApplyCodeFixAsync(trackedDocument, trackedRoot.GetCurrentNode(node), cancellationToken).ConfigureAwait(false);
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