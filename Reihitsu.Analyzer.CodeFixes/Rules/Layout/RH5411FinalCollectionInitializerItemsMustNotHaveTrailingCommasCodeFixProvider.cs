using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Layout;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Code fix provider for <see cref="RH5411FinalCollectionInitializerItemsMustNotHaveTrailingCommasAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5411FinalCollectionInitializerItemsMustNotHaveTrailingCommasCodeFixProvider))]
public class RH5411FinalCollectionInitializerItemsMustNotHaveTrailingCommasCodeFixProvider : TrailingCommaRemovalCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5411FinalCollectionInitializerItemsMustNotHaveTrailingCommasCodeFixProvider()
        : base(RH5411FinalCollectionInitializerItemsMustNotHaveTrailingCommasAnalyzer.DiagnosticId, CodeFixResources.RH5411Title)
    {
    }

    #endregion // Constructor
}