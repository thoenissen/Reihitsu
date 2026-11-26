using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;
using Reihitsu.Formatter.Data;

namespace Reihitsu.Formatter.Pipeline.StructuralTransforms.Rewriter;

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

    /// <summary>
    /// The language version the source targets, taken from the formatting context because a node that an earlier
    /// rewriter replaced belongs to a new tree whose parse options fall back to the defaults
    /// </summary>
    private readonly LanguageVersion _languageVersion;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context">The formatting context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public EmptyTypeDeclarationSemicolonTransform(FormattingContext context, CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
        _languageVersion = context.LanguageVersion;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Creates the declaration's one semicolon. It keeps the trailing trivia of the semicolon that followed the
    /// removed close brace, or of the close brace when no semicolon followed it; the whitespace and line breaks
    /// between the close brace and a following semicolon go with them (FMT-STRUCT-017)
    /// </summary>
    /// <param name="declaration">Declaration whose braces are removed</param>
    /// <returns>The replacement semicolon token</returns>
    private static SyntaxToken CreateSemicolonToken(TypeDeclarationSyntax declaration)
    {
        var trailingTrivia = declaration.SemicolonToken.IsKind(SyntaxKind.SemicolonToken)
                                 ? declaration.SemicolonToken.TrailingTrivia
                                 : declaration.CloseBraceToken.TrailingTrivia;

        // Built with explicit trivia, so the created token carries no elastic trivia (FMT-STRUCT-026)
        return SyntaxFactory.Token(default, SyntaxKind.SemicolonToken, trailingTrivia);
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
    /// Converts the declaration from brace form to semicolon form, replacing a semicolon that followed the braces
    /// </summary>
    /// <typeparam name="TDeclaration">Declaration type</typeparam>
    /// <param name="declaration">Declaration</param>
    /// <returns>The converted declaration</returns>
    private static TDeclaration ConvertToSemicolonDeclaration<TDeclaration>(TDeclaration declaration)
        where TDeclaration : TypeDeclarationSyntax
    {
        var tokenBeforeOpenBrace = SyntaxTokenNavigationUtilities.GetPreviousToken(declaration.OpenBraceToken);
        var semicolonToken = CreateSemicolonToken(declaration);
        var updatedDeclaration = declaration;

        if (tokenBeforeOpenBrace.IsKind(SyntaxKind.None) == false)
        {
            updatedDeclaration = updatedDeclaration.ReplaceToken(tokenBeforeOpenBrace, StripTrailingFormattingTrivia(tokenBeforeOpenBrace));
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

        if (node == null || EmptyTypeDeclarationSemicolonAnalysisUtilities.CanConvertSafely(node, node.Kind(), LanguageVersion.CSharp12, _languageVersion) == false)
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

        if (node == null || EmptyTypeDeclarationSemicolonAnalysisUtilities.CanConvertSafely(node, node.Kind(), LanguageVersion.CSharp12, _languageVersion) == false)
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

        if (node == null || EmptyTypeDeclarationSemicolonAnalysisUtilities.CanConvertSafely(node, node.Kind(), LanguageVersion.CSharp12, _languageVersion) == false)
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

        if (EmptyTypeDeclarationSemicolonAnalysisUtilities.CanConvertSafely(node, node.Kind(), minimumLanguageVersion, _languageVersion) == false)
        {
            return node;
        }

        return ConvertToSemicolonDeclaration(node);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitUnionDeclaration(UnionDeclarationSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (UnionDeclarationSyntax)base.VisitUnionDeclaration(node);

        if (node == null || EmptyTypeDeclarationSemicolonAnalysisUtilities.CanConvertSafely(node, node.Kind(), LanguageVersion.CSharp15, _languageVersion) == false)
        {
            return node;
        }

        return ConvertToSemicolonDeclaration(node);
    }

    #endregion // CSharpSyntaxVisitor
}