using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Reihitsu.Analyzer.Rules.Ordering;

/// <summary>
/// Code fix provider for <see cref="RH0609UsingAliasDirectivesMustBeOrderedAlphabeticallyByAliasNameAnalyzer"/>.
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0609UsingAliasDirectivesMustBeOrderedAlphabeticallyByAliasNameCodeFixProvider))]
public class RH0609UsingAliasDirectivesMustBeOrderedAlphabeticallyByAliasNameCodeFixProvider : UsingDirectiveOrderingCodeFixProviderBase
{
    /// <summary>
    /// Constructor
    /// </summary>
    public RH0609UsingAliasDirectivesMustBeOrderedAlphabeticallyByAliasNameCodeFixProvider()
        : base(RH0609UsingAliasDirectivesMustBeOrderedAlphabeticallyByAliasNameAnalyzer.DiagnosticId, CodeFixResources.RH0609Title)
    {
    }
}