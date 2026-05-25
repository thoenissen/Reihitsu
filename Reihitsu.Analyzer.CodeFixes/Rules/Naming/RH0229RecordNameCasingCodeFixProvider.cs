using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Naming;

/// <summary>
/// Providing fixes for <see cref="RH0229RecordNameCasingAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0229RecordNameCasingCodeFixProvider))]
public class RH0229RecordNameCasingCodeFixProvider : CasingCodeFixProviderBase<RecordDeclarationSyntax>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0229RecordNameCasingCodeFixProvider()
        : base(RH0229RecordNameCasingAnalyzer.DiagnosticId, CodeFixResources.RH0229Title, CasingUtilities.ToPascalCase)
    {
    }

    #endregion // Constructor

    #region CasingCodeFixProviderBase

    /// <inheritdoc/>
    protected override string GetIdentifier(RecordDeclarationSyntax node)
    {
        return node.Identifier.ValueText;
    }

    /// <inheritdoc/>
    protected override SyntaxNode ReplaceIdentifier(RecordDeclarationSyntax node, string identifier)
    {
        return node.WithIdentifier(SyntaxFactory.Identifier(identifier));
    }

    #endregion // CasingCodeFixProviderBase
}