using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Naming;

/// <summary>
/// Providing fixes for <see cref="RH4004EnumNameCasingAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH4004EnumNameCasingCodeFixProvider))]
public class RH4004EnumNameCasingCodeFixProvider : CasingCodeFixProviderBase<EnumDeclarationSyntax>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH4004EnumNameCasingCodeFixProvider()
        : base(RH4004EnumNameCasingAnalyzer.DiagnosticId, CodeFixResources.RH4004Title, CasingUtilities.ToPascalCase)
    {
    }

    #endregion // Constructor

    #region CasingCodeFixProviderBase

    /// <inheritdoc/>
    protected override string GetIdentifier(EnumDeclarationSyntax node)
    {
        return node.Identifier.ValueText;
    }

    /// <inheritdoc/>
    protected override SyntaxNode ReplaceIdentifier(EnumDeclarationSyntax node, string identifier)
    {
        return node.WithIdentifier(SyntaxFactory.Identifier(identifier));
    }

    #endregion // CasingCodeFixProviderBase
}