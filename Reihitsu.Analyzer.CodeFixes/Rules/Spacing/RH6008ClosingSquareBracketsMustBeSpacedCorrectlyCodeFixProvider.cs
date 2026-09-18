using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Spacing;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Spacing;

/// <summary>
/// Code fix provider for <see cref="RH6008ClosingSquareBracketsMustBeSpacedCorrectlyAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH6008ClosingSquareBracketsMustBeSpacedCorrectlyCodeFixProvider))]
public class RH6008ClosingSquareBracketsMustBeSpacedCorrectlyCodeFixProvider : WhitespaceSpanRemovalCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH6008ClosingSquareBracketsMustBeSpacedCorrectlyCodeFixProvider()
        : base(RH6008ClosingSquareBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, CodeFixResources.RH6008Title)
    {
    }

    #endregion // Constructor
}