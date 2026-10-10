using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Layout;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Providing fixes for <see cref="RH5009GotoStatementsShouldBePrecededByABlankLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5009GotoStatementsShouldBePrecededByABlankLineCodeFixProvider))]
public class RH5009GotoStatementsShouldBePrecededByABlankLineCodeFixProvider : StatementShouldBePrecededByABlankLineCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5009GotoStatementsShouldBePrecededByABlankLineCodeFixProvider()
        : base(RH5009GotoStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, CodeFixResources.RH5009Title)
    {
    }

    #endregion // Constructor
}