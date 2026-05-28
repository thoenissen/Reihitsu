using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Layout;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Providing fixes for <see cref="RH5010BreakStatementsShouldBePrecededByABlankLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5010BreakStatementsShouldBePrecededByABlankLineCodeFixProvider))]
public class RH5010BreakStatementsShouldBePrecededByABlankLineCodeFixProvider : StatementShouldBePrecededByABlankLineCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5010BreakStatementsShouldBePrecededByABlankLineCodeFixProvider()
        : base(RH5010BreakStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, CodeFixResources.RH5010Title)
    {
    }

    #endregion // Constructor
}