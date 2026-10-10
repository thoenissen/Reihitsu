using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Organization;

/// <summary>
/// Code fix provider for <see cref="RH7105DeclarationKeywordsMustFollowOrderAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH7105DeclarationKeywordsMustFollowOrderCodeFixProvider))]
public class RH7105DeclarationKeywordsMustFollowOrderCodeFixProvider : ModifierOrderingCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH7105DeclarationKeywordsMustFollowOrderCodeFixProvider()
        : base(RH7105DeclarationKeywordsMustFollowOrderAnalyzer.DiagnosticId, CodeFixResources.RH7105Title)
    {
    }

    #endregion // Constructor

    #region ModifierOrderingCodeFixProviderBase

    /// <inheritdoc/>
    protected override SyntaxTokenList GetUpdatedModifiers(SyntaxTokenList modifiers)
    {
        return ModifierOrderingUtilities.OrderModifiersForRh7105(modifiers);
    }

    #endregion // ModifierOrderingCodeFixProviderBase
}