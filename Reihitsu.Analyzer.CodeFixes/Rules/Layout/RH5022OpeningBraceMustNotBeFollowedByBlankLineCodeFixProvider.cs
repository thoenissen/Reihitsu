using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Layout;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Code fix provider for <see cref="RH5022OpeningBraceMustNotBeFollowedByBlankLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5022OpeningBraceMustNotBeFollowedByBlankLineCodeFixProvider))]
public class RH5022OpeningBraceMustNotBeFollowedByBlankLineCodeFixProvider : BlankLineSpanRemovalCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5022OpeningBraceMustNotBeFollowedByBlankLineCodeFixProvider()
        : base(RH5022OpeningBraceMustNotBeFollowedByBlankLineAnalyzer.DiagnosticId, CodeFixResources.RH5022Title)
    {
    }

    #endregion // Constructor
}