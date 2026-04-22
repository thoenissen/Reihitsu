using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Reihitsu.Analyzer.Rules.Ordering;

/// <summary>
/// Code fix provider for <see cref="RH0613UsingStaticDirectivesMustBePlacedAtCorrectPositionAnalyzer"/>.
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0613UsingStaticDirectivesMustBePlacedAtCorrectPositionCodeFixProvider))]
public class RH0613UsingStaticDirectivesMustBePlacedAtCorrectPositionCodeFixProvider : UsingDirectiveOrderingCodeFixProviderBase
{
    /// <summary>
    /// Constructor
    /// </summary>
    public RH0613UsingStaticDirectivesMustBePlacedAtCorrectPositionCodeFixProvider()
        : base(RH0613UsingStaticDirectivesMustBePlacedAtCorrectPositionAnalyzer.DiagnosticId, CodeFixResources.RH0613Title)
    {
    }
}