using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Layout;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Providing fixes for <see cref="RH5013ThrowStatementsShouldBePrecededByABlankLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5013ThrowStatementsShouldBePrecededByABlankLineCodeFixProvider))]
public class RH5013ThrowStatementsShouldBePrecededByABlankLineCodeFixProvider : StatementShouldBePrecededByABlankLineCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5013ThrowStatementsShouldBePrecededByABlankLineCodeFixProvider()
        : base(RH5013ThrowStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, CodeFixResources.RH5013Title)
    {
    }

    #endregion // Constructor
}