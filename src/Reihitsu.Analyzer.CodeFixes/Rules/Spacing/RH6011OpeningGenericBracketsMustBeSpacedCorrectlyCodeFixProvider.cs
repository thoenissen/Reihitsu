using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Spacing;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Spacing;

/// <summary>
/// Code fix provider for <see cref="RH6011OpeningGenericBracketsMustBeSpacedCorrectlyAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH6011OpeningGenericBracketsMustBeSpacedCorrectlyCodeFixProvider))]
public class RH6011OpeningGenericBracketsMustBeSpacedCorrectlyCodeFixProvider : RemoveWhitespaceRunCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH6011OpeningGenericBracketsMustBeSpacedCorrectlyCodeFixProvider()
        : base(RH6011OpeningGenericBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, CodeFixResources.RH6011Title)
    {
    }

    #endregion // Constructor
}