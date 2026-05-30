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
/// Providing fixes for <see cref="RH4101EnumMemberCasingAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH4101EnumMemberCasingCodeFixProvider))]
public class RH4101EnumMemberCasingCodeFixProvider : CasingCodeFixProviderBase<EnumMemberDeclarationSyntax>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH4101EnumMemberCasingCodeFixProvider()
        : base(RH4101EnumMemberCasingAnalyzer.DiagnosticId, CodeFixResources.RH4101Title, CasingUtilities.ToPascalCase)
    {
    }

    #endregion // Constructor

    #region CasingCodeFixProviderBase

    /// <inheritdoc/>
    protected override string GetIdentifier(EnumMemberDeclarationSyntax node)
    {
        return node.Identifier.ValueText;
    }

    /// <inheritdoc/>
    protected override SyntaxNode ReplaceIdentifier(EnumMemberDeclarationSyntax node, string identifier)
    {
        return node.WithIdentifier(SyntaxFactory.Identifier(identifier))
                   .WithLeadingTrivia(node.GetLeadingTrivia())
                   .WithTrailingTrivia(node.GetTrailingTrivia());
    }

    #endregion // CasingCodeFixProviderBase
}