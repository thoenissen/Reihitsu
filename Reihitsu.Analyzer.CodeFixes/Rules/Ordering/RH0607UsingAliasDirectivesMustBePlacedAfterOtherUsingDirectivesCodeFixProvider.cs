using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Reihitsu.Analyzer.Rules.Ordering;

/// <summary>
/// Code fix provider for <see cref="RH0607UsingAliasDirectivesMustBePlacedAfterOtherUsingDirectivesAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0607UsingAliasDirectivesMustBePlacedAfterOtherUsingDirectivesCodeFixProvider))]
public class RH0607UsingAliasDirectivesMustBePlacedAfterOtherUsingDirectivesCodeFixProvider : UsingDirectiveOrderingCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0607UsingAliasDirectivesMustBePlacedAfterOtherUsingDirectivesCodeFixProvider()
        : base(RH0607UsingAliasDirectivesMustBePlacedAfterOtherUsingDirectivesAnalyzer.DiagnosticId, CodeFixResources.RH0607Title)
    {
    }

    #endregion // Constructor
}