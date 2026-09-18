using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Spacing;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Spacing;

/// <summary>
/// Code fix provider for <see cref="RH6014ClosingAttributeBracketsMustBeSpacedCorrectlyAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH6014ClosingAttributeBracketsMustBeSpacedCorrectlyCodeFixProvider))]
public class RH6014ClosingAttributeBracketsMustBeSpacedCorrectlyCodeFixProvider : WhitespaceSpanRemovalCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH6014ClosingAttributeBracketsMustBeSpacedCorrectlyCodeFixProvider()
        : base(RH6014ClosingAttributeBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, CodeFixResources.RH6014Title)
    {
    }

    #endregion // Constructor
}