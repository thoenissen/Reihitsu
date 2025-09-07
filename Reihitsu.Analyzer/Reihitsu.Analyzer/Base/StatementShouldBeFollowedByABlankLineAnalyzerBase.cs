using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Base;

/// <summary>
/// Analyzer base class for checking if a statement is followed by a blank line
/// </summary>
/// <typeparam name="TStatement">Type of the statement syntax</typeparam>
/// <typeparam name="TAnalyzer">Type of the analyzer</typeparam>
public abstract class StatementShouldBeFollowedByABlankLineAnalyzerBase<TStatement, TAnalyzer> : DiagnosticAnalyzerBase<TAnalyzer>
    where TStatement : StatementSyntax
    where TAnalyzer : DiagnosticAnalyzer
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
    internal StatementShouldBeFollowedByABlankLineAnalyzerBase(string diagnosticId, DiagnosticCategory category, string titleResourceName, string messageFormatResourceName, SyntaxKind syntaxKind)
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
    /// Get previous token
    /// </summary>
    /// <param name="statement">Statement</param>
    /// <returns>Token</returns>
    protected abstract SyntaxToken GetNextToken(TStatement statement);

    /// <summary>
    /// Check if, the statement preceded by a blank line
    /// </summary>
    /// <param name="leadingTrivia">Leading trivia of the statement</param>
    /// <returns>Is the statement preceded by a blank line?</returns>
    private static bool IsFollowedByBlankLine(IEnumerable<SyntaxTrivia> leadingTrivia)
    {
        var endOfLineCount = 0;

        foreach (var trivia in leadingTrivia)
        {
            if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                if (++endOfLineCount >= 2)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Analyze try statement
    /// </summary>
    /// <param name="context">Context</param>
    private void OnStatement(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is TStatement statement)
        {
            var nextToken = GetNextToken(statement);

            if (nextToken.IsKind(SyntaxKind.CloseBraceToken) == false
                && nextToken.IsKind(SyntaxKind.None) == false)
            {
                var trivia = statement.GetTrailingTrivia().Concat(nextToken.LeadingTrivia);

                if (IsFollowedByBlankLine(trivia) == false)
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