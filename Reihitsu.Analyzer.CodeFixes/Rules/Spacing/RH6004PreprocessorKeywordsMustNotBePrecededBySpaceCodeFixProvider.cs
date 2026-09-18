using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Spacing;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Spacing;

/// <summary>
/// Code fix provider for <see cref="RH6004PreprocessorKeywordsMustNotBePrecededBySpaceAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH6004PreprocessorKeywordsMustNotBePrecededBySpaceCodeFixProvider))]
public class RH6004PreprocessorKeywordsMustNotBePrecededBySpaceCodeFixProvider : WhitespaceSpanRemovalCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH6004PreprocessorKeywordsMustNotBePrecededBySpaceCodeFixProvider()
        : base(RH6004PreprocessorKeywordsMustNotBePrecededBySpaceAnalyzer.DiagnosticId, CodeFixResources.RH6004Title)
    {
    }

    #endregion // Constructor
}