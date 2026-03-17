using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.Base;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// Providing fixes for <see cref="RH0308ForeachStatementsShouldBePrecededByABlankLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0308ForeachStatementsShouldBePrecededByABlankLineCodeFixProvider))]
public class RH0308ForeachStatementsShouldBePrecededByABlankLineCodeFixProvider : StatementShouldBePrecededByABlankLineCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0308ForeachStatementsShouldBePrecededByABlankLineCodeFixProvider()
        : base(RH0308ForeachStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, CodeFixResources.RH0308Title)
    {
    }

    #endregion // Constructor
}
