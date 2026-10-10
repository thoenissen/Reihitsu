using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Layout;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Providing fixes for <see cref="RH5012ContinueStatementsShouldBePrecededByABlankLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5012ContinueStatementsShouldBePrecededByABlankLineCodeFixProvider))]
public class RH5012ContinueStatementsShouldBePrecededByABlankLineCodeFixProvider : StatementShouldBePrecededByABlankLineCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5012ContinueStatementsShouldBePrecededByABlankLineCodeFixProvider()
        : base(RH5012ContinueStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, CodeFixResources.RH5012Title)
    {
    }

    #endregion // Constructor
}