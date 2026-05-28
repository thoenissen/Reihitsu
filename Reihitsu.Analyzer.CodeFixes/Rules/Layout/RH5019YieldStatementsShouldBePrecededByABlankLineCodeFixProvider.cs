using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Layout;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Providing fixes for <see cref="RH5019YieldStatementsShouldBePrecededByABlankLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5019YieldStatementsShouldBePrecededByABlankLineCodeFixProvider))]
public class RH5019YieldStatementsShouldBePrecededByABlankLineCodeFixProvider : StatementShouldBePrecededByABlankLineCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5019YieldStatementsShouldBePrecededByABlankLineCodeFixProvider()
        : base(RH5019YieldStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, CodeFixResources.RH5019Title)
    {
    }

    #endregion // Constructor
}