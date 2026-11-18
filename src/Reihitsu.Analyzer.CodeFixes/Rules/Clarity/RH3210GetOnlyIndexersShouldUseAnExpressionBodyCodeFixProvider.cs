using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Clarity;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Clarity;

/// <summary>
/// Code fix provider for <see cref="RH3210GetOnlyIndexersShouldUseAnExpressionBodyAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH3210GetOnlyIndexersShouldUseAnExpressionBodyCodeFixProvider))]
public class RH3210GetOnlyIndexersShouldUseAnExpressionBodyCodeFixProvider : GetOnlyMemberExpressionBodyCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH3210GetOnlyIndexersShouldUseAnExpressionBodyCodeFixProvider()
        : base(RH3210GetOnlyIndexersShouldUseAnExpressionBodyAnalyzer.DiagnosticId, CodeFixResources.RH3210Title)
    {
    }

    #endregion // Constructor
}