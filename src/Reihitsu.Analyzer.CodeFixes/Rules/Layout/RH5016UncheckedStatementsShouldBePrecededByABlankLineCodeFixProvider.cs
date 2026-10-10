using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Layout;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Providing fixes for <see cref="RH5016UncheckedStatementsShouldBePrecededByABlankLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5016UncheckedStatementsShouldBePrecededByABlankLineCodeFixProvider))]
public class RH5016UncheckedStatementsShouldBePrecededByABlankLineCodeFixProvider : StatementShouldBePrecededByABlankLineCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5016UncheckedStatementsShouldBePrecededByABlankLineCodeFixProvider()
        : base(RH5016UncheckedStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, CodeFixResources.RH5016Title)
    {
    }

    #endregion // Constructor
}