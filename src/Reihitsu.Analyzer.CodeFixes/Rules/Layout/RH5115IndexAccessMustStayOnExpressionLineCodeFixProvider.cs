using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Layout;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Code fix provider for <see cref="RH5115IndexAccessMustStayOnExpressionLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5115IndexAccessMustStayOnExpressionLineCodeFixProvider))]
public class RH5115IndexAccessMustStayOnExpressionLineCodeFixProvider : AttachedPartLineBreakCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5115IndexAccessMustStayOnExpressionLineCodeFixProvider()
        : base(new RH5115IndexAccessMustStayOnExpressionLineAnalyzer(), RH5115IndexAccessMustStayOnExpressionLineAnalyzer.DiagnosticId, CodeFixResources.RH5115Title)
    {
    }

    #endregion // Constructor
}