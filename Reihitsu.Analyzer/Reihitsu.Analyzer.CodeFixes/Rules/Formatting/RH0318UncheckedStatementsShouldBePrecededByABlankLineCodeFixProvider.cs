using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.Base;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// Providing fixes for <see cref="RH0318UncheckedStatementsShouldBePrecededByABlankLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0318UncheckedStatementsShouldBePrecededByABlankLineCodeFixProvider))]
public class RH0318UncheckedStatementsShouldBePrecededByABlankLineCodeFixProvider : StatementShouldBePrecededByABlankLineCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0318UncheckedStatementsShouldBePrecededByABlankLineCodeFixProvider()
        : base(RH0318UncheckedStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, CodeFixResources.RH0318Title)
    {
    }

    #endregion // Constructor
}
