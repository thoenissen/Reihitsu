using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.Base;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// Providing fixes for <see cref="RH0317CheckedStatementsShouldBePrecededByABlankLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0317CheckedStatementsShouldBePrecededByABlankLineCodeFixProvider))]
public class RH0317CheckedStatementsShouldBePrecededByABlankLineCodeFixProvider : StatementShouldBePrecededByABlankLineCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0317CheckedStatementsShouldBePrecededByABlankLineCodeFixProvider()
        : base(RH0317CheckedStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, CodeFixResources.RH0317Title)
    {
    }

    #endregion // Constructor
}