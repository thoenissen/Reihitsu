using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Layout;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Code fix provider for <see cref="RH5106ClosingParenthesisMustBeOnLineOfOpeningParenthesisAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5106ClosingParenthesisMustBeOnLineOfOpeningParenthesisCodeFixProvider))]
public class RH5106ClosingParenthesisMustBeOnLineOfOpeningParenthesisCodeFixProvider : CollapseTokenGapCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5106ClosingParenthesisMustBeOnLineOfOpeningParenthesisCodeFixProvider()
        : base(RH5106ClosingParenthesisMustBeOnLineOfOpeningParenthesisAnalyzer.DiagnosticId, CodeFixResources.RH5106Title)
    {
    }

    #endregion // Constructor
}