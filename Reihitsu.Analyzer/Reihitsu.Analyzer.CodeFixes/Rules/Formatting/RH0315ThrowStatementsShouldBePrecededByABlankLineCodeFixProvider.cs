using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.Base;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// Providing fixes for <see cref="RH0315ThrowStatementsShouldBePrecededByABlankLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0315ThrowStatementsShouldBePrecededByABlankLineCodeFixProvider))]
public class RH0315ThrowStatementsShouldBePrecededByABlankLineCodeFixProvider : StatementShouldBePrecededByABlankLineCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0315ThrowStatementsShouldBePrecededByABlankLineCodeFixProvider()
        : base(RH0315ThrowStatementsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, CodeFixResources.RH0315Title)
    {
    }

    #endregion // Constructor
}
