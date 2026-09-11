using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Core;

/// <summary>
/// Shared syntax-depth policy for indentation-sensitive formatting and code fixes
/// </summary>
public static class SyntaxIndentationUtilities
{
    #region Constants

    /// <summary>
    /// Number of spaces in one indentation level
    /// </summary>
    public const int IndentSize = 4;

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Computes the base indentation level of a node from its syntax-tree ownership
    /// </summary>
    /// <param name="node">Node whose base indentation level should be computed</param>
    /// <returns>Number of indenting ancestor constructs</returns>
    public static int ComputeBaseIndentLevel(SyntaxNode node)
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
    /// Computes the indentation level of a statement using the same direct-child scope transitions as full-tree
    /// layout. This model recognizes only brace-owning scopes and switch sections - see
    /// <see cref="GetIndentingScopeRange"/> for why <see cref="InitializerExpressionSyntax"/> and
    /// <see cref="AnonymousObjectCreationExpressionSyntax"/> are deliberately absent - and answers a different
    /// question from <see cref="ComputeBaseIndentLevel"/>, which recognizes those two kinds in addition
    /// </summary>
    /// <param name="statement">Statement whose indentation level should be computed</param>
    /// <returns>Number of indentation levels owned by the statement's syntax ancestors</returns>
    public static int ComputeStatementIndentLevel(StatementSyntax statement)
    {
        var indentLevel = 0;
        SyntaxNode child = statement;
        var parent = child.Parent;

        while (parent != null)
        {
            indentLevel = GetChildIndentLevel(parent, child, indentLevel);
            child = parent;
            parent = child.Parent;
        }

        return indentLevel;
    }

    /// <summary>
    /// Computes the indentation level inherited by a direct child of a syntax node
    /// </summary>
    /// <param name="parent">Syntax node that owns the child</param>
    /// <param name="child">Direct child node or token</param>
    /// <param name="indentLevel">Indentation level of the parent</param>
    /// <returns>Indentation level of the child</returns>
    public static int GetChildIndentLevel(SyntaxNode parent, SyntaxNodeOrToken child, int indentLevel)
    {
        var childIndentLevel = indentLevel;

        if (IsInsideScopeRange(child.SpanStart, GetIndentingScopeRange(parent)))
        {
            childIndentLevel++;
        }

        if (child.IsNode && child.AsNode() == GetEmbeddedStatement(parent))
        {
            childIndentLevel++;
        }

        return childIndentLevel;
    }

    /// <summary>
    /// Computes the indentation level inherited by trivia that belongs to a direct child token
    /// </summary>
    /// <param name="parent">Syntax node that owns the child token</param>
    /// <param name="trivia">Trivia whose indentation level should be computed</param>
    /// <param name="indentLevel">Indentation level of the parent</param>
    /// <returns>Indentation level of the trivia</returns>
    public static int GetTriviaIndentLevel(SyntaxNode parent, SyntaxTrivia trivia, int indentLevel)
    {
        return IsInsideScopeRange(trivia.SpanStart, GetIndentingScopeRange(parent))
                   ? indentLevel + 1
                   : indentLevel;
    }

    /// <summary>
    /// Determines whether a syntax node owns an indenting scope
    /// </summary>
    /// <param name="node">Syntax node to inspect</param>
    /// <returns><see langword="true"/> if the node owns an indenting scope</returns>
    public static bool IsIndentingScope(SyntaxNode node)
    {
        return GetIndentingScopeRange(node) != null;
    }

