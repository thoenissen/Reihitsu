using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Layout;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Code fix provider for <see cref="RH5031RegionDirectivesMustBeSurroundedByBlankLinesAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5031RegionDirectivesMustBeSurroundedByBlankLinesCodeFixProvider))]
public class RH5031RegionDirectivesMustBeSurroundedByBlankLinesCodeFixProvider : RegionDirectiveBlankLineCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5031RegionDirectivesMustBeSurroundedByBlankLinesCodeFixProvider()
        : base(RH5031RegionDirectivesMustBeSurroundedByBlankLinesAnalyzer.DiagnosticId, CodeFixResources.RH5031Title)
    {
    }

    #endregion // Constructor
}