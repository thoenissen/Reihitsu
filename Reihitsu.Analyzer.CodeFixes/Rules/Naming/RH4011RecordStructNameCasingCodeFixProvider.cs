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
/// Providing fixes for <see cref="RH4011RecordStructNameCasingAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH4011RecordStructNameCasingCodeFixProvider))]
public class RH4011RecordStructNameCasingCodeFixProvider : CasingCodeFixProviderBase<RecordDeclarationSyntax>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH4011RecordStructNameCasingCodeFixProvider()
        : base(RH4011RecordStructNameCasingAnalyzer.DiagnosticId, CodeFixResources.RH4011Title, CasingUtilities.ToPascalCase)
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