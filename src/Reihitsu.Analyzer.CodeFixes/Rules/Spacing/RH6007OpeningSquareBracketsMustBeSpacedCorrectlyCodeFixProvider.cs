using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Spacing;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Spacing;

/// <summary>
/// Code fix provider for <see cref="RH6007OpeningSquareBracketsMustBeSpacedCorrectlyAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH6007OpeningSquareBracketsMustBeSpacedCorrectlyCodeFixProvider))]
public class RH6007OpeningSquareBracketsMustBeSpacedCorrectlyCodeFixProvider : RemoveWhitespaceRunCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH6007OpeningSquareBracketsMustBeSpacedCorrectlyCodeFixProvider()
        : base(RH6007OpeningSquareBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, CodeFixResources.RH6007Title)
    {
    }

    #endregion // Constructor
}