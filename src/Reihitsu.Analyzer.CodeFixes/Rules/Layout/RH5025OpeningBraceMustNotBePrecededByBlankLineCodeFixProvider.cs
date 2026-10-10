using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Layout;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Code fix provider for <see cref="RH5025OpeningBraceMustNotBePrecededByBlankLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5025OpeningBraceMustNotBePrecededByBlankLineCodeFixProvider))]
public class RH5025OpeningBraceMustNotBePrecededByBlankLineCodeFixProvider : BlankLineSpanRemovalCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5025OpeningBraceMustNotBePrecededByBlankLineCodeFixProvider()
        : base(RH5025OpeningBraceMustNotBePrecededByBlankLineAnalyzer.DiagnosticId, CodeFixResources.RH5025Title)
    {
    }

    #endregion // Constructor
}