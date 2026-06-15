using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Naming;

/// <summary>
/// Providing fixes for <see cref="RH4106PrivateFieldCasingAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH4106PrivateFieldCasingCodeFixProvider))]
public class RH4106PrivateFieldCasingCodeFixProvider : CasingCodeFixProviderBase<VariableDeclaratorSyntax>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH4106PrivateFieldCasingCodeFixProvider()
        : base(RH4106PrivateFieldCasingAnalyzer.DiagnosticId, CodeFixResources.RH4106Title, CasingUtilities.ToUnderlineCamelCase)
    {
    }

    #endregion // Constructor

    #region CasingCodeFixProviderBase

    /// <inheritdoc/>
    protected override string GetIdentifier(VariableDeclaratorSyntax node)
    {
        return node.Identifier.ValueText;
    }

    #endregion // CasingCodeFixProviderBase
}