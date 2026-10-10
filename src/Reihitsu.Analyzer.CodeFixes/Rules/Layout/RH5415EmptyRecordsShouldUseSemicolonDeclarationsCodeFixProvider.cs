using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Layout;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Code fix provider for <see cref="RH5415EmptyRecordsShouldUseSemicolonDeclarationsAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5415EmptyRecordsShouldUseSemicolonDeclarationsCodeFixProvider))]
public class RH5415EmptyRecordsShouldUseSemicolonDeclarationsCodeFixProvider : EmptyTypeDeclarationShouldUseSemicolonCodeFixProviderBase
{
    #region EmptyTypeDeclarationShouldUseSemicolonCodeFixProviderBase

    /// <inheritdoc/>
    protected override string DiagnosticId => RH5415EmptyRecordsShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId;

    /// <inheritdoc/>
    protected override string CodeFixTitle => CodeFixResources.RH5415Title;

    /// <inheritdoc/>
    protected override SyntaxKind DeclarationKind => SyntaxKind.RecordDeclaration;

    /// <inheritdoc/>
    protected override LanguageVersion MinimumLanguageVersion => LanguageVersion.CSharp9;

    #endregion // EmptyTypeDeclarationShouldUseSemicolonCodeFixProviderBase
}