using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Spacing;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Spacing;

/// <summary>
/// Code fix provider for <see cref="RH6003SemicolonsMustBeSpacedCorrectlyAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH6003SemicolonsMustBeSpacedCorrectlyCodeFixProvider))]
public class RH6003SemicolonsMustBeSpacedCorrectlyCodeFixProvider : RemoveWhitespaceRunCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH6003SemicolonsMustBeSpacedCorrectlyCodeFixProvider()
        : base(RH6003SemicolonsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, CodeFixResources.RH6003Title)
    {
    }

    #endregion // Constructor
}