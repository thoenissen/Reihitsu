using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.Base;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// Providing fixes for <see cref="RH0304IfStatementsShouldBePrecededByABlankLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0304IfStatementsShouldBePrecededByABlankLineCodeFixProvider))]
public class RH0304IfStatementsShouldBePrecededByABlankLineCodeFixProvider : StatementShouldBePrecededByABlankLineCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0304IfStatementsShouldBePrecededByABlankLineCodeFixProvider()
        : base(RH0304IfStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, CodeFixResources.RH0304Title)
    {
    }

    #endregion // Constructor
}
