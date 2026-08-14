using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.Core.Utilities;
using Reihitsu.Formatter.Pipeline.LineBreaks.Utilities;

namespace Reihitsu.Formatter.Pipeline.LineBreaks.Rewriter;

/// <summary>
/// Applies Allman brace placement for declarations, member bodies and the accessor lists of
/// indexer and event declarations, collapses parameter-list openers onto the declaration line, and
/// places constructor initializers on their own line
/// </summary>
internal sealed class DeclarationBraceLineBreakRewriter : CSharpSyntaxRewriter
{
    #region Fields

    /// <summary>
    /// The formatting context
    /// </summary>
    private readonly FormattingContext _context;

    /// <summary>
    /// The cancellation token
    /// </summary>
    private readonly CancellationToken _cancellationToken;

    /// <summary>
    /// The brace placer
    /// </summary>
    private readonly BracePlacer _bracePlacer;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context">The formatting context</param>
    /// <param name="bracePlacer">The brace placer</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public DeclarationBraceLineBreakRewriter(FormattingContext context,
                                             BracePlacer bracePlacer,
                                             CancellationToken cancellationToken)
    {
        _context = context;
        _cancellationToken = cancellationToken;
        _bracePlacer = bracePlacer;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Collapses a parameter list opener onto the same line as its declaration token
    /// </summary>
    /// <typeparam name="TNode">The syntax node type that owns the declaration</typeparam>
    /// <param name="node">The syntax node that contains the declaration token and parameter list</param>
    /// <param name="declarationToken">The declaration token that should share a line with the opening parenthesis</param>
    /// <param name="parameterList">The parameter list to normalize</param>
    /// <returns>The updated syntax node</returns>
    private static TNode CollapseParameterListToDeclarationLine<TNode>(TNode node,
                                                                       SyntaxToken declarationToken,
                                                                       ParameterListSyntax parameterList)
        where TNode : SyntaxNode
    {
        if (TokenGapUtilities.HasLineBreakBetween(declarationToken, parameterList.OpenParenToken) == false)
        {
            return node;
        }

        if (LineBreakTriviaUtilities.WouldJoinAcrossUnjoinableTrivia(declarationToken, parameterList.OpenParenToken))
        {
            return node;
        }

        var newDeclarationToken = declarationToken.WithTrailingTrivia(LineBreakTriviaUtilities.RemoveTrailingWhitespace(LineBreakTriviaUtilities.RemoveTrailingEndOfLineTrivia(declarationToken.TrailingTrivia)));
        var newOpenParen = LineBreakTriviaUtilities.RemoveLeadingEndOfLineAndWhitespace(parameterList.OpenParenToken);

        return node.ReplaceTokens([declarationToken, parameterList.OpenParenToken],
                                  (original, _) => original == declarationToken
                                                       ? newDeclarationToken
                                                       : newOpenParen);
    }

    /// <summary>
    /// Places an opening and closing brace on their own lines and keeps the first content token
    /// and the close-brace continuation correct
    /// </summary>
    /// <typeparam name="TNode">The owning syntax node type</typeparam>
    /// <param name="node">The node owning the braces</param>
    /// <param name="getOpenBrace">Selects the open brace token from the current node</param>
    /// <param name="withOpenBrace">Replaces the open brace token on the node</param>
    /// <param name="getCloseBrace">Selects the close brace token from the current node</param>
    /// <param name="withCloseBrace">Replaces the close brace token on the node</param>
    /// <returns>The node with braces placed on their own lines</returns>
    private TNode NormalizeBraces<TNode>(TNode node,
                                         Func<TNode, SyntaxToken> getOpenBrace,
                                         Func<TNode, SyntaxToken, TNode> withOpenBrace,
                                         Func<TNode, SyntaxToken> getCloseBrace,
                                         Func<TNode, SyntaxToken, TNode> withCloseBrace)
        where TNode : SyntaxNode
    {
        node = _bracePlacer.EnsureBraceOnOwnLine(node, getOpenBrace, withOpenBrace, getCloseBrace, withCloseBrace);
        node = _bracePlacer.EnsureFirstContentOnNewLine(node, getOpenBrace(node));
        node = _bracePlacer.EnsureCloseBraceContinuation(node, getCloseBrace(node));

        return node;
    }

    /// <summary>
    /// Places the braces of a type or enum declaration on their own lines
    /// </summary>
    /// <param name="node">The type or enum declaration node</param>
    /// <returns>The declaration with braces placed on their own lines</returns>
    private BaseTypeDeclarationSyntax NormalizeTypeBraces(BaseTypeDeclarationSyntax node)
    {
        return NormalizeBraces(node,
                               static typeDeclaration => typeDeclaration.OpenBraceToken,
                               static (typeDeclaration, token) => typeDeclaration.WithOpenBraceToken(token),
                               static typeDeclaration => typeDeclaration.CloseBraceToken,
                               static (typeDeclaration, token) => typeDeclaration.WithCloseBraceToken(token));
    }

    /// <summary>
    /// Ensures the constructor initializer (<c>: base()</c> or <c>: this()</c>) starts on a new line
    /// </summary>
    /// <param name="node">The constructor declaration node</param>
    /// <returns>The constructor declaration with the initializer on a new line</returns>
    private ConstructorDeclarationSyntax EnsureConstructorInitializerOnNewLine(ConstructorDeclarationSyntax node)
    {
        if (node.Initializer == null)
        {
            return node;
        }

        var colonToken = node.Initializer.ColonToken;

        if (LineBreakTriviaUtilities.HasLeadingEndOfLine(colonToken))
        {
            return node;
        }

        var newColonToken = LineBreakTriviaUtilities.PrependEndOfLine(colonToken, _context.EndOfLine);

        return node.WithInitializer(node.Initializer.WithColonToken(newColonToken));
    }

    /// <summary>
    /// Normalizes a constructor declaration's parameter-list opener, initializer, and body braces
    /// </summary>
    /// <param name="node">The constructor declaration node</param>
    /// <returns>The updated constructor declaration</returns>
    private ConstructorDeclarationSyntax NormalizeConstructor(ConstructorDeclarationSyntax node)
    {
        node = CollapseParameterListToDeclarationLine(node, node.Identifier, node.ParameterList);

        if (node.Initializer != null)
        {
            node = EnsureConstructorInitializerOnNewLine(node);
        }

        if (node.Body != null)
        {
            node = _bracePlacer.NormalizeOwnedBraces(node, static constructor => constructor.Body.OpenBraceToken, static constructor => constructor.Body.CloseBraceToken);
        }

        return node;
    }

    /// <summary>
    /// Normalizes a method declaration's parameter-list opener and body braces
    /// </summary>
    /// <param name="node">The method declaration node</param>
    /// <returns>The updated method declaration</returns>
    private MethodDeclarationSyntax NormalizeMethod(MethodDeclarationSyntax node)
    {
        node = CollapseParameterListToDeclarationLine(node, node.Identifier, node.ParameterList);

        if (node.Body != null)
        {
            node = _bracePlacer.NormalizeOwnedBraces(node, static method => method.Body.OpenBraceToken, static method => method.Body.CloseBraceToken);
        }

        return node;
    }

    /// <summary>
    /// Normalizes a conversion operator declaration's parameter-list opener and body braces
    /// </summary>
    /// <param name="node">The conversion operator declaration node</param>
    /// <returns>The updated conversion operator declaration</returns>
    private ConversionOperatorDeclarationSyntax NormalizeConversionOperator(ConversionOperatorDeclarationSyntax node)
    {
        node = CollapseParameterListToDeclarationLine(node, node.Type.GetLastToken(), node.ParameterList);

        if (node.Body != null)
        {
            node = _bracePlacer.NormalizeOwnedBraces(node, static conversionOperator => conversionOperator.Body.OpenBraceToken, static conversionOperator => conversionOperator.Body.CloseBraceToken);
        }

        return node;
    }

    /// <summary>
    /// Normalizes a destructor declaration's parameter-list opener and body braces
    /// </summary>
    /// <param name="node">The destructor declaration node</param>
    /// <returns>The updated destructor declaration</returns>
    private DestructorDeclarationSyntax NormalizeDestructor(DestructorDeclarationSyntax node)
    {
        node = CollapseParameterListToDeclarationLine(node, node.Identifier, node.ParameterList);

        if (node.Body != null)
        {
            node = _bracePlacer.NormalizeOwnedBraces(node, static destructor => destructor.Body.OpenBraceToken, static destructor => destructor.Body.CloseBraceToken);
        }

        return node;
    }

    /// <summary>
    /// Normalizes a local function statement's parameter-list opener and body braces
    /// </summary>
    /// <param name="node">The local function statement node</param>
    /// <returns>The updated local function statement</returns>
    private LocalFunctionStatementSyntax NormalizeLocalFunction(LocalFunctionStatementSyntax node)
    {
        node = CollapseParameterListToDeclarationLine(node, node.Identifier, node.ParameterList);

        if (node.Body != null)
        {
            node = _bracePlacer.NormalizeOwnedBraces(node, static localFunction => localFunction.Body.OpenBraceToken, static localFunction => localFunction.Body.CloseBraceToken);
        }

        return node;
    }

    #endregion // Methods

    #region CSharpSyntaxVisitor

    /// <inheritdoc/>
    public override SyntaxNode Visit(SyntaxNode node)
    {
        if (node == null)
        {
            return null;
        }

        _cancellationToken.ThrowIfCancellationRequested();

        var visited = base.Visit(node);

        if (visited == null)
        {
            return null;
        }

        switch (visited)
        {
            case ClassDeclarationSyntax:
            case StructDeclarationSyntax:
            case InterfaceDeclarationSyntax:
            case RecordDeclarationSyntax:
                {
                    var typeDeclaration = (BaseTypeDeclarationSyntax)visited;

                    return typeDeclaration.OpenBraceToken.IsMissing
                               ? visited
                               : NormalizeTypeBraces(typeDeclaration);
                }

            case EnumDeclarationSyntax enumDeclaration:
                {
                    return NormalizeTypeBraces(enumDeclaration);
                }

            case NamespaceDeclarationSyntax namespaceDeclaration:
                {
                    return NormalizeBraces(namespaceDeclaration,
                                           static declaration => declaration.OpenBraceToken,
                                           static (declaration, token) => declaration.WithOpenBraceToken(token),
                                           static declaration => declaration.CloseBraceToken,
                                           static (declaration, token) => declaration.WithCloseBraceToken(token));
                }

            case ConstructorDeclarationSyntax constructor:
                {
                    return NormalizeConstructor(constructor);
                }

            case MethodDeclarationSyntax method:
                {
                    return NormalizeMethod(method);
                }

            case OperatorDeclarationSyntax operatorDeclaration:
                {
                    return operatorDeclaration.Body == null
                               ? visited
                               : _bracePlacer.NormalizeOwnedBraces(operatorDeclaration, static declaration => declaration.Body.OpenBraceToken, static declaration => declaration.Body.CloseBraceToken);
                }

            case ConversionOperatorDeclarationSyntax conversionOperator:
                {
                    return NormalizeConversionOperator(conversionOperator);
                }

            case DestructorDeclarationSyntax destructor:
                {
                    return NormalizeDestructor(destructor);
                }

            case IndexerDeclarationSyntax indexer:
                {
                    return LineBreakDetection.ShouldNormalizeAccessorListBraces(indexer.AccessorList)
                               ? _bracePlacer.NormalizeOwnedBraces(indexer, static declaration => declaration.AccessorList.OpenBraceToken, static declaration => declaration.AccessorList.CloseBraceToken)
                               : visited;
                }

            case EventDeclarationSyntax eventDeclaration:
                {
                    return LineBreakDetection.ShouldNormalizeAccessorListBraces(eventDeclaration.AccessorList)
                               ? _bracePlacer.NormalizeOwnedBraces(eventDeclaration, static declaration => declaration.AccessorList.OpenBraceToken, static declaration => declaration.AccessorList.CloseBraceToken)
                               : visited;
                }

            case LocalFunctionStatementSyntax localFunction:
                {
                    return NormalizeLocalFunction(localFunction);
                }

            case DelegateDeclarationSyntax delegateDeclaration:
                {
                    return CollapseParameterListToDeclarationLine(delegateDeclaration, delegateDeclaration.Identifier, delegateDeclaration.ParameterList);
                }

            default:
                {
                    return visited;
                }
        }
    }

    #endregion // CSharpSyntaxVisitor
}