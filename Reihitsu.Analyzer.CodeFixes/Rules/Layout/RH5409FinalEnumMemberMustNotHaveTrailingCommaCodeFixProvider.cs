using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Layout;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Code fix provider for <see cref="RH5409FinalEnumMemberMustNotHaveTrailingCommaAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5409FinalEnumMemberMustNotHaveTrailingCommaCodeFixProvider))]
public class RH5409FinalEnumMemberMustNotHaveTrailingCommaCodeFixProvider : TrailingCommaRemovalCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5409FinalEnumMemberMustNotHaveTrailingCommaCodeFixProvider()
        : base(RH5409FinalEnumMemberMustNotHaveTrailingCommaAnalyzer.DiagnosticId, CodeFixResources.RH5409Title)
    {
    }

    #endregion // Constructor
}