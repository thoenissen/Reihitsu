using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter;

/// <summary>
/// Reihitsu code formatter
/// </summary>
public static class ReihitsuFormatter
{
    #region Methods

    /// <summary>
    /// Formats an entire document by applying all formatting rules.
    /// </summary>
    /// <param name="document">The Roslyn Document to format.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A new Document with formatting applied.</returns>
    public static async Task<Document> FormatDocumentAsync(Document document, CancellationToken cancellationToken = default)
    {
        var syntaxTree = await document.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);

        if (syntaxTree == null)
        {
            return document;
        }

        var root = await syntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(false);

        if (HasSyntaxErrors(syntaxTree))
        {
            return document;
        }

        var endOfLine = DetectEndOfLine(root);
        var context = new FormattingContext(endOfLine, document);
        var formattedRoot = FormattingPipeline.Execute(root, context, cancellationToken);

        return document.WithSyntaxRoot(formattedRoot);
    }

    /// <summary>
    /// Formats a syntax tree by applying all formatting rules.
    /// Use this overload when no Workspace/Document context is available.
    /// </summary>
    /// <param name="syntaxTree">The syntax tree to format.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A new SyntaxTree with formatting applied.</returns>
    public static SyntaxTree FormatSyntaxTree(SyntaxTree syntaxTree, CancellationToken cancellationToken = default)
    {
        if (HasSyntaxErrors(syntaxTree))
        {
            return syntaxTree;
        }

        var root = syntaxTree.GetRoot(cancellationToken);
        var endOfLine = DetectEndOfLine(root);
        var context = new FormattingContext(endOfLine);
        var formattedRoot = FormattingPipeline.Execute(root, context, cancellationToken);

        return syntaxTree.WithRootAndOptions(formattedRoot, syntaxTree.Options);
    }

    /// <summary>
    /// Formats a specific syntax node and its descendants.
    /// Useful for code fix providers that need to format only a
    /// newly generated or modified node rather than the full document.
    /// </summary>
    /// <param name="node">The syntax node to format.</param>
    /// <param name="indentLevel">The indentation level of the node within its containing document.
    /// Required for newly generated nodes that are not yet inserted into a tree,
    /// where the indentation level cannot be inferred from position.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A new SyntaxNode with formatting applied.</returns>
    public static SyntaxNode FormatNode(SyntaxNode node, int indentLevel = -1, CancellationToken cancellationToken = default)
    {
        var endOfLine = DetectEndOfLine(node);
        var baseIndentLevel = indentLevel >= 0 ? indentLevel : ComputeBaseIndentLevel(node);
        var context = new FormattingContext(endOfLine, baseIndentLevel: baseIndentLevel);

        return FormattingPipeline.Execute(node, context, cancellationToken);
    }

    /// <summary>
    /// Detects the predominant end-of-line sequence in the syntax node.
    /// Falls back to <see cref="Environment.NewLine"/> if no end-of-line trivia is found.
    /// </summary>
    /// <param name="node">The syntax node to analyze.</param>
    /// <returns>The detected end-of-line sequence.</returns>
    private static string DetectEndOfLine(SyntaxNode node)
    {
        var firstEndOfLine = node.DescendantTrivia()
                                 .FirstOrDefault(t => t.IsKind(SyntaxKind.EndOfLineTrivia));

        if (firstEndOfLine != default)
        {
            return firstEndOfLine.ToString();
        }

        return Environment.NewLine;
    }

    /// <summary>
    /// Checks whether the syntax tree contains any syntax errors.
    /// Files with syntax errors are skipped by the formatter.
    /// </summary>
    /// <param name="syntaxTree">The syntax tree to check.</param>
    /// <returns><see langword="true"/> if the tree has error-level diagnostics; otherwise, <see langword="false"/>.</returns>
    private static bool HasSyntaxErrors(SyntaxTree syntaxTree)
    {
        return syntaxTree.GetDiagnostics()
                         .Any(d => d.Severity == DiagnosticSeverity.Error);
    }

    /// <summary>
    /// Computes the base indentation level of a node from its position in the syntax tree.
    /// Walks the parent chain and counts indenting constructs (types, namespaces, blocks, etc.).
    /// </summary>
    /// <param name="node">The node whose base indentation level to compute.</param>
    /// <returns>The number of indenting ancestor constructs.</returns>
    private static int ComputeBaseIndentLevel(SyntaxNode node)
    {
        var spanStart = node.SpanStart;
        var level = 0;
        var parent = node.Parent;

        while (parent != null)
        {
            if (IsIndentingAncestor(parent, spanStart))
            {
                level++;
            }

            parent = parent.Parent;
        }

        return level;
    }

    /// <summary>
    /// Determines whether the given ancestor node is an indenting construct
    /// that contains the specified span position within its braces.
    /// </summary>
    /// <param name="node">The ancestor node to check.</param>
    /// <param name="spanStart">The span start position of the node being checked.</param>
    /// <returns><c>true</c> if the ancestor increases indentation; otherwise, <c>false</c>.</returns>
    private static bool IsIndentingAncestor(SyntaxNode node, int spanStart)
    {
        switch (node)
        {
            case BlockSyntax block:
                return IsBetweenBraces(spanStart, block.OpenBraceToken, block.CloseBraceToken);

            case TypeDeclarationSyntax typeDecl:
                return IsBetweenBraces(spanStart, typeDecl.OpenBraceToken, typeDecl.CloseBraceToken);

            case NamespaceDeclarationSyntax nsDecl:
                return IsBetweenBraces(spanStart, nsDecl.OpenBraceToken, nsDecl.CloseBraceToken);

            case EnumDeclarationSyntax enumDecl:
                return IsBetweenBraces(spanStart, enumDecl.OpenBraceToken, enumDecl.CloseBraceToken);

            case SwitchStatementSyntax switchStmt:
                return IsBetweenBraces(spanStart, switchStmt.OpenBraceToken, switchStmt.CloseBraceToken);

            case AccessorListSyntax accessorList:
                return IsBetweenBraces(spanStart, accessorList.OpenBraceToken, accessorList.CloseBraceToken);

            case InitializerExpressionSyntax initializer:
                return IsBetweenBraces(spanStart, initializer.OpenBraceToken, initializer.CloseBraceToken);

            case AnonymousObjectCreationExpressionSyntax anon:
                return IsBetweenBraces(spanStart, anon.OpenBraceToken, anon.CloseBraceToken);

            default:
                return false;
        }
    }

    /// <summary>
    /// Determines whether a span position falls between the open and close brace tokens.
    /// </summary>
    /// <param name="spanStart">The span start position to check.</param>
    /// <param name="openBrace">The open brace token.</param>
    /// <param name="closeBrace">The close brace token.</param>
    /// <returns><c>true</c> if the position is between the braces; otherwise, <c>false</c>.</returns>
    private static bool IsBetweenBraces(int spanStart, SyntaxToken openBrace, SyntaxToken closeBrace)
    {
        if (openBrace.IsMissing || closeBrace.IsMissing)
        {
            return false;
        }

        return spanStart > openBrace.SpanStart && spanStart < closeBrace.SpanStart;
    }

    #endregion // Methods
}