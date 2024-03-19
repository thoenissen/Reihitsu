using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Naming;

/// <summary>
/// Providing fixes for <see cref="RH0205EnumMemberCasingAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0205EnumMemberCasingCodeFixProvider))]
public class RH0205EnumMemberCasingCodeFixProvider : CasingCodeFixProviderBase<EnumMemberDeclarationSyntax>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0205EnumMemberCasingCodeFixProvider()
        : base(RH0205EnumMemberCasingAnalyzer.DiagnosticId, CodeFixResources.RH0205Title, CasingUtilities.ToPascalCase)
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