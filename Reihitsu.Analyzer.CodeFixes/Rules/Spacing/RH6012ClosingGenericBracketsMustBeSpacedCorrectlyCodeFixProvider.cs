using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Spacing;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Spacing;

/// <summary>
/// Code fix provider for <see cref="RH6012ClosingGenericBracketsMustBeSpacedCorrectlyAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH6012ClosingGenericBracketsMustBeSpacedCorrectlyCodeFixProvider))]
public class RH6012ClosingGenericBracketsMustBeSpacedCorrectlyCodeFixProvider : WhitespaceSpanRemovalCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH6012ClosingGenericBracketsMustBeSpacedCorrectlyCodeFixProvider()
        : base(RH6012ClosingGenericBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, CodeFixResources.RH6012Title)
    {
    }

    #endregion // Constructor
}