using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.Base;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// Providing fixes for <see cref="RH0310ReturnStatementsShouldBePrecededByABlankLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0310ReturnStatementsShouldBePrecededByABlankLineCodeFixProvider))]
public class RH0310ReturnStatementsShouldBePrecededByABlankLineCodeFixProvider : StatementShouldBePrecededByABlankLineCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0310ReturnStatementsShouldBePrecededByABlankLineCodeFixProvider()
        : base(RH0310ReturnStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, CodeFixResources.RH0310Title)
    {
    }

    #endregion // Constructor
}