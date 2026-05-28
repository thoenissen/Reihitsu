using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5019: The yield-Statement should be preceded by a blank line
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5019YieldStatementsShouldBePrecededByABlankLineAnalyzer : StatementShouldBePrecededByABlankLineAnalyzerBase<YieldStatementSyntax, RH5019YieldStatementsShouldBePrecededByABlankLineAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5019";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5019YieldStatementsShouldBePrecededByABlankLineAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Layout, nameof(AnalyzerResources.RH5019Title), nameof(AnalyzerResources.RH5019MessageFormat), SyntaxKind.YieldReturnStatement)
    {
    }

    #endregion // Constructor

    #region StatementShouldBePrecededByABlankLineAnalyzerBase

    /// <inheritdoc />
    protected override bool IsRelevant(YieldStatementSyntax statement)
    {
        if (statement.Parent is BlockSyntax block)
        {
            var index = block.Statements.IndexOf(statement);

            if (index > 0)
            {
                return block.Statements[index - 1] is not YieldStatementSyntax;
            }
        }

        return true;
    }

    /// <inheritdoc />
    protected override SyntaxToken GetPreviousToken(YieldStatementSyntax yieldStatement)
    {
        return yieldStatement.YieldKeyword.GetPreviousToken();
    }

    /// <inheritdoc />
    protected override Location GetLocation(YieldStatementSyntax statement)
    {
        return statement.YieldKeyword.GetLocation();
    }

    #endregion // StatementShouldBePrecededByABlankLineAnalyzerBase
}