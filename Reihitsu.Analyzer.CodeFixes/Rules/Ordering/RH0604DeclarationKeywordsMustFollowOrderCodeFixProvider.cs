using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Ordering;

/// <summary>
/// Code fix provider for <see cref="RH0604DeclarationKeywordsMustFollowOrderAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0604DeclarationKeywordsMustFollowOrderCodeFixProvider))]
public class RH0604DeclarationKeywordsMustFollowOrderCodeFixProvider : ModifierOrderingCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0604DeclarationKeywordsMustFollowOrderCodeFixProvider()
        : base(RH0604DeclarationKeywordsMustFollowOrderAnalyzer.DiagnosticId, CodeFixResources.RH0604Title)
    {
    }

    #endregion // Constructor

    #region ModifierOrderingCodeFixProviderBase

    /// <inheritdoc/>
    protected override SyntaxTokenList GetUpdatedModifiers(SyntaxTokenList modifiers)
    {
        return ModifierOrderingUtilities.OrderModifiersForRh0604(modifiers);
    }

    #endregion // ModifierOrderingCodeFixProviderBase
}