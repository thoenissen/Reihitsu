using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Naming;

/// <summary>
/// Providing fixes for <see cref="RH4012UnionNameCasingAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH4012UnionNameCasingCodeFixProvider))]
public class RH4012UnionNameCasingCodeFixProvider : CasingCodeFixProviderBase<UnionDeclarationSyntax>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH4012UnionNameCasingCodeFixProvider()
        : base(new RH4012UnionNameCasingAnalyzer(), CodeFixResources.RH4012Title, CasingUtilities.ToPascalCase)
    {
    }

    #endregion // Constructor

    #region CasingCodeFixProviderBase

    /// <inheritdoc/>
    protected override string GetIdentifier(UnionDeclarationSyntax node)
    {
        return node.Identifier.ValueText;
    }

    #endregion // CasingCodeFixProviderBase
}