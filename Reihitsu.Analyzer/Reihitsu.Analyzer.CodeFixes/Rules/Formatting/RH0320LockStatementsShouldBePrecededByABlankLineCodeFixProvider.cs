using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.Base;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// Providing fixes for <see cref="RH0320LockStatementsShouldBePrecededByABlankLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0320LockStatementsShouldBePrecededByABlankLineCodeFixProvider))]
public class RH0320LockStatementsShouldBePrecededByABlankLineCodeFixProvider : StatementShouldBePrecededByABlankLineCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0320LockStatementsShouldBePrecededByABlankLineCodeFixProvider()
        : base(RH0320LockStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, CodeFixResources.RH0320Title)
    {
    }

    #endregion // Constructor
}
