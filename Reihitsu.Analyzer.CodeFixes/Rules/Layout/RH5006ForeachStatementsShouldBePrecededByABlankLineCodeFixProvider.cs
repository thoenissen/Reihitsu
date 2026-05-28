using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Layout;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Providing fixes for <see cref="RH5006ForeachStatementsShouldBePrecededByABlankLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5006ForeachStatementsShouldBePrecededByABlankLineCodeFixProvider))]
public class RH5006ForeachStatementsShouldBePrecededByABlankLineCodeFixProvider : StatementShouldBePrecededByABlankLineCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5006ForeachStatementsShouldBePrecededByABlankLineCodeFixProvider()
        : base(RH5006ForeachStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, CodeFixResources.RH5006Title)
    {
    }

    #endregion // Constructor
}