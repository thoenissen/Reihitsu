using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5036: Comments must not be placed between a statement header and its body
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5036CommentsMustNotBePlacedBetweenStatementHeaderAndBodyAnalyzer : CommentPositionAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5036";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5036CommentsMustNotBePlacedBetweenStatementHeaderAndBodyAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH5036Title), nameof(AnalyzerResources.RH5036MessageFormat))
    {
    }

    #endregion // Constructor

    #region CommentPositionAnalyzerBase

    /// <inheritdoc/>
    protected override bool IsMisplaced(SyntaxTrivia comment, SyntaxToken previousToken, SyntaxToken nextToken)
    {
        return previousToken.Parent != null
               && CommentPositionUtilities.TryGetStatementHeader(previousToken.Parent, out _, out var closeParenToken, out var body)
               && closeParenToken == previousToken
               && body.GetFirstToken() == nextToken;
    }

    #endregion // CommentPositionAnalyzerBase
}