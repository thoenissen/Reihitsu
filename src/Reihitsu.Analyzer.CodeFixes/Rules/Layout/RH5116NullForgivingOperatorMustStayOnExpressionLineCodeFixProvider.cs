using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Layout;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Code fix provider for <see cref="RH5116NullForgivingOperatorMustStayOnExpressionLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5116NullForgivingOperatorMustStayOnExpressionLineCodeFixProvider))]
public class RH5116NullForgivingOperatorMustStayOnExpressionLineCodeFixProvider : AttachedPartLineBreakCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5116NullForgivingOperatorMustStayOnExpressionLineCodeFixProvider()
        : base(new RH5116NullForgivingOperatorMustStayOnExpressionLineAnalyzer(), RH5116NullForgivingOperatorMustStayOnExpressionLineAnalyzer.DiagnosticId, CodeFixResources.RH5116Title)
    {
    }

    #endregion // Constructor
}