using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Analyzer.Core;

/// <summary>
/// The one owner of which members the rules that judge a member's body judge, which body each of them has, and which
/// token names it (RH2008-002, RH2008-004, RH2008-005): a method, constructor, finalizer, operator, conversion operator,
/// local function, or accessor with a block body or an expression body, and a property or indexer with an expression
/// body. A member without a body, and a property or indexer with an accessor list, is not judged
/// </summary>
internal static class MemberBodyUtilities
{
    #region Properties

    /// <summary>
    /// Syntax kinds of the members whose body is judged
    /// </summary>
    internal static ImmutableArray<SyntaxKind> MemberKinds { get; } = [
                                                                          SyntaxKind.MethodDeclaration,
                                                                          SyntaxKind.ConstructorDeclaration,
                                                                          SyntaxKind.DestructorDeclaration,
                                                                          SyntaxKind.OperatorDeclaration,
                                                                          SyntaxKind.ConversionOperatorDeclaration,
                                                                          SyntaxKind.LocalFunctionStatement,
                                                                          SyntaxKind.GetAccessorDeclaration,
                                                                          SyntaxKind.SetAccessorDeclaration,
                                                                          SyntaxKind.InitAccessorDeclaration,
                                                                          SyntaxKind.AddAccessorDeclaration,
                                                                          SyntaxKind.RemoveAccessorDeclaration,
                                                                          SyntaxKind.PropertyDeclaration,
                                                                          SyntaxKind.IndexerDeclaration
                                                                      ];

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Tries to get the body of a judged member and the token that names it
    /// </summary>
    /// <param name="node">Member of one of the <see cref="MemberKinds"/></param>
    /// <param name="body">Block body, or <see langword="null"/></param>
    /// <param name="expressionBody">Expression body, or <see langword="null"/></param>
    /// <param name="nameToken">Token that names the member: its identifier, or the keyword or operator token of a member without one</param>
    /// <returns><see langword="true"/> when the member has a block body or an expression body; otherwise <see langword="false"/></returns>
    internal static bool TryGetBody(SyntaxNode node, out BlockSyntax body, out ArrowExpressionClauseSyntax expressionBody, out SyntaxToken nameToken)
    {
        (body, expressionBody, nameToken) = node switch
                                            {
                                                MethodDeclarationSyntax method => (method.Body, method.ExpressionBody, method.Identifier),
                                                ConstructorDeclarationSyntax constructor => (constructor.Body, constructor.ExpressionBody, constructor.Identifier),
                                                DestructorDeclarationSyntax destructor => (destructor.Body, destructor.ExpressionBody, destructor.Identifier),
                                                OperatorDeclarationSyntax operatorDeclaration => (operatorDeclaration.Body, operatorDeclaration.ExpressionBody, operatorDeclaration.OperatorToken),
                                                ConversionOperatorDeclarationSyntax conversion => (conversion.Body, conversion.ExpressionBody, conversion.OperatorKeyword),
                                                LocalFunctionStatementSyntax localFunction => (localFunction.Body, localFunction.ExpressionBody, localFunction.Identifier),
                                                AccessorDeclarationSyntax accessor => (accessor.Body, accessor.ExpressionBody, accessor.Keyword),
                                                PropertyDeclarationSyntax property => (null, property.ExpressionBody, property.Identifier),
                                                IndexerDeclarationSyntax indexer => (null, indexer.ExpressionBody, indexer.ThisKeyword),
                                                _ => ((BlockSyntax)null, (ArrowExpressionClauseSyntax)null, default(SyntaxToken))
                                            };

        return body != null
               || expressionBody != null;
    }

    #endregion // Methods
}