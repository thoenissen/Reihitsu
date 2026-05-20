using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// Code fix provider for <see cref="RH0817EmptyInterfacesShouldUseSemicolonDeclarationsAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0817EmptyInterfacesShouldUseSemicolonDeclarationsCodeFixProvider))]
public class RH0817EmptyInterfacesShouldUseSemicolonDeclarationsCodeFixProvider : EmptyTypeDeclarationShouldUseSemicolonCodeFixProviderBase
{
    #region EmptyTypeDeclarationShouldUseSemicolonCodeFixProviderBase

    /// <inheritdoc/>
    protected override string DiagnosticId => RH0817EmptyInterfacesShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId;

    /// <inheritdoc/>
    protected override string CodeFixTitle => CodeFixResources.RH0817Title;

    /// <inheritdoc/>
    protected override SyntaxKind DeclarationKind => SyntaxKind.InterfaceDeclaration;

    /// <inheritdoc/>
    protected override LanguageVersion MinimumLanguageVersion => LanguageVersion.CSharp12;

    #endregion // EmptyTypeDeclarationShouldUseSemicolonCodeFixProviderBase
}