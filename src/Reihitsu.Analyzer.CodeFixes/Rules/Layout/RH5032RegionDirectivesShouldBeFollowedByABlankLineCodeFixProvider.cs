using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Layout;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Code fix provider for <see cref="RH5032RegionDirectivesShouldBeFollowedByABlankLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5032RegionDirectivesShouldBeFollowedByABlankLineCodeFixProvider))]
public class RH5032RegionDirectivesShouldBeFollowedByABlankLineCodeFixProvider : RegionDirectiveBlankLineCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5032RegionDirectivesShouldBeFollowedByABlankLineCodeFixProvider()
        : base(RH5032RegionDirectivesShouldBeFollowedByABlankLineAnalyzer.DiagnosticId, CodeFixResources.RH5032Title, insertPrecedingBlankLine: false)
    {
    }

    #endregion // Constructor
}