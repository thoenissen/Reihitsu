using Microsoft.CodeAnalysis;
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

    #region CommentPositionAnalyzerBase

    /// <inheritdoc/>
    protected override bool IsMisplaced(SyntaxTrivia comment, SyntaxToken previousToken, SyntaxToken nextToken)
    {
        if (comment.Token != nextToken)
        {
            return false;
        }

        var leadingTrivia = nextToken.LeadingTrivia;
        var commentIndex = leadingTrivia.IndexOf(comment);

        for (var triviaIndex = 0; triviaIndex < commentIndex; triviaIndex++)
        {
            if (CommentPositionUtilities.IsDocumentingComment(leadingTrivia[triviaIndex]))
            {
                return true;
            }
        }

        return false;
    }

    #endregion // CommentPositionAnalyzerBase
}