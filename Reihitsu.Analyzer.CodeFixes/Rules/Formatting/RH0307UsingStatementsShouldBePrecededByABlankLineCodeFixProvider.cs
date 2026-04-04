using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.Base;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// Providing fixes for <see cref="RH0307UsingStatementsShouldBePrecededByABlankLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0307UsingStatementsShouldBePrecededByABlankLineCodeFixProvider))]
public class RH0307UsingStatementsShouldBePrecededByABlankLineCodeFixProvider : StatementShouldBePrecededByABlankLineCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0307UsingStatementsShouldBePrecededByABlankLineCodeFixProvider()
        : base(RH0307UsingStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, CodeFixResources.RH0307Title)
    {
    }

    #endregion // Constructor
}