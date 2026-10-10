using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Layout;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Providing fixes for <see cref="RH5007ForStatementsShouldBePrecededByABlankLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5007ForStatementsShouldBePrecededByABlankLineCodeFixProvider))]
public class RH5007ForStatementsShouldBePrecededByABlankLineCodeFixProvider : StatementShouldBePrecededByABlankLineCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5007ForStatementsShouldBePrecededByABlankLineCodeFixProvider()
        : base(RH5007ForStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, CodeFixResources.RH5007Title)
    {
    }

    #endregion // Constructor
}