    /// <summary>
    /// Determines whether any ancestor of a node, all the way to the root, is an object initializer or an
    /// anonymous object - not only the nearest one. Both are anchor-derived rather than level-derived: their
    /// members align to a token's own column plus one indentation size, not to a multiple of
    /// <see cref="IndentSize"/> counted from ancestry, so no caller of <see cref="ComputeBaseIndentLevel"/> or
    /// <see cref="ComputeStatementIndentLevel"/> can turn "one more initializer" into the column the formatter's
    /// own alignment contributors would place it at. The search cannot stop at the first brace scope it meets:
    /// a block nested inside an initializer is itself anchor-positioned, even though the block itself is a
    /// recognized brace scope. A caller that needs that column when this returns <see langword="true"/> has to
    /// read it from the current source text instead of computing it (issue #748)
    /// </summary>
    /// <param name="node">Node to inspect</param>
    /// <returns><see langword="true"/> if such an ancestor exists</returns>
    public static bool HasAnchorScopeAncestor(SyntaxNode node)
    {
        for (var ancestor = node.Parent; ancestor != null; ancestor = ancestor.Parent)
        {
            if (ancestor is InitializerExpressionSyntax or AnonymousObjectCreationExpressionSyntax)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether a position that leads a shared source line falls inside a switch section's own label
    /// region rather than a sibling statement, trivia, or a different section's label. A section's own statements
    /// sit exactly one indentation level deeper than the labels they belong to - see
    /// <see cref="GetIndentingScopeRange"/> - so a caller that reads a shared line's leading whitespace as an
    /// anchor-derived column (issue #748) has to add that one level only when the line is actually led by this
    /// section's own label, never merely because the line's leading whitespace does not already match the target's
    /// own preceding sibling. Applying the level whenever that weaker condition holds misfires on every other thing
    /// that can lead such a line - an earlier sibling statement, trivia before the section's first statement, or a
    /// different section's label - and adds a spurious level there instead (issue #786)
    /// </summary>
    /// <param name="switchSection">Switch section that directly owns the statement being indented</param>
    /// <param name="contentStart">Position immediately following the shared line's own leading whitespace</param>
    /// <returns><see langword="true"/> if <paramref name="contentStart"/> lies inside the section's own label region</returns>
    public static bool IsWithinSwitchSectionLabelRegion(SwitchSectionSyntax switchSection, int contentStart)
    {
        return contentStart >= switchSection.SpanStart
               && contentStart < switchSection.Labels[switchSection.Labels.Count - 1].Span.End;
    }

    /// <summary>
    /// Determines whether an ancestor owns an indenting scope containing the specified position. This model
    /// additionally recognizes <see cref="InitializerExpressionSyntax"/> and
    /// <see cref="AnonymousObjectCreationExpressionSyntax"/>, unlike <see cref="GetIndentingScopeRange"/>, because
    /// its only uncancelled consumer - <see cref="ComputeBaseIndentLevel"/>'s callers in
    /// <c>ReihitsuFormatter.FormatNode</c> and the document-scoped overloads - needs an absolute level for every
    /// node, including one nested in an initializer, rather than a level that stops at the nearest brace scope.
    /// The approximation this produces for an initializer-nested node is deliberately inexact (issue #748): it
    /// undercounts the anchor-derived column by not accounting for the initializer's own alignment, but the two
    /// document-scoped overloads cancel that undercount as a uniform column offset against the node's own
    /// original position, and the detached overload's only production caller can never reach an initializer
    /// ancestor. A caller that instead needs the exact anchor-derived column - as the code fixes in
    /// <c>Reihitsu.Analyzer.CodeFixes</c> do - must check <see cref="HasAnchorScopeAncestor"/> and read the
    /// column from source text when it returns <see langword="true"/>, the same way those code fixes do
    /// </summary>
    /// <param name="node">Ancestor to inspect</param>
    /// <param name="spanStart">Position of the nested node</param>
    /// <returns><see langword="true"/> if the ancestor adds one indentation level</returns>
    private static bool IsIndentingAncestor(SyntaxNode node, int spanStart)
    {
        switch (node)
        {
            case BlockSyntax block:
                return IsBetweenBraces(spanStart, block.OpenBraceToken, block.CloseBraceToken);

            case TypeDeclarationSyntax typeDeclaration:
                return IsBetweenBraces(spanStart, typeDeclaration.OpenBraceToken, typeDeclaration.CloseBraceToken);

            case NamespaceDeclarationSyntax namespaceDeclaration:
                return IsBetweenBraces(spanStart, namespaceDeclaration.OpenBraceToken, namespaceDeclaration.CloseBraceToken);

            case EnumDeclarationSyntax enumDeclaration:
                return IsBetweenBraces(spanStart, enumDeclaration.OpenBraceToken, enumDeclaration.CloseBraceToken);

            case SwitchStatementSyntax switchStatement:
                return IsBetweenBraces(spanStart, switchStatement.OpenBraceToken, switchStatement.CloseBraceToken);

            case SwitchSectionSyntax switchSection:
                return IsInsideScopeRange(spanStart, GetIndentingScopeRange(switchSection));

            case AccessorListSyntax accessorList:
                return IsBetweenBraces(spanStart, accessorList.OpenBraceToken, accessorList.CloseBraceToken);

            case InitializerExpressionSyntax initializer:
                return IsBetweenBraces(spanStart, initializer.OpenBraceToken, initializer.CloseBraceToken);

            case AnonymousObjectCreationExpressionSyntax anonymousObject:
                return IsBetweenBraces(spanStart, anonymousObject.OpenBraceToken, anonymousObject.CloseBraceToken);

            default:
                return false;
        }
    }

    /// <summary>
    /// Gets an unbraced embedded statement that receives one indentation level from its owner
    /// </summary>
    /// <param name="node">Potential embedded-statement owner</param>
    /// <returns>The embedded statement, or <see langword="null"/> when the owner has none or owns a block</returns>
    private static StatementSyntax GetEmbeddedStatement(SyntaxNode node)
    {
        var statement = node switch
                        {
                            IfStatementSyntax ifStatement => ifStatement.Statement,
                            ElseClauseSyntax { Statement: IfStatementSyntax } => null,
                            ElseClauseSyntax elseClause => elseClause.Statement,
                            WhileStatementSyntax whileStatement => whileStatement.Statement,
                            DoStatementSyntax doStatement => doStatement.Statement,
                            ForStatementSyntax forStatement => forStatement.Statement,
                            CommonForEachStatementSyntax forEachStatement => forEachStatement.Statement,
                            UsingStatementSyntax usingStatement => usingStatement.Statement,
                            LockStatementSyntax lockStatement => lockStatement.Statement,
                            FixedStatementSyntax fixedStatement => fixedStatement.Statement,
                            _ => null
                        };

        return statement is BlockSyntax ? null : statement;
    }

    /// <summary>
    /// Gets the range of a syntax scope that adds one indentation level to its children, backing
    /// <see cref="GetChildIndentLevel"/>, <see cref="GetTriviaIndentLevel"/>, and <see cref="IsIndentingScope"/>.
    /// A switch section is expressed as the interval spanning its own statements rather than as a brace pair,
    /// because a section owns no braces of its own; the interval starts at the first statement's own start so
    /// that statement is included, matching the direct-child arm this model previously carried for switch
    /// sections. This model deliberately omits <see cref="InitializerExpressionSyntax"/> and
    /// <see cref="AnonymousObjectCreationExpressionSyntax"/>, unlike <see cref="IsIndentingAncestor"/>: their
    /// members are anchor-derived, aligned to a token's own column plus one indentation size rather than a
    /// brace-scope level, and pass-2 alignment contributors in <c>Reihitsu.Formatter.Pipeline.Indentation</c> -
    /// which this model's consumers (<see cref="GetChildIndentLevel"/>'s <c>LayoutComputer</c> and RH5204 callers)
    /// already delegate to for those columns - already own them (issue #748)
    /// </summary>
    /// <param name="node">Potential scope owner</param>
    /// <returns>Start and end of the indenting range; otherwise, <see langword="null"/></returns>
    private static (int Start, int End)? GetIndentingScopeRange(SyntaxNode node)
    {
        SyntaxToken openBrace;
        SyntaxToken closeBrace;

        switch (node)
        {
            case NamespaceDeclarationSyntax namespaceDeclaration:
                {
                    openBrace = namespaceDeclaration.OpenBraceToken;
                    closeBrace = namespaceDeclaration.CloseBraceToken;
                }
                break;

            case BaseTypeDeclarationSyntax typeDeclaration:
                {
                    openBrace = typeDeclaration.OpenBraceToken;
                    closeBrace = typeDeclaration.CloseBraceToken;
                }
                break;

            case BlockSyntax block:
                {
                    openBrace = block.OpenBraceToken;
                    closeBrace = block.CloseBraceToken;
                }
                break;

            case SwitchStatementSyntax switchStatement:
                {
                    openBrace = switchStatement.OpenBraceToken;
                    closeBrace = switchStatement.CloseBraceToken;
                }
                break;

            case AccessorListSyntax accessorList:
                {
                    openBrace = accessorList.OpenBraceToken;
                    closeBrace = accessorList.CloseBraceToken;
                }
                break;

            case SwitchSectionSyntax switchSection:
                {
                    return switchSection.Statements.Count == 0
                               ? null
                               : (switchSection.Statements[0].SpanStart, switchSection.Statements[switchSection.Statements.Count - 1].Span.End);
                }

            default:
                {
                    return null;
                }
        }

        return openBrace.IsMissing || closeBrace.IsMissing
                   ? null
                   : (openBrace.Span.End, closeBrace.SpanStart);
    }

    /// <summary>
    /// Determines whether a position lies within an optional indenting range
    /// </summary>
    /// <param name="position">Position to inspect</param>
    /// <param name="scopeRange">Optional indenting range</param>
    /// <returns><see langword="true"/> if the position lies inside the range</returns>
    private static bool IsInsideScopeRange(int position, (int Start, int End)? scopeRange)
    {
        return scopeRange is { } range
               && position >= range.Start
               && position < range.End;
    }

    /// <summary>
    /// Determines whether a position lies between two brace tokens
    /// </summary>
    /// <param name="spanStart">Position to inspect</param>
    /// <param name="openBrace">Opening brace</param>
    /// <param name="closeBrace">Closing brace</param>
    /// <returns><see langword="true"/> if the position lies between the braces</returns>
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