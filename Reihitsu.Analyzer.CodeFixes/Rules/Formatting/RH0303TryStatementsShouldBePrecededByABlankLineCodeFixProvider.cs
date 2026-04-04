using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.Base;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// Providing fixes for <see cref="RH0303TryStatementsShouldBePrecededByABlankLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0303TryStatementsShouldBePrecededByABlankLineCodeFixProvider))]
public class RH0303TryStatementsShouldBePrecededByABlankLineCodeFixProvider : StatementShouldBePrecededByABlankLineCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0303TryStatementsShouldBePrecededByABlankLineCodeFixProvider()
        : base(RH0303TryStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, CodeFixResources.RH0303Title)
    {
    }

    #endregion // Constructor
}