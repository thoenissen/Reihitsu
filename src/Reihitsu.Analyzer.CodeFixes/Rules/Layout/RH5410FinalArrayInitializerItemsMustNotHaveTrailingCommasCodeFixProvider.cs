using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Layout;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Code fix provider for <see cref="RH5410FinalArrayInitializerItemsMustNotHaveTrailingCommasAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5410FinalArrayInitializerItemsMustNotHaveTrailingCommasCodeFixProvider))]
public class RH5410FinalArrayInitializerItemsMustNotHaveTrailingCommasCodeFixProvider : TrailingCommaRemovalCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5410FinalArrayInitializerItemsMustNotHaveTrailingCommasCodeFixProvider()
        : base(RH5410FinalArrayInitializerItemsMustNotHaveTrailingCommasAnalyzer.DiagnosticId, CodeFixResources.RH5410Title)
    {
    }

    #endregion // Constructor
}