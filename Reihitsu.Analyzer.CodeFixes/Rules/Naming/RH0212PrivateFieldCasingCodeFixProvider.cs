using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Naming;

/// <summary>
/// Providing fixes for <see cref="RH0212PrivateFieldCasingAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0212PrivateFieldCasingCodeFixProvider))]
public class RH0212PrivateFieldCasingCodeFixProvider : CasingCodeFixProviderBase<VariableDeclaratorSyntax>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0212PrivateFieldCasingCodeFixProvider()
        : base(RH0212PrivateFieldCasingAnalyzer.DiagnosticId, CodeFixResources.RH0212Title, CasingUtilities.ToUnderLineCamelCase)
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