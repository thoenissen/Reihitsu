using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.Base;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// Providing fixes for <see cref="RH0305WhileStatementsShouldBePrecededByABlankLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0305WhileStatementsShouldBePrecededByABlankLineCodeFixProvider))]
public class RH0305WhileStatementsShouldBePrecededByABlankLineCodeFixProvider : StatementShouldBePrecededByABlankLineCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0305WhileStatementsShouldBePrecededByABlankLineCodeFixProvider()
        : base(RH0305WhileStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, CodeFixResources.RH0305Title)
    {
    }

    #endregion // Constructor
}
