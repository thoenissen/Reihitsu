using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Layout;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Providing fixes for <see cref="RH5002IfStatementsShouldBePrecededByABlankLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5002IfStatementsShouldBePrecededByABlankLineCodeFixProvider))]
public class RH5002IfStatementsShouldBePrecededByABlankLineCodeFixProvider : StatementShouldBePrecededByABlankLineCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5002IfStatementsShouldBePrecededByABlankLineCodeFixProvider()
        : base(RH5002IfStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, CodeFixResources.RH5002Title)
    {
    }

    #endregion // Constructor
}