using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Organization;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Organization;

/// <summary>
/// Code fix provider for <see cref="RH7205UsingStaticDirectivesMustBePlacedAtCorrectPositionAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH7205UsingStaticDirectivesMustBePlacedAtCorrectPositionCodeFixProvider))]
public class RH7205UsingStaticDirectivesMustBePlacedAtCorrectPositionCodeFixProvider : UsingDirectiveOrderingCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH7205UsingStaticDirectivesMustBePlacedAtCorrectPositionCodeFixProvider()
        : base(RH7205UsingStaticDirectivesMustBePlacedAtCorrectPositionAnalyzer.DiagnosticId, CodeFixResources.RH7205Title)
    {
    }

    #endregion // Constructor
}