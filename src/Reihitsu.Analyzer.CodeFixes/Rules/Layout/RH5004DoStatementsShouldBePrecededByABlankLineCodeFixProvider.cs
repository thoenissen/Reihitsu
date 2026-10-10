using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Layout;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Providing fixes for <see cref="RH5004DoStatementsShouldBePrecededByABlankLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5004DoStatementsShouldBePrecededByABlankLineCodeFixProvider))]
public class RH5004DoStatementsShouldBePrecededByABlankLineCodeFixProvider : StatementShouldBePrecededByABlankLineCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5004DoStatementsShouldBePrecededByABlankLineCodeFixProvider()
        : base(RH5004DoStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, CodeFixResources.RH5004Title)
    {
    }

    #endregion // Constructor
}