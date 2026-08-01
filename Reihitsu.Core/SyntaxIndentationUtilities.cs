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
    /// layout
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

        if (IsInsideBraceRange(child.SpanStart, GetIndentingBraceRange(parent)))
        {
            childIndentLevel++;
        }

        if (parent is SwitchSectionSyntax
            && child.IsNode
            && child.AsNode() is StatementSyntax)
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
        return IsInsideBraceRange(trivia.SpanStart, GetIndentingBraceRange(parent))
                   ? indentLevel + 1
                   : indentLevel;
    }

    /// <summary>
    /// Determines whether a syntax node owns an indenting brace range
    /// </summary>
    /// <param name="node">Syntax node to inspect</param>
    /// <returns><see langword="true"/> if the node owns an indenting brace range</returns>
    public static bool IsIndentingBraceScope(SyntaxNode node)
    {
        return GetIndentingBraceRange(node) != null;
    }

    /// <summary>
    /// Determines whether an ancestor owns an indenting scope containing the specified position
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
                return IsInsideSwitchSectionStatements(switchSection, spanStart);

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
    /// Gets the brace range of a syntax scope that adds one indentation level to its children
    /// </summary>
    /// <param name="node">Potential brace-owning scope</param>
    /// <returns>Opening-brace end and closing-brace start; otherwise, <see langword="null"/></returns>
    private static (int OpenEnd, int CloseStart)? GetIndentingBraceRange(SyntaxNode node)
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
    /// Determines whether a position lies within an optional brace range
    /// </summary>
    /// <param name="position">Position to inspect</param>
    /// <param name="braceRange">Optional brace range</param>
    /// <returns><see langword="true"/> if the position lies inside the range</returns>
    private static bool IsInsideBraceRange(int position, (int OpenEnd, int CloseStart)? braceRange)
    {
        return braceRange is { } range
               && position >= range.OpenEnd
               && position < range.CloseStart;
    }

    /// <summary>
    /// Determines whether a position lies inside a statement owned directly by a switch section
    /// </summary>
    /// <param name="switchSection">Switch section to inspect</param>
    /// <param name="spanStart">Position of the nested node</param>
    /// <returns><see langword="true"/> if the position lies within a section statement</returns>
    private static bool IsInsideSwitchSectionStatements(SwitchSectionSyntax switchSection, int spanStart)
    {
        foreach (var statement in switchSection.Statements)
        {
            if (spanStart >= statement.SpanStart && spanStart < statement.Span.End)
            {
                return true;
            }
        }

        return false;
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