using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Layout;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Providing fixes for <see cref="RH5008ReturnStatementsShouldBePrecededByABlankLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5008ReturnStatementsShouldBePrecededByABlankLineCodeFixProvider))]
public class RH5008ReturnStatementsShouldBePrecededByABlankLineCodeFixProvider : StatementShouldBePrecededByABlankLineCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5008ReturnStatementsShouldBePrecededByABlankLineCodeFixProvider()
        : base(RH5008ReturnStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, CodeFixResources.RH5008Title)
    {
    }

    #endregion // Constructor
}