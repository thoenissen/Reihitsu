using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Layout;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Code fix provider for <see cref="RH5031RegionDirectivesShouldBePrecededByABlankLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5031RegionDirectivesShouldBePrecededByABlankLineCodeFixProvider))]
public class RH5031RegionDirectivesShouldBePrecededByABlankLineCodeFixProvider : RegionDirectiveBlankLineCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5031RegionDirectivesShouldBePrecededByABlankLineCodeFixProvider()
        : base(RH5031RegionDirectivesShouldBePrecededByABlankLineAnalyzer.DiagnosticId, CodeFixResources.RH5031Title, insertPrecedingBlankLine: true)
    {
    }

    #endregion // Constructor
}