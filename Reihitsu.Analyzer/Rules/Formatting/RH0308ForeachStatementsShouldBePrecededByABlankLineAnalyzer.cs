using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0308: The foreach-Statement should be preceded by a blank line
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0308ForeachStatementsShouldBePrecededByABlankLineAnalyzer : StatementShouldBePrecededByABlankLineAnalyzerBase<ForEachStatementSyntax, RH0308ForeachStatementsShouldBePrecededByABlankLineAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0308";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0308ForeachStatementsShouldBePrecededByABlankLineAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0308Title), nameof(AnalyzerResources.RH0308MessageFormat), SyntaxKind.ForEachStatement)
    {
    }

    #endregion // Constructor

    #region StatementShouldBePrecededByABlankLineAnalyzerBase

    /// <inheritdoc />
    protected override SyntaxToken GetPreviousToken(ForEachStatementSyntax foreachStatement)
    {
        return foreachStatement.AwaitKeyword == default
                   ? foreachStatement.ForEachKeyword.GetPreviousToken()
                   : foreachStatement.AwaitKeyword.GetPreviousToken();
    }

    /// <inheritdoc />
    protected override Location GetLocation(ForEachStatementSyntax statement)
    {
        return statement.ForEachKeyword.GetLocation();
    }

    #endregion // StatementShouldBePrecededByABlankLineAnalyzerBase
}