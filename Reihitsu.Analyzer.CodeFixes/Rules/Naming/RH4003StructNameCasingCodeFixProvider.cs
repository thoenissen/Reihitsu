using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Naming;

/// <summary>
/// Providing fixes for <see cref="RH4003StructNameCasingAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH4003StructNameCasingCodeFixProvider))]
public class RH4003StructNameCasingCodeFixProvider : CasingCodeFixProviderBase<StructDeclarationSyntax>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH4003StructNameCasingCodeFixProvider()
        : base(RH4003StructNameCasingAnalyzer.DiagnosticId, CodeFixResources.RH4003Title, CasingUtilities.ToPascalCase)
    {
    }

    #endregion // Constructor

    #region CasingCodeFixProviderBase

    /// <inheritdoc/>
    protected override string GetIdentifier(StructDeclarationSyntax node)
    {
        return node.Identifier.ValueText;
    }

    #endregion // CasingCodeFixProviderBase
}