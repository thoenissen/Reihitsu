using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Organization;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Organization;

/// <summary>
/// Code fix provider for <see cref="RH7203UsingDirectivesMustBeOrderedAlphabeticallyByNamespaceAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH7203UsingDirectivesMustBeOrderedAlphabeticallyByNamespaceCodeFixProvider))]
public class RH7203UsingDirectivesMustBeOrderedAlphabeticallyByNamespaceCodeFixProvider : UsingDirectiveOrderingCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH7203UsingDirectivesMustBeOrderedAlphabeticallyByNamespaceCodeFixProvider()
        : base(RH7203UsingDirectivesMustBeOrderedAlphabeticallyByNamespaceAnalyzer.DiagnosticId, CodeFixResources.RH7203Title)
    {
    }

    #endregion // Constructor
}