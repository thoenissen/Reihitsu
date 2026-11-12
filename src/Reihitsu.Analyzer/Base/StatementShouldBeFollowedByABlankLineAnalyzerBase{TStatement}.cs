using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Base;

/// <summary>
/// Analyzer base class for checking if a statement is followed by a blank line
/// </summary>
/// <typeparam name="TStatement">Type of the statement syntax</typeparam>
public abstract class StatementShouldBeFollowedByABlankLineAnalyzerBase<TStatement> : DiagnosticAnalyzerBase
    where TStatement : StatementSyntax
{
    #region Fields

    /// <summary>
    /// <see cref="SyntaxKind"/> of <typeparamref name="TStatement"/>
    /// </summary>
    private readonly SyntaxKind _syntaxKind;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="diagnosticId">The diagnostic ID</param>
    /// <param name="category">The diagnostic category</param>
    /// <param name="titleResourceName">The resource name for the title of the diagnostic</param>
    /// <param name="messageFormatResourceName">The resource name for the message format of the diagnostic</param>
    /// <param name="syntaxKind"><see cref="SyntaxKind"/> of <typeparamref name="TStatement"/></param>
    private protected StatementShouldBeFollowedByABlankLineAnalyzerBase(string diagnosticId, DiagnosticCategory category, string titleResourceName, string messageFormatResourceName, SyntaxKind syntaxKind)
        : base(diagnosticId, category, titleResourceName, messageFormatResourceName)
    {
        _syntaxKind = syntaxKind;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Get location to generate the diagnostics
    /// </summary>
    /// <param name="statement">Statement</param>
    /// <returns>Location</returns>
    protected abstract Location GetLocation(TStatement statement);

    /// <summary>
    /// Get next token
    /// </summary>
    /// <param name="statement">Statement</param>
    /// <returns>Token</returns>
    protected abstract SyntaxToken GetNextToken(TStatement statement);

    /// <summary>
    /// Determines whether a token continues a statement that holds the analyzed statement rather than starting a new one:
    /// the <c>else</c> keyword of an <c>if</c> statement or the <c>while</c> keyword of a <c>do</c> statement (RH5011-008)
    /// </summary>
    /// <param name="nextToken">Token that follows the analyzed statement</param>
    /// <returns><see langword="true"/> if the token continues the enclosing statement</returns>
    private static bool ContinuesEnclosingStatement(SyntaxToken nextToken)
    {
        return nextToken.IsKind(SyntaxKind.ElseKeyword)
               || (nextToken.IsKind(SyntaxKind.WhileKeyword)
                   && nextToken.Parent is DoStatementSyntax);
    }

    /// <summary>
    /// Analyze statement. Blank lines are counted with <see cref="TokenGapAnalysis"/>, the one measure of SHARED-BLANK-001,
    /// so a line holding only whitespace directly below a trivia that ends with its own line break - a <c>///</c> comment
    /// or a directive - counts as blank
    /// </summary>
    /// <param name="context">Context</param>
    private void OnStatement(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is TStatement statement)
        {
            var nextToken = GetNextToken(statement);

            if (nextToken.IsKind(SyntaxKind.CloseBraceToken) == false
                && nextToken.IsKind(SyntaxKind.None) == false
                && ContinuesEnclosingStatement(nextToken) == false)
            {
                var lastToken = statement.GetLastToken();
                var trivia = lastToken.TrailingTrivia.Concat(nextToken.LeadingTrivia);

                if (TokenGapAnalysis.Between(lastToken, nextToken).BlankLineCount == 0
                    && SyntaxTriviaUtilities.IsFollowedByDirective(trivia) == false)
                {
                    context.ReportDiagnostic(CreateDiagnostic(GetLocation(statement)));
                }
            }
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnStatement, _syntaxKind);
    }

    #endregion // DiagnosticAnalyzer
}