using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.LineBreaks.Utilities;

namespace Reihitsu.Formatter.Pipeline.LineBreaks.Rewriter;

/// <summary>
/// Applies Allman brace placement for type and namespace declarations and the accessor lists of
/// indexer declarations — including the accessor-list layout that indexers share with properties —
/// collapses parameter-list openers onto the declaration line, and places constructor initializers
/// on their own line. The braces of member bodies and event accessor lists are placed by
/// <see cref="LineBreakBlockRewriter"/>
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

    /// <summary>
    /// The accessor-list layout
    /// </summary>
    private readonly AccessorListLayout _accessorListLayout;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context">The formatting context</param>
    /// <param name="bracePlacer">The brace placer</param>
    /// <param name="accessorListLayout">The accessor-list layout</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public DeclarationBraceLineBreakRewriter(FormattingContext context,
                                             BracePlacer bracePlacer,
                                             AccessorListLayout accessorListLayout,
                                             CancellationToken cancellationToken)
    {
        _context = context;
        _cancellationToken = cancellationToken;
        _bracePlacer = bracePlacer;
        _accessorListLayout = accessorListLayout;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether a type declaration is a class, struct, interface, record, or union declaration whose opening
    /// brace is not missing
    /// </summary>
    /// <param name="node">The type declaration</param>
    /// <returns><see langword="true"/> if the declaration's braces are normalized</returns>
    private static bool IsBracedTypeDeclaration(BaseTypeDeclarationSyntax node)
    {
        return node is ClassDeclarationSyntax or StructDeclarationSyntax or InterfaceDeclarationSyntax or RecordDeclarationSyntax or UnionDeclarationSyntax
               && node.OpenBraceToken.IsMissing == false;
    }

    /// <summary>
    /// Places an opening and closing brace on their own lines and keeps the first content token
    /// correct
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
    /// Normalizes a constructor declaration's parameter-list opener and initializer
    /// </summary>
    /// <param name="node">The constructor declaration node</param>
    /// <returns>The updated constructor declaration</returns>
    private ConstructorDeclarationSyntax NormalizeConstructor(ConstructorDeclarationSyntax node)
    {
        node = LineBreakListRewriter.CollapseOwnedOpenParenToDeclarationLine(node, node.ParameterList);

        if (node.Initializer != null)
        {
            node = EnsureConstructorInitializerOnNewLine(node);
        }

        return node;
    }

    /// <summary>
    /// Normalizes the braces and the parameter-list opener of a declaration, and returns any other node unchanged, also a
    /// type declaration whose opening brace is missing
    /// </summary>
    /// <param name="node">The visited node</param>
    /// <returns>The updated node</returns>
    private SyntaxNode NormalizeDeclaration(SyntaxNode node)
    {
        return node switch
               {
                   BaseTypeDeclarationSyntax typeDeclaration when IsBracedTypeDeclaration(typeDeclaration) => NormalizeTypeBraces(typeDeclaration),
                   EnumDeclarationSyntax enumDeclaration => NormalizeTypeBraces(enumDeclaration),
                   NamespaceDeclarationSyntax namespaceDeclaration => NormalizeBraces(namespaceDeclaration,
                                                                                      static declaration => declaration.OpenBraceToken,
                                                                                      static (declaration, token) => declaration.WithOpenBraceToken(token),
                                                                                      static declaration => declaration.CloseBraceToken,
                                                                                      static (declaration, token) => declaration.WithCloseBraceToken(token)),
                   ConstructorDeclarationSyntax constructor => NormalizeConstructor(constructor),
                   MethodDeclarationSyntax method => LineBreakListRewriter.CollapseOwnedOpenParenToDeclarationLine(method, method.ParameterList),
                   ConversionOperatorDeclarationSyntax conversionOperator => LineBreakListRewriter.CollapseOwnedOpenParenToDeclarationLine(conversionOperator, conversionOperator.ParameterList),
                   DestructorDeclarationSyntax destructor => LineBreakListRewriter.CollapseOwnedOpenParenToDeclarationLine(destructor, destructor.ParameterList),
                   IndexerDeclarationSyntax indexer => _accessorListLayout.Apply(LineBreakListRewriter.CollapseOwnedOpenParenToDeclarationLine(indexer, indexer.ParameterList)),
                   LocalFunctionStatementSyntax localFunction => LineBreakListRewriter.CollapseOwnedOpenParenToDeclarationLine(localFunction, localFunction.ParameterList),
                   DelegateDeclarationSyntax delegateDeclaration => LineBreakListRewriter.CollapseOwnedOpenParenToDeclarationLine(delegateDeclaration, delegateDeclaration.ParameterList),
                   _ => node
               };
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

        return NormalizeDeclaration(visited);
    }

    #endregion // CSharpSyntaxVisitor
}