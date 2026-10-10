using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Layout;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Providing fixes for <see cref="RH5015CheckedStatementsShouldBePrecededByABlankLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5015CheckedStatementsShouldBePrecededByABlankLineCodeFixProvider))]
public class RH5015CheckedStatementsShouldBePrecededByABlankLineCodeFixProvider : StatementShouldBePrecededByABlankLineCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5015CheckedStatementsShouldBePrecededByABlankLineCodeFixProvider()
        : base(RH5015CheckedStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, CodeFixResources.RH5015Title)
    {
    }

    #endregion // Constructor
}