using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;

namespace Reihitsu.Formatter.Pipeline.StructuralTransforms;

/// <summary>
/// Rewrites eligible empty type declarations to semicolon declarations
/// </summary>
internal sealed class EmptyTypeDeclarationSemicolonTransform : CSharpSyntaxRewriter
{
    #region Fields

    /// <summary>
    /// Cancellation token
    /// </summary>
    private readonly CancellationToken _cancellationToken;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    public EmptyTypeDeclarationSemicolonTransform(CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether the syntax tree uses at least the requested language version
    /// </summary>
    /// <param name="typeDeclaration">Type declaration</param>
    /// <param name="minimumLanguageVersion">Minimum required language version</param>
    /// <returns><see langword="true"/> if the declaration is in a supported language version; otherwise, <see langword="false"/></returns>
    private static bool SupportsLanguageVersion(TypeDeclarationSyntax typeDeclaration, LanguageVersion minimumLanguageVersion)
    {
        return typeDeclaration.SyntaxTree?.Options is not CSharpParseOptions parseOptions
               || parseOptions.LanguageVersion >= minimumLanguageVersion;
    }

    /// <summary>
    /// Determines whether the declaration can be rewritten safely
    /// </summary>
    /// <param name="typeDeclaration">Type declaration</param>
    /// <param name="minimumLanguageVersion">Minimum required language version</param>
    /// <returns><see langword="true"/> if the declaration can be rewritten; otherwise, <see langword="false"/></returns>
    private static bool CanRewrite(TypeDeclarationSyntax typeDeclaration, LanguageVersion minimumLanguageVersion)
    {
        if (typeDeclaration.OpenBraceToken.IsMissing
            || typeDeclaration.CloseBraceToken.IsMissing
            || typeDeclaration.SemicolonToken.IsKind(SyntaxKind.SemicolonToken)
            || typeDeclaration.Members.Count != 0)
        {
            return false;
        }

        return SupportsLanguageVersion(typeDeclaration, minimumLanguageVersion)
               && EmptyTypeDeclarationSemicolonAnalysisUtilities.HasMeaningfulBodyTrivia(typeDeclaration) == false;
    }

    /// <summary>
    /// Creates a semicolon token that preserves the trailing trivia from the removed close brace
    /// </summary>
    /// <param name="closeBraceToken">Close brace token</param>
    /// <returns>The replacement semicolon token</returns>
    private static SyntaxToken CreateSemicolonToken(SyntaxToken closeBraceToken)
    {
        return SyntaxFactory.Token(SyntaxKind.SemicolonToken)
                            .WithLeadingTrivia(SyntaxFactory.TriviaList())
                            .WithTrailingTrivia(closeBraceToken.TrailingTrivia);
    }

    /// <summary>
    /// Removes trailing whitespace and end-of-line trivia from a token
    /// </summary>
    /// <param name="token">Token to clean</param>
    /// <returns>The cleaned token</returns>
    private static SyntaxToken StripTrailingFormattingTrivia(SyntaxToken token)
    {
        return token.WithTrailingTrivia(token.TrailingTrivia.Where(static trivia => trivia.IsKind(SyntaxKind.WhitespaceTrivia) == false
                                                                                    && trivia.IsKind(SyntaxKind.EndOfLineTrivia) == false));
    }

    /// <summary>
    /// Converts the declaration from brace form to semicolon form
    /// </summary>
    /// <typeparam name="TDeclaration">Declaration type</typeparam>
    /// <param name="declaration">Declaration</param>
    /// <returns>The converted declaration</returns>
    private static TDeclaration ConvertToSemicolonDeclaration<TDeclaration>(TDeclaration declaration)
        where TDeclaration : TypeDeclarationSyntax
    {
        var tokenBeforeOpenBrace = declaration.OpenBraceToken.GetPreviousToken();
        var semicolonToken = CreateSemicolonToken(declaration.CloseBraceToken);
        var updatedDeclaration = declaration;

        if (tokenBeforeOpenBrace.IsKind(SyntaxKind.None) == false)
        {
            updatedDeclaration = (TDeclaration)updatedDeclaration.ReplaceToken(tokenBeforeOpenBrace, StripTrailingFormattingTrivia(tokenBeforeOpenBrace));
        }

        return (TDeclaration)updatedDeclaration.WithOpenBraceToken(default)
                                               .WithCloseBraceToken(default)
                                               .WithSemicolonToken(semicolonToken);
    }

    #endregion // Methods

    #region CSharpSyntaxVisitor

    /// <inheritdoc/>
    public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (ClassDeclarationSyntax)base.VisitClassDeclaration(node);

        if (node == null || CanRewrite(node, LanguageVersion.CSharp12) == false)
        {
            return node;
        }

        return ConvertToSemicolonDeclaration(node);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitStructDeclaration(StructDeclarationSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (StructDeclarationSyntax)base.VisitStructDeclaration(node);

        if (node == null || CanRewrite(node, LanguageVersion.CSharp12) == false)
        {
            return node;
        }

        return ConvertToSemicolonDeclaration(node);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (InterfaceDeclarationSyntax)base.VisitInterfaceDeclaration(node);

        if (node == null || CanRewrite(node, LanguageVersion.CSharp12) == false)
        {
            return node;
        }

        return ConvertToSemicolonDeclaration(node);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitRecordDeclaration(RecordDeclarationSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (RecordDeclarationSyntax)base.VisitRecordDeclaration(node);

        if (node == null)
        {
            return null;
        }

        var minimumLanguageVersion = node.IsKind(SyntaxKind.RecordStructDeclaration) ? LanguageVersion.CSharp10 : LanguageVersion.CSharp9;

        if (CanRewrite(node, minimumLanguageVersion) == false)
        {
            return node;
        }

        return ConvertToSemicolonDeclaration(node);
    }

    #endregion // CSharpSyntaxVisitor
}