using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Layout;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Code fix provider for <see cref="RH5106ClosingParenthesisMustBeOnLineOfLastArgumentAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5106ClosingParenthesisMustBeOnLineOfLastArgumentCodeFixProvider))]
public class RH5106ClosingParenthesisMustBeOnLineOfLastArgumentCodeFixProvider : CollapseTokenGapCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5106ClosingParenthesisMustBeOnLineOfLastArgumentCodeFixProvider()
        : base(RH5106ClosingParenthesisMustBeOnLineOfLastArgumentAnalyzer.DiagnosticId, CodeFixResources.RH5106Title)
    {
    }

    #endregion // Constructor
}