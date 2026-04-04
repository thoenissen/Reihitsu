using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.Base;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// Providing fixes for <see cref="RH0319FixedStatementsShouldBePrecededByABlankLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0319FixedStatementsShouldBePrecededByABlankLineCodeFixProvider))]
public class RH0319FixedStatementsShouldBePrecededByABlankLineCodeFixProvider : StatementShouldBePrecededByABlankLineCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0319FixedStatementsShouldBePrecededByABlankLineCodeFixProvider()
        : base(RH0319FixedStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, CodeFixResources.RH0319Title)
    {
    }

    #endregion // Constructor
}