using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Reihitsu.Analyzer.Rules.Ordering;

/// <summary>
/// Code fix provider for <see cref="RH0606SystemUsingDirectivesMustBePlacedBeforeOtherUsingDirectivesAnalyzer"/>.
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0606SystemUsingDirectivesMustBePlacedBeforeOtherUsingDirectivesCodeFixProvider))]
public class RH0606SystemUsingDirectivesMustBePlacedBeforeOtherUsingDirectivesCodeFixProvider : UsingDirectiveOrderingCodeFixProviderBase
{
    /// <summary>
    /// Constructor
    /// </summary>
    public RH0606SystemUsingDirectivesMustBePlacedBeforeOtherUsingDirectivesCodeFixProvider()
        : base(RH0606SystemUsingDirectivesMustBePlacedBeforeOtherUsingDirectivesAnalyzer.DiagnosticId, CodeFixResources.RH0606Title)
    {
    }
}