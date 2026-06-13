using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Layout;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Code fix provider for <see cref="RH5108ParameterListMustFollowDeclarationAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5108ParameterListMustFollowDeclarationCodeFixProvider))]
public class RH5108ParameterListMustFollowDeclarationCodeFixProvider : CollapseTokenGapCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5108ParameterListMustFollowDeclarationCodeFixProvider()
        : base(RH5108ParameterListMustFollowDeclarationAnalyzer.DiagnosticId, CodeFixResources.RH5108Title)
    {
    }

    #endregion // Constructor
}