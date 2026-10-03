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
        var body = previousToken.Parent switch
                   {
                       IfStatementSyntax statement when statement.CloseParenToken == previousToken => statement.Statement,
                       WhileStatementSyntax statement when statement.CloseParenToken == previousToken => statement.Statement,
                       ForStatementSyntax statement when statement.CloseParenToken == previousToken => statement.Statement,
                       CommonForEachStatementSyntax statement when statement.CloseParenToken == previousToken => statement.Statement,
                       UsingStatementSyntax statement when statement.CloseParenToken == previousToken => statement.Statement,
                       LockStatementSyntax statement when statement.CloseParenToken == previousToken => statement.Statement,
                       FixedStatementSyntax statement when statement.CloseParenToken == previousToken => statement.Statement,
                       _ => null
                   };

        return body != null
               && body.GetFirstToken() == nextToken;
    }

    #endregion // CommentPositionAnalyzerBase
}