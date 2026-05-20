using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// Code fix provider for <see cref="RH0816EmptyStructsShouldUseSemicolonDeclarationsAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0816EmptyStructsShouldUseSemicolonDeclarationsCodeFixProvider))]
public class RH0816EmptyStructsShouldUseSemicolonDeclarationsCodeFixProvider : EmptyTypeDeclarationShouldUseSemicolonCodeFixProviderBase
{
    #region EmptyTypeDeclarationShouldUseSemicolonCodeFixProviderBase

    /// <inheritdoc/>
    protected override string DiagnosticId => RH0816EmptyStructsShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId;

    /// <inheritdoc/>
    protected override string CodeFixTitle => CodeFixResources.RH0816Title;

    /// <inheritdoc/>
    protected override SyntaxKind DeclarationKind => SyntaxKind.StructDeclaration;

    /// <inheritdoc/>
    protected override LanguageVersion MinimumLanguageVersion => LanguageVersion.CSharp12;

    #endregion // EmptyTypeDeclarationShouldUseSemicolonCodeFixProviderBase
}