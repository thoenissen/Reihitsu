using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.Base;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// Providing fixes for <see cref="RH0309ForStatementsShouldBePrecededByABlankLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0309ForStatementsShouldBePrecededByABlankLineCodeFixProvider))]
public class RH0309ForStatementsShouldBePrecededByABlankLineCodeFixProvider : StatementShouldBePrecededByABlankLineCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0309ForStatementsShouldBePrecededByABlankLineCodeFixProvider()
        : base(RH0309ForStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, CodeFixResources.RH0309Title)
    {
    }

    #endregion // Constructor
}
