using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5034: Comments must not be placed between a documentation comment and its declaration
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5034CommentsMustNotBePlacedBetweenDocumentationAndDeclarationAnalyzer : CommentPositionAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5034";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5034CommentsMustNotBePlacedBetweenDocumentationAndDeclarationAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH5034Title), nameof(AnalyzerResources.RH5034MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Gets the documentable member declaration that starts with the given token
    /// </summary>
    /// <param name="token">Token</param>
    /// <returns>The member declaration, or <see langword="null"/> if the token does not start one</returns>
    private static MemberDeclarationSyntax GetDeclarationStartingWith(SyntaxToken token)
    {
        for (var node = token.Parent; node != null; node = node.Parent)
        {
            if (node.GetFirstToken() != token)
            {
                return null;
            }

            if (node is MemberDeclarationSyntax declaration)
            {
                return declaration is BaseNamespaceDeclarationSyntax or GlobalStatementSyntax or IncompleteMemberSyntax
                           ? null
                           : declaration;
            }
        }

        return null;
    }

    #endregion // Methods

    #region CommentPositionAnalyzerBase

    /// <inheritdoc/>
    protected override bool IsMisplaced(SyntaxTrivia comment, SyntaxToken previousToken, SyntaxToken nextToken)
    {
        if (comment.Token != nextToken
            || GetDeclarationStartingWith(nextToken) == null)
        {
            return false;
        }

        var leadingTrivia = nextToken.LeadingTrivia;
        var commentIndex = leadingTrivia.IndexOf(comment);

        for (var triviaIndex = 0; triviaIndex < commentIndex; triviaIndex++)
        {
            if (CommentPositionUtilities.IsDocumentationComment(leadingTrivia[triviaIndex]))
            {
                return true;
            }
        }

        return false;
    }

    #endregion // CommentPositionAnalyzerBase
}