using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Layout;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Code fix provider for <see cref="RH5024ClosingBraceMustNotBePrecededByBlankLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5024ClosingBraceMustNotBePrecededByBlankLineCodeFixProvider))]
public class RH5024ClosingBraceMustNotBePrecededByBlankLineCodeFixProvider : BlankLineSpanRemovalCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5024ClosingBraceMustNotBePrecededByBlankLineCodeFixProvider()
        : base(RH5024ClosingBraceMustNotBePrecededByBlankLineAnalyzer.DiagnosticId, CodeFixResources.RH5024Title)
    {
    }

    #endregion // Constructor
}