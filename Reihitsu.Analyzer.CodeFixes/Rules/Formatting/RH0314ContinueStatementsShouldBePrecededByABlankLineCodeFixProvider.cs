using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.Base;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// Providing fixes for <see cref="RH0314ContinueStatementsShouldBePrecededByABlankLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0314ContinueStatementsShouldBePrecededByABlankLineCodeFixProvider))]
public class RH0314ContinueStatementsShouldBePrecededByABlankLineCodeFixProvider : StatementShouldBePrecededByABlankLineCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0314ContinueStatementsShouldBePrecededByABlankLineCodeFixProvider()
        : base(RH0314ContinueStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, CodeFixResources.RH0314Title)
    {
    }

    #endregion // Constructor
}