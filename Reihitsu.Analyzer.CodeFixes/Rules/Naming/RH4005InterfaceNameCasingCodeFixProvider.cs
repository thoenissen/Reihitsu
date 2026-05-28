using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Naming;

/// <summary>
/// Providing fixes for <see cref="RH4005InterfaceNameCasingAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH4005InterfaceNameCasingCodeFixProvider))]
public class RH4005InterfaceNameCasingCodeFixProvider : CasingCodeFixProviderBase<InterfaceDeclarationSyntax>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH4005InterfaceNameCasingCodeFixProvider()
        : base(RH4005InterfaceNameCasingAnalyzer.DiagnosticId, CodeFixResources.RH4005Title, OnTransformIdentifier)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Transform identifier for to IPascalCase
    /// </summary>
    /// <param name="identifier">Identifier</param>
    /// <returns>Transformed identifier</returns>
    private static string OnTransformIdentifier(string identifier)
    {
        if (identifier.StartsWith("i", StringComparison.InvariantCultureIgnoreCase))
        {
            identifier = identifier.Substring(1);
        }

        identifier = CasingUtilities.ToPascalCase(identifier);

        return $"I{identifier}";
    }

    #endregion // Methods

    #region CasingCodeFixProviderBase

    /// <inheritdoc/>
    protected override string GetIdentifier(InterfaceDeclarationSyntax node)
    {
        return node.Identifier.ValueText;
    }

    /// <inheritdoc/>
    protected override SyntaxNode ReplaceIdentifier(InterfaceDeclarationSyntax node, string identifier)
    {
        return node.WithIdentifier(SyntaxFactory.Identifier(identifier));
    }

    #endregion // CasingCodeFixProviderBase
}