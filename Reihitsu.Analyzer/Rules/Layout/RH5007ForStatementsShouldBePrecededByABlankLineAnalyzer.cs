using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5007: The for-Statement should be preceded by a blank line
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5007ForStatementsShouldBePrecededByABlankLineAnalyzer : StatementShouldBePrecededByABlankLineAnalyzerBase<ForStatementSyntax>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5007";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5007ForStatementsShouldBePrecededByABlankLineAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Layout, nameof(AnalyzerResources.RH5007Title), nameof(AnalyzerResources.RH5007MessageFormat), SyntaxKind.ForStatement)
    {
    }

    #endregion // Constructor

    #region StatementShouldBePrecededByABlankLineAnalyzerBase

    /// <inheritdoc />
    protected override SyntaxToken GetPreviousToken(ForStatementSyntax forStatement)
    {
        return forStatement.ForKeyword.GetPreviousToken();
    }

    /// <inheritdoc />
    protected override Location GetLocation(ForStatementSyntax statement)
    {
        return statement.ForKeyword.GetLocation();
    }

    #endregion // StatementShouldBePrecededByABlankLineAnalyzerBase
}