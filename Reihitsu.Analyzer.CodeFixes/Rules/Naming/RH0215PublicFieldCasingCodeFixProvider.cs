using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Naming;

/// <summary>
/// Providing fixes for <see cref="RH0215PublicFieldCasingAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0215PublicFieldCasingCodeFixProvider))]
public class RH0215PublicFieldCasingCodeFixProvider : CasingCodeFixProviderBase<VariableDeclaratorSyntax>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0215PublicFieldCasingCodeFixProvider()
        : base(RH0215PublicFieldCasingAnalyzer.DiagnosticId, CodeFixResources.RH0215Title, CasingUtilities.ToPascalCase)
    {
    }

    #endregion // Constructor

    #region CasingCodeFixProviderBase

    /// <inheritdoc/>
    protected override string GetIdentifier(VariableDeclaratorSyntax node)
    {
        return node.Identifier.ValueText;
    }

    /// <inheritdoc/>
    protected override SyntaxNode ReplaceIdentifier(VariableDeclaratorSyntax node, string identifier)
    {
        return node.WithIdentifier(SyntaxFactory.Identifier(identifier));
    }

    #endregion // CasingCodeFixProviderBase
}