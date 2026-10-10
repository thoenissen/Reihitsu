using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Layout;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Providing fixes for <see cref="RH5014SwitchStatementsShouldBePrecededByABlankLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5014SwitchStatementsShouldBePrecededByABlankLineCodeFixProvider))]
public class RH5014SwitchStatementsShouldBePrecededByABlankLineCodeFixProvider : StatementShouldBePrecededByABlankLineCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5014SwitchStatementsShouldBePrecededByABlankLineCodeFixProvider()
        : base(RH5014SwitchStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, CodeFixResources.RH5014Title)
    {
    }

    #endregion // Constructor
}