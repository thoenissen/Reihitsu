using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5039: Comments must not be placed inside a statement header
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5039CommentsMustNotBePlacedInsideStatementHeaderAnalyzer : CommentRegionAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5039";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5039CommentsMustNotBePlacedInsideStatementHeaderAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH5039Title), nameof(AnalyzerResources.RH5039MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Gets the first token of a statement header, which is the <see langword="await"/> keyword when present
    /// </summary>
    /// <param name="awaitKeyword">Optional <see langword="await"/> keyword</param>
    /// <param name="keyword">Statement keyword</param>
    /// <returns>The first token of the header</returns>
    private static SyntaxToken GetHeaderStart(SyntaxToken awaitKeyword, SyntaxToken keyword)
    {
        return awaitKeyword.IsKind(SyntaxKind.None)
                   ? keyword
                   : awaitKeyword;
    }

    #endregion // Methods

    #region CommentRegionAnalyzerBase

    /// <inheritdoc/>
    protected override bool TryGetRegion(SyntaxNode node, out SyntaxToken openToken, out SyntaxToken closeToken)
    {
        (openToken, closeToken) = node switch
                                  {
                                      IfStatementSyntax statement => (statement.IfKeyword, statement.CloseParenToken),
                                      WhileStatementSyntax statement => (statement.WhileKeyword, statement.CloseParenToken),
                                      ForStatementSyntax statement => (statement.ForKeyword, statement.CloseParenToken),
                                      CommonForEachStatementSyntax statement => (GetHeaderStart(statement.AwaitKeyword, statement.ForEachKeyword), statement.CloseParenToken),
                                      UsingStatementSyntax statement => (GetHeaderStart(statement.AwaitKeyword, statement.UsingKeyword), statement.CloseParenToken),
                                      LockStatementSyntax statement => (statement.LockKeyword, statement.CloseParenToken),
                                      FixedStatementSyntax statement => (statement.FixedKeyword, statement.CloseParenToken),
                                      _ => (default, default)
                                  };

        return openToken.IsKind(SyntaxKind.None) == false;
    }

    #endregion // CommentRegionAnalyzerBase
}