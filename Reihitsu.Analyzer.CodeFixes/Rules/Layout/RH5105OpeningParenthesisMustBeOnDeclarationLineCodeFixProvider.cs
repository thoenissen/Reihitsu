using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Layout;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Code fix provider for <see cref="RH5105OpeningParenthesisMustBeOnDeclarationLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5105OpeningParenthesisMustBeOnDeclarationLineCodeFixProvider))]
public class RH5105OpeningParenthesisMustBeOnDeclarationLineCodeFixProvider : CollapseTokenGapCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5105OpeningParenthesisMustBeOnDeclarationLineCodeFixProvider()
        : base(RH5105OpeningParenthesisMustBeOnDeclarationLineAnalyzer.DiagnosticId, CodeFixResources.RH5105Title)
    {
    }

    #endregion // Constructor
}