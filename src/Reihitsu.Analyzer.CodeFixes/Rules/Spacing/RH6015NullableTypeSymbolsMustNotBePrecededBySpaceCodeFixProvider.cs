using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Spacing;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Spacing;

/// <summary>
/// Code fix provider for <see cref="RH6015NullableTypeSymbolsMustNotBePrecededBySpaceAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH6015NullableTypeSymbolsMustNotBePrecededBySpaceCodeFixProvider))]
public class RH6015NullableTypeSymbolsMustNotBePrecededBySpaceCodeFixProvider : RemoveWhitespaceRunCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH6015NullableTypeSymbolsMustNotBePrecededBySpaceCodeFixProvider()
        : base(RH6015NullableTypeSymbolsMustNotBePrecededBySpaceAnalyzer.DiagnosticId, CodeFixResources.RH6015Title)
    {
    }

    #endregion // Constructor
}