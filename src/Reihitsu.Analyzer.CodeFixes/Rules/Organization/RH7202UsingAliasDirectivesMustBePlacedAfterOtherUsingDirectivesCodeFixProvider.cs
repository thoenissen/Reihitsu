using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Organization;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Organization;

/// <summary>
/// Code fix provider for <see cref="RH7202UsingAliasDirectivesMustBePlacedAfterOtherUsingDirectivesAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH7202UsingAliasDirectivesMustBePlacedAfterOtherUsingDirectivesCodeFixProvider))]
public class RH7202UsingAliasDirectivesMustBePlacedAfterOtherUsingDirectivesCodeFixProvider : UsingDirectiveOrderingCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH7202UsingAliasDirectivesMustBePlacedAfterOtherUsingDirectivesCodeFixProvider()
        : base(RH7202UsingAliasDirectivesMustBePlacedAfterOtherUsingDirectivesAnalyzer.DiagnosticId, CodeFixResources.RH7202Title)
    {
    }

    #endregion // Constructor
}