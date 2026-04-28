using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Ordering;

/// <summary>
/// Code fix provider for <see cref="RH0605ProtectedMustComeBeforeInternalAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0605ProtectedMustComeBeforeInternalCodeFixProvider))]
public class RH0605ProtectedMustComeBeforeInternalCodeFixProvider : ModifierOrderingCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0605ProtectedMustComeBeforeInternalCodeFixProvider()
        : base(RH0605ProtectedMustComeBeforeInternalAnalyzer.DiagnosticId, CodeFixResources.RH0605Title)
    {
    }

    #endregion // Constructor

    #region ModifierOrderingCodeFixProviderBase

    /// <inheritdoc/>
    protected override SyntaxTokenList GetUpdatedModifiers(SyntaxTokenList modifiers)
    {
        return ModifierOrderingUtilities.OrderModifiersForRh0605(modifiers);
    }

    #endregion // ModifierOrderingCodeFixProviderBase
}