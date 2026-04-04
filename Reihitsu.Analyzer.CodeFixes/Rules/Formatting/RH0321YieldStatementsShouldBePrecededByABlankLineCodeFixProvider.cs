using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.Base;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// Providing fixes for <see cref="RH0321YieldStatementsShouldBePrecededByABlankLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0321YieldStatementsShouldBePrecededByABlankLineCodeFixProvider))]
public class RH0321YieldStatementsShouldBePrecededByABlankLineCodeFixProvider : StatementShouldBePrecededByABlankLineCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0321YieldStatementsShouldBePrecededByABlankLineCodeFixProvider()
        : base(RH0321YieldStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, CodeFixResources.RH0321Title)
    {
    }

    #endregion // Constructor
}