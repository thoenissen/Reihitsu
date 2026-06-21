using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Layout;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Code fix provider for <see cref="RH5032EndRegionDirectivesMustBeSurroundedByBlankLinesAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5032EndRegionDirectivesMustBeSurroundedByBlankLinesCodeFixProvider))]
public class RH5032EndRegionDirectivesMustBeSurroundedByBlankLinesCodeFixProvider : RegionDirectiveBlankLineCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5032EndRegionDirectivesMustBeSurroundedByBlankLinesCodeFixProvider()
        : base(RH5032EndRegionDirectivesMustBeSurroundedByBlankLinesAnalyzer.DiagnosticId, CodeFixResources.RH5032Title)
    {
    }

    #endregion // Constructor
}