using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Core;

/// <summary>
/// Helper methods for empty type declarations that may use semicolon syntax
/// </summary>
public static class EmptyTypeDeclarationSemicolonAnalysisUtilities
{
    #region Methods

    /// <summary>
    /// Determines whether the declaration should report a diagnostic for the requested syntax kind
    /// </summary>
    /// <param name="typeDeclaration">Type declaration</param>
    /// <param name="declarationKind">Expected declaration kind</param>
    /// <param name="minimumLanguageVersion">Minimum supported language version</param>
    /// <returns><see langword="true"/> if the declaration should report a diagnostic; otherwise, <see langword="false"/></returns>
    public static bool ShouldReport(TypeDeclarationSyntax typeDeclaration, SyntaxKind declarationKind, LanguageVersion minimumLanguageVersion)
    {
        if (typeDeclaration.IsKind(declarationKind) == false)
        {
            return false;
        }

        if (typeDeclaration.SemicolonToken.IsKind(SyntaxKind.SemicolonToken))
        {
            return false;
        }

        if (typeDeclaration.OpenBraceToken.IsMissing || typeDeclaration.CloseBraceToken.IsMissing)
        {
            return false;
        }

        if (typeDeclaration.Members.Count != 0)
        {
            return false;
        }

        if (typeDeclaration.SyntaxTree.Options is not CSharpParseOptions parseOptions)
        {
            return false;
        }

        return parseOptions.LanguageVersion >= minimumLanguageVersion;
    }

    /// <summary>
    /// Determines whether the declaration can be converted safely to a semicolon declaration
    /// </summary>
    /// <param name="typeDeclaration">Type declaration</param>
    /// <param name="declarationKind">Expected declaration kind</param>
    /// <param name="minimumLanguageVersion">Minimum supported language version</param>
    /// <returns><see langword="true"/> if the declaration can be converted safely; otherwise, <see langword="false"/></returns>
    public static bool CanConvertSafely(TypeDeclarationSyntax typeDeclaration, SyntaxKind declarationKind, LanguageVersion minimumLanguageVersion)
    {
        return ShouldReport(typeDeclaration, declarationKind, minimumLanguageVersion)
               && HasMeaningfulBodyTrivia(typeDeclaration) == false;
    }

    /// <summary>
    /// Determines whether the declaration body contains comments, directives, or other meaningful trivia
    /// </summary>
    /// <param name="typeDeclaration">Type declaration</param>
    /// <returns><see langword="true"/> if meaningful trivia exists inside the body; otherwise, <see langword="false"/></returns>
    public static bool HasMeaningfulBodyTrivia(TypeDeclarationSyntax typeDeclaration)
    {
        return ContainsNonFormattingTrivia(typeDeclaration.OpenBraceToken.TrailingTrivia)
               || ContainsNonFormattingTrivia(typeDeclaration.CloseBraceToken.LeadingTrivia);
    }

    /// <summary>
    /// Determines whether the trivia list contains anything other than whitespace or end-of-line trivia
    /// </summary>
    /// <param name="triviaList">Trivia list</param>
    /// <returns><see langword="true"/> if the trivia list contains meaningful content; otherwise, <see langword="false"/></returns>
    private static bool ContainsNonFormattingTrivia(SyntaxTriviaList triviaList)
    {
        foreach (var trivia in triviaList)
        {
            if (trivia.IsKind(SyntaxKind.WhitespaceTrivia) == false
                && trivia.IsKind(SyntaxKind.EndOfLineTrivia) == false)
            {
                return true;
            }
        }

        return false;
    }

    #endregion // Methods
}