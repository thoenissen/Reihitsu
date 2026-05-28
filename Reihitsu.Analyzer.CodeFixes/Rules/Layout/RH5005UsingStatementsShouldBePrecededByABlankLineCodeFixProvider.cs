using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Layout;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Providing fixes for <see cref="RH5005UsingStatementsShouldBePrecededByABlankLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5005UsingStatementsShouldBePrecededByABlankLineCodeFixProvider))]
public class RH5005UsingStatementsShouldBePrecededByABlankLineCodeFixProvider : StatementShouldBePrecededByABlankLineCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5005UsingStatementsShouldBePrecededByABlankLineCodeFixProvider()
        : base(RH5005UsingStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, CodeFixResources.RH5005Title)
    {
    }

    #endregion // Constructor
}