using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Reihitsu.Analyzer.Rules.Ordering;

/// <summary>
/// Code fix provider for <see cref="RH0608UsingDirectivesMustBeOrderedAlphabeticallyByNamespaceAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0608UsingDirectivesMustBeOrderedAlphabeticallyByNamespaceCodeFixProvider))]
public class RH0608UsingDirectivesMustBeOrderedAlphabeticallyByNamespaceCodeFixProvider : UsingDirectiveOrderingCodeFixProviderBase
{
    /// <summary>
    /// Constructor
    /// </summary>
    public RH0608UsingDirectivesMustBeOrderedAlphabeticallyByNamespaceCodeFixProvider()
        : base(RH0608UsingDirectivesMustBeOrderedAlphabeticallyByNamespaceAnalyzer.DiagnosticId, CodeFixResources.RH0608Title)
    {
    }
}