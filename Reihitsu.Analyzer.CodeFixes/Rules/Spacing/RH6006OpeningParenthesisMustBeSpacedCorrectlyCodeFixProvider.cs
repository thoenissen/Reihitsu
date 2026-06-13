using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Spacing;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Spacing;

/// <summary>
/// Code fix provider for <see cref="RH6006OpeningParenthesisMustBeSpacedCorrectlyAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH6006OpeningParenthesisMustBeSpacedCorrectlyCodeFixProvider))]
public class RH6006OpeningParenthesisMustBeSpacedCorrectlyCodeFixProvider : RemoveWhitespaceRunCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH6006OpeningParenthesisMustBeSpacedCorrectlyCodeFixProvider()
        : base(RH6006OpeningParenthesisMustBeSpacedCorrectlyAnalyzer.DiagnosticId, CodeFixResources.RH6006Title)
    {
    }

    #endregion // Constructor
}