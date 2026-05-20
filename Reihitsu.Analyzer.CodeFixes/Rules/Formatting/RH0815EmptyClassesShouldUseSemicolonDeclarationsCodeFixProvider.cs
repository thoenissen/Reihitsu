using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// Code fix provider for <see cref="RH0815EmptyClassesShouldUseSemicolonDeclarationsAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0815EmptyClassesShouldUseSemicolonDeclarationsCodeFixProvider))]
public class RH0815EmptyClassesShouldUseSemicolonDeclarationsCodeFixProvider : EmptyTypeDeclarationShouldUseSemicolonCodeFixProviderBase
{
    #region EmptyTypeDeclarationShouldUseSemicolonCodeFixProviderBase

    /// <inheritdoc/>
    protected override string DiagnosticId => RH0815EmptyClassesShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId;

    /// <inheritdoc/>
    protected override string CodeFixTitle => CodeFixResources.RH0815Title;

    /// <inheritdoc/>
    protected override SyntaxKind DeclarationKind => SyntaxKind.ClassDeclaration;

    /// <inheritdoc/>
    protected override LanguageVersion MinimumLanguageVersion => LanguageVersion.CSharp12;

    #endregion // EmptyTypeDeclarationShouldUseSemicolonCodeFixProviderBase
}