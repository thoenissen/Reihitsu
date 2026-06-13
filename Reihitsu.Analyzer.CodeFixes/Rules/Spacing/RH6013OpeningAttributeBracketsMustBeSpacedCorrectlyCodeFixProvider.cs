using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Spacing;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Spacing;

/// <summary>
/// Code fix provider for <see cref="RH6013OpeningAttributeBracketsMustBeSpacedCorrectlyAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH6013OpeningAttributeBracketsMustBeSpacedCorrectlyCodeFixProvider))]
public class RH6013OpeningAttributeBracketsMustBeSpacedCorrectlyCodeFixProvider : RemoveWhitespaceRunCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH6013OpeningAttributeBracketsMustBeSpacedCorrectlyCodeFixProvider()
        : base(RH6013OpeningAttributeBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, CodeFixResources.RH6013Title)
    {
    }

    #endregion // Constructor
}