using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Organization;

/// <summary>
/// Code fix provider for <see cref="RH7106ProtectedMustComeBeforeInternalAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH7106ProtectedMustComeBeforeInternalCodeFixProvider))]
public class RH7106ProtectedMustComeBeforeInternalCodeFixProvider : ModifierOrderingCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH7106ProtectedMustComeBeforeInternalCodeFixProvider()
        : base(RH7106ProtectedMustComeBeforeInternalAnalyzer.DiagnosticId, CodeFixResources.RH7106Title)
    {
    }

    #endregion // Constructor

    #region ModifierOrderingCodeFixProviderBase

    /// <inheritdoc/>
    protected override SyntaxTokenList GetUpdatedModifiers(SyntaxTokenList modifiers)
    {
        return ModifierOrderingUtilities.OrderModifiersForRh7106(modifiers);
    }

    #endregion // ModifierOrderingCodeFixProviderBase
}