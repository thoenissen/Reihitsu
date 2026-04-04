using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.Base;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// Providing fixes for <see cref="RH0311GotoStatementsShouldBePrecededByABlankLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0311GotoStatementsShouldBePrecededByABlankLineCodeFixProvider))]
public class RH0311GotoStatementsShouldBePrecededByABlankLineCodeFixProvider : StatementShouldBePrecededByABlankLineCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0311GotoStatementsShouldBePrecededByABlankLineCodeFixProvider()
        : base(RH0311GotoStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, CodeFixResources.RH0311Title)
    {
    }

    #endregion // Constructor
}