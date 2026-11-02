using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;
using Reihitsu.Formatter.Pipeline.Core.Utilities;
using Reihitsu.Formatter.Pipeline.LineBreaks.Utilities;

namespace Reihitsu.Formatter.Pipeline.LineBreaks.Rewriter;

/// <summary>
/// Applies line-break rules for blocks, accessor lists, switch braces, and the lines that switch sections, their labels, and
/// their first statements start on. It is the one owner of the braces of every block and every event accessor list, member
/// bodies included
/// </summary>
internal sealed class LineBreakBlockRewriter : CSharpSyntaxRewriter
{
    #region Fields

    /// <summary>
    /// The cancellation token
    /// </summary>
    private readonly CancellationToken _cancellationToken;

    /// <summary>
    /// The token gap normalizer
    /// </summary>
    private readonly TokenGapNormalizer _gapNormalizer;

    /// <summary>
    /// The brace placer
    /// </summary>
    private readonly BracePlacer _bracePlacer;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="gapNormalizer">The token gap normalizer</param>
    /// <param name="bracePlacer">The brace placer</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public LineBreakBlockRewriter(TokenGapNormalizer gapNormalizer,
                                  BracePlacer bracePlacer,
                                  CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
        _gapNormalizer = gapNormalizer;
        _bracePlacer = bracePlacer;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Ensures sibling statements in a statement list start on separate lines (FMT-BREAK-006). An empty statement and the
    /// statement next to it are left as written, unless a statement rule of the blank-line phase separates the two by a blank
    /// line; which pairs it separates is read from <see cref="BlankLineSpacingPolicy"/>, the rule the blank-line phase applies,
    /// so that phase, which runs later, adds only the blank line. For such a pair, a statement behind the last line of a
    /// multi-line comment does not start its line although a line break lies in the gap (SHARED-COMMENT-012), so it is moved
    /// onto the line below the comment here, and the blank-line phase never writes the line break that ends the line in
    /// front of it. Other pairs whose gap already holds a line break are left as written
    /// </summary>
    /// <typeparam name="TNode">The syntax node type owning the statement list</typeparam>
    /// <param name="node">The node owning the statement list</param>
    /// <param name="getStatements">Selects the statement list from the current node</param>
    /// <param name="withStatements">Replaces the statement list on the node</param>
    /// <returns>The updated node</returns>
    private TNode EnsureStatementsStartOnSeparateLines<TNode>(TNode node,
                                                              Func<TNode, SyntaxList<StatementSyntax>> getStatements,
                                                              Func<TNode, SyntaxList<StatementSyntax>, TNode> withStatements)
        where TNode : SyntaxNode
    {
        if (getStatements(node).Count <= 1)
        {
            return node;
        }

        var modified = false;
        var statements = getStatements(node).ToArray();

        for (var statementIndex = 1; statementIndex < statements.Length; statementIndex++)
        {
            var requiresBlankLine = BlankLineSpacingPolicy.RequiresBlankLineBetweenStatements(statements[statementIndex - 1], statements[statementIndex]);

            if ((statements[statementIndex - 1] is EmptyStatementSyntax
                 || statements[statementIndex] is EmptyStatementSyntax)
                && requiresBlankLine == false)
            {
                continue;
            }

            var previousToken = statements[statementIndex - 1].GetLastToken();
            var currentToken = statements[statementIndex].GetFirstToken();

            if (TokenGapUtilities.HasLineBreakBetween(previousToken, currentToken) == false)
            {
                statements[statementIndex] = _gapNormalizer.NormalizeGapBeforeToken(statements[statementIndex], currentToken);
                modified = true;
            }
            else if (requiresBlankLine)
            {
                var placedStatement = _gapNormalizer.EnsureTokenStartsLine(statements[statementIndex], currentToken);

                if (placedStatement != statements[statementIndex])
                {
                    statements[statementIndex] = placedStatement;
                    modified = true;
                }
            }
        }

        return modified
                   ? withStatements(node, SyntaxFactory.List(statements))
                   : node;
    }

    /// <summary>
    /// Ensures every switch section behind the first starts its own line (FMT-BREAK-033). The first section starts its line
    /// behind the opening brace (FMT-BREAK-002)
    /// </summary>
    /// <param name="node">The switch statement</param>
    /// <returns>The switch statement with every section starting its own line</returns>
    private SwitchStatementSyntax EnsureSectionsStartOnSeparateLines(SwitchStatementSyntax node)
    {
        for (var sectionIndex = 1; sectionIndex < node.Sections.Count; sectionIndex++)
        {
            node = _gapNormalizer.EnsureTokenStartsLine(node, node.Sections[sectionIndex].GetFirstToken());
        }

        return node;
    }

    /// <summary>
    /// Ensures every label of a switch section behind the first starts its own line (FMT-BREAK-034), and the section's first
    /// statement starts its own line behind the labels (FMT-BREAK-035). An empty first statement is left as written, as
    /// FMT-BREAK-006 leaves it between statements, and a block that forms the section's body is placed by
    /// <see cref="BracePlacer.NormalizeContainedBlock"/> (FMT-BREAK-008). The section's first label is placed by the switch
    /// statement (FMT-BREAK-033)
    /// </summary>
    /// <param name="node">The switch section</param>
    /// <returns>The switch section with its labels and its first statement starting their own lines</returns>
    private SwitchSectionSyntax EnsureLabelsAndFirstStatementStartOnSeparateLines(SwitchSectionSyntax node)
    {
        for (var labelIndex = 1; labelIndex < node.Labels.Count; labelIndex++)
        {
            node = _gapNormalizer.EnsureTokenStartsLine(node, node.Labels[labelIndex].GetFirstToken());
        }

        if (node.Statements.Count > 0
            && (node.Statements[0] is EmptyStatementSyntax or BlockSyntax) == false)
        {
            node = _gapNormalizer.EnsureTokenStartsLine(node, node.Statements[0].GetFirstToken());
        }

        return node;
    }

    /// <summary>
    /// Places the braces of a member body again on the declaration that owns them. The block was laid out when it was
    /// visited, but a block whose content changed during the visit is detached from the token in front of its open brace,
    /// so its open brace can only be placed here, where that token lies inside the node
    /// </summary>
    /// <typeparam name="TNode">The declaration type</typeparam>
    /// <param name="node">The visited declaration</param>
    /// <returns>The declaration with the braces of its body placed</returns>
    private TNode NormalizeBodyBraces<TNode>(TNode node)
        where TNode : BaseMethodDeclarationSyntax
    {
        _cancellationToken.ThrowIfCancellationRequested();

        return node?.Body == null
                   ? node
                   : _bracePlacer.NormalizeOwnedBraces(node, static declaration => declaration.Body.OpenBraceToken, static declaration => declaration.Body.CloseBraceToken);
    }

    #endregion // Methods

    #region CSharpSyntaxVisitor

    /// <inheritdoc/>
    public override SyntaxNode VisitBlock(BlockSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (BlockSyntax)base.VisitBlock(node);

        if (node == null)
        {
            return null;
        }

        node = _bracePlacer.EnsureBraceOnOwnLine(node, owner => owner.OpenBraceToken, (owner, token) => owner.WithOpenBraceToken(token), owner => owner.CloseBraceToken, (owner, token) => owner.WithCloseBraceToken(token));
        node = _bracePlacer.EnsureFirstContentOnNewLine(node, node.OpenBraceToken);

        node = EnsureStatementsStartOnSeparateLines(node, static block => block.Statements, static (block, statements) => block.WithStatements(statements));

        return node;
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitAccessorList(AccessorListSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (AccessorListSyntax)base.VisitAccessorList(node);

        if (node == null)
        {
            return null;
        }

        if (LineBreakDetection.ShouldNormalizeAccessorListBraces(node) == false)
        {
            return node;
        }

        node = _bracePlacer.EnsureBraceOnOwnLine(node, owner => owner.OpenBraceToken, (owner, token) => owner.WithOpenBraceToken(token), owner => owner.CloseBraceToken, (owner, token) => owner.WithCloseBraceToken(token));
        node = _bracePlacer.EnsureFirstContentOnNewLine(node, node.OpenBraceToken);
        node = _bracePlacer.EnsureAccessorsStartOnSeparateLines(node, static accessorList => accessorList);

        return node;
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
    {
        return NormalizeBodyBraces((ConstructorDeclarationSyntax)base.VisitConstructorDeclaration(node));
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        return NormalizeBodyBraces((MethodDeclarationSyntax)base.VisitMethodDeclaration(node));
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitOperatorDeclaration(OperatorDeclarationSyntax node)
    {
        return NormalizeBodyBraces((OperatorDeclarationSyntax)base.VisitOperatorDeclaration(node));
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitConversionOperatorDeclaration(ConversionOperatorDeclarationSyntax node)
    {
        return NormalizeBodyBraces((ConversionOperatorDeclarationSyntax)base.VisitConversionOperatorDeclaration(node));
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitDestructorDeclaration(DestructorDeclarationSyntax node)
    {
        return NormalizeBodyBraces((DestructorDeclarationSyntax)base.VisitDestructorDeclaration(node));
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitLocalFunctionStatement(LocalFunctionStatementSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (LocalFunctionStatementSyntax)base.VisitLocalFunctionStatement(node);

        return node?.Body == null
                   ? node
                   : _bracePlacer.NormalizeOwnedBraces(node, static localFunction => localFunction.Body.OpenBraceToken, static localFunction => localFunction.Body.CloseBraceToken);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitEventDeclaration(EventDeclarationSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (EventDeclarationSyntax)base.VisitEventDeclaration(node);

        if (node?.AccessorList == null
            || LineBreakDetection.ShouldNormalizeAccessorListBraces(node.AccessorList) == false)
        {
            return node;
        }

        node = _bracePlacer.NormalizeOwnedBraces(node, static declaration => declaration.AccessorList.OpenBraceToken, static declaration => declaration.AccessorList.CloseBraceToken);

        return _bracePlacer.EnsureAccessorsStartOnSeparateLines(node, static declaration => declaration.AccessorList);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitSwitchStatement(SwitchStatementSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (SwitchStatementSyntax)base.VisitSwitchStatement(node);

        if (node == null)
        {
            return null;
        }

        node = _bracePlacer.EnsureBraceOnOwnLine(node, owner => owner.OpenBraceToken, (owner, token) => owner.WithOpenBraceToken(token), owner => owner.CloseBraceToken, (owner, token) => owner.WithCloseBraceToken(token));
        node = _bracePlacer.EnsureFirstContentOnNewLine(node, node.OpenBraceToken);
        node = EnsureSectionsStartOnSeparateLines(node);

        return node;
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitSwitchExpression(SwitchExpressionSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (SwitchExpressionSyntax)base.VisitSwitchExpression(node);

        if (node == null)
        {
            return null;
        }

        node = _bracePlacer.EnsureBraceOnOwnLine(node, owner => owner.OpenBraceToken, (owner, token) => owner.WithOpenBraceToken(token), owner => owner.CloseBraceToken, (owner, token) => owner.WithCloseBraceToken(token));
        node = _bracePlacer.EnsureFirstContentOnNewLine(node, node.OpenBraceToken);

        return node;
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitSwitchSection(SwitchSectionSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (SwitchSectionSyntax)base.VisitSwitchSection(node);

        if (node == null)
        {
            return null;
        }

        node = EnsureLabelsAndFirstStatementStartOnSeparateLines(node);
        node = EnsureStatementsStartOnSeparateLines(node, static section => section.Statements, static (section, statements) => section.WithStatements(statements));

        if (node.Statements.Count > 0 && node.Statements[0] is BlockSyntax block)
        {
            node = _bracePlacer.NormalizeContainedBlock(node, block);
        }

        return node;
    }

    #endregion // CSharpSyntaxVisitor
}