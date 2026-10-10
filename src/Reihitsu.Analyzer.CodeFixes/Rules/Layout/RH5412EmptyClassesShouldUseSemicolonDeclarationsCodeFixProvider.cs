using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Layout;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Code fix provider for <see cref="RH5412EmptyClassesShouldUseSemicolonDeclarationsAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5412EmptyClassesShouldUseSemicolonDeclarationsCodeFixProvider))]
public class RH5412EmptyClassesShouldUseSemicolonDeclarationsCodeFixProvider : EmptyTypeDeclarationShouldUseSemicolonCodeFixProviderBase
{
    #region EmptyTypeDeclarationShouldUseSemicolonCodeFixProviderBase

    /// <inheritdoc/>
    protected override string DiagnosticId => RH5412EmptyClassesShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId;

    /// <inheritdoc/>
    protected override string CodeFixTitle => CodeFixResources.RH5412Title;

    /// <inheritdoc/>
    protected override SyntaxKind DeclarationKind => SyntaxKind.ClassDeclaration;

    /// <inheritdoc/>
    protected override LanguageVersion MinimumLanguageVersion => LanguageVersion.CSharp12;

    #endregion // EmptyTypeDeclarationShouldUseSemicolonCodeFixProviderBase
}