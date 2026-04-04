using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.Base;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// Providing fixes for <see cref="RH0316SwitchStatementsShouldBePrecededByABlankLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0316SwitchStatementsShouldBePrecededByABlankLineCodeFixProvider))]
public class RH0316SwitchStatementsShouldBePrecededByABlankLineCodeFixProvider : StatementShouldBePrecededByABlankLineCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0316SwitchStatementsShouldBePrecededByABlankLineCodeFixProvider()
        : base(RH0316SwitchStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, CodeFixResources.RH0316Title)
    {
    }

    #endregion // Constructor
}