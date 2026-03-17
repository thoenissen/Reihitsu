using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.Base;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// Providing fixes for <see cref="RH0306DoStatementsShouldBePrecededByABlankLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0306DoStatementsShouldBePrecededByABlankLineCodeFixProvider))]
public class RH0306DoStatementsShouldBePrecededByABlankLineCodeFixProvider : StatementShouldBePrecededByABlankLineCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0306DoStatementsShouldBePrecededByABlankLineCodeFixProvider()
        : base(RH0306DoStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, CodeFixResources.RH0306Title)
    {
    }

    #endregion // Constructor
}
