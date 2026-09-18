using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Spacing;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Spacing;

/// <summary>
/// Code fix provider for <see cref="RH6017IncrementAndDecrementSymbolsMustBeSpacedCorrectlyAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH6017IncrementAndDecrementSymbolsMustBeSpacedCorrectlyCodeFixProvider))]
public class RH6017IncrementAndDecrementSymbolsMustBeSpacedCorrectlyCodeFixProvider : WhitespaceSpanRemovalCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH6017IncrementAndDecrementSymbolsMustBeSpacedCorrectlyCodeFixProvider()
        : base(RH6017IncrementAndDecrementSymbolsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, CodeFixResources.RH6017Title)
    {
    }

    #endregion // Constructor
}