using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Layout;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Code fix provider for <see cref="RH5023CodeMustNotContainMultipleBlankLinesInARowAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5023CodeMustNotContainMultipleBlankLinesInARowCodeFixProvider))]
public class RH5023CodeMustNotContainMultipleBlankLinesInARowCodeFixProvider : BlankLineSpanRemovalCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5023CodeMustNotContainMultipleBlankLinesInARowCodeFixProvider()
        : base(RH5023CodeMustNotContainMultipleBlankLinesInARowAnalyzer.DiagnosticId, CodeFixResources.RH5023Title)
    {
    }

    #endregion // Constructor
}