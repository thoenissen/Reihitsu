using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.Base;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// Providing fixes for <see cref="RH0312BreakStatementsShouldBePrecededByABlankLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0312BreakStatementsShouldBePrecededByABlankLineCodeFixProvider))]
public class RH0312BreakStatementsShouldBePrecededByABlankLineCodeFixProvider : StatementShouldBePrecededByABlankLineCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0312BreakStatementsShouldBePrecededByABlankLineCodeFixProvider()
        : base(RH0312BreakStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, CodeFixResources.RH0312Title)
    {
    }

    #endregion // Constructor
}