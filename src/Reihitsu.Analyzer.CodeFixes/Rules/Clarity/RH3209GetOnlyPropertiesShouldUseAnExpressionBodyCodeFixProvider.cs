using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Clarity;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Clarity;

/// <summary>
/// Code fix provider for <see cref="RH3209GetOnlyPropertiesShouldUseAnExpressionBodyAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH3209GetOnlyPropertiesShouldUseAnExpressionBodyCodeFixProvider))]
public class RH3209GetOnlyPropertiesShouldUseAnExpressionBodyCodeFixProvider : GetOnlyMemberExpressionBodyCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH3209GetOnlyPropertiesShouldUseAnExpressionBodyCodeFixProvider()
        : base(RH3209GetOnlyPropertiesShouldUseAnExpressionBodyAnalyzer.DiagnosticId, CodeFixResources.RH3209Title, new RH3209GetOnlyPropertiesShouldUseAnExpressionBodyAnalyzer())
    {
    }

    #endregion // Constructor
}