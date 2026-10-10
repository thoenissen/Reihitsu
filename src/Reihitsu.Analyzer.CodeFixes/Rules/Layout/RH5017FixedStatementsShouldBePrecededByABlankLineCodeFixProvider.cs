using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Layout;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Providing fixes for <see cref="RH5017FixedStatementsShouldBePrecededByABlankLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5017FixedStatementsShouldBePrecededByABlankLineCodeFixProvider))]
public class RH5017FixedStatementsShouldBePrecededByABlankLineCodeFixProvider : StatementShouldBePrecededByABlankLineCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5017FixedStatementsShouldBePrecededByABlankLineCodeFixProvider()
        : base(RH5017FixedStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, CodeFixResources.RH5017Title)
    {
    }

    #endregion // Constructor
}