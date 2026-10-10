using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Layout;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Providing fixes for <see cref="RH5003WhileStatementsShouldBePrecededByABlankLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5003WhileStatementsShouldBePrecededByABlankLineCodeFixProvider))]
public class RH5003WhileStatementsShouldBePrecededByABlankLineCodeFixProvider : StatementShouldBePrecededByABlankLineCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5003WhileStatementsShouldBePrecededByABlankLineCodeFixProvider()
        : base(RH5003WhileStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, CodeFixResources.RH5003Title)
    {
    }

    #endregion // Constructor
}