using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Reihitsu.Analyzer.Rules.Ordering;

/// <summary>
/// Code fix provider for <see cref="RH0614UsingStaticDirectivesMustBeOrderedAlphabeticallyAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0614UsingStaticDirectivesMustBeOrderedAlphabeticallyCodeFixProvider))]
public class RH0614UsingStaticDirectivesMustBeOrderedAlphabeticallyCodeFixProvider : UsingDirectiveOrderingCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0614UsingStaticDirectivesMustBeOrderedAlphabeticallyCodeFixProvider()
        : base(RH0614UsingStaticDirectivesMustBeOrderedAlphabeticallyAnalyzer.DiagnosticId, CodeFixResources.RH0614Title)
    {
    }

    #endregion // Constructor
}