using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Layout;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Providing fixes for <see cref="RH5018LockStatementsShouldBePrecededByABlankLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5018LockStatementsShouldBePrecededByABlankLineCodeFixProvider))]
public class RH5018LockStatementsShouldBePrecededByABlankLineCodeFixProvider : StatementShouldBePrecededByABlankLineCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5018LockStatementsShouldBePrecededByABlankLineCodeFixProvider()
        : base(RH5018LockStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, CodeFixResources.RH5018Title)
    {
    }

    #endregion // Constructor
}