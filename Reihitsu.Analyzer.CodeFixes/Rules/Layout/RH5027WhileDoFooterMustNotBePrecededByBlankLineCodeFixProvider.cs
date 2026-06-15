using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Layout;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Code fix provider for <see cref="RH5027WhileDoFooterMustNotBePrecededByBlankLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5027WhileDoFooterMustNotBePrecededByBlankLineCodeFixProvider))]
public class RH5027WhileDoFooterMustNotBePrecededByBlankLineCodeFixProvider : BlankLineSpanRemovalCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5027WhileDoFooterMustNotBePrecededByBlankLineCodeFixProvider()
        : base(RH5027WhileDoFooterMustNotBePrecededByBlankLineAnalyzer.DiagnosticId, CodeFixResources.RH5027Title)
    {
    }

    #endregion // Constructor
}