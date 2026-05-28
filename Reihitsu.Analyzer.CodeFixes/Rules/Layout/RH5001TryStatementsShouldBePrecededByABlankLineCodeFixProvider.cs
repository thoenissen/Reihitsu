using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Layout;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Providing fixes for <see cref="RH5001TryStatementsShouldBePrecededByABlankLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5001TryStatementsShouldBePrecededByABlankLineCodeFixProvider))]
public class RH5001TryStatementsShouldBePrecededByABlankLineCodeFixProvider : StatementShouldBePrecededByABlankLineCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5001TryStatementsShouldBePrecededByABlankLineCodeFixProvider()
        : base(RH5001TryStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, CodeFixResources.RH5001Title)
    {
    }

    #endregion // Constructor
}