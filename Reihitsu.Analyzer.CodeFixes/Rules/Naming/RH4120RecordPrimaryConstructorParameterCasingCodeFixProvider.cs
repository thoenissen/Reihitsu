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
/// Providing fixes for <see cref="RH4120RecordPrimaryConstructorParameterCasingAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH4120RecordPrimaryConstructorParameterCasingCodeFixProvider))]
public class RH4120RecordPrimaryConstructorParameterCasingCodeFixProvider : CasingCodeFixProviderBase<ParameterSyntax>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH4120RecordPrimaryConstructorParameterCasingCodeFixProvider()
        : base(RH4120RecordPrimaryConstructorParameterCasingAnalyzer.DiagnosticId, CodeFixResources.RH4120Title, CasingUtilities.ToPascalCase)
    {
    }

    #endregion // Constructor

    #region CasingCodeFixProviderBase

    /// <inheritdoc/>
    protected override string GetIdentifier(ParameterSyntax node)
    {
        return node.Identifier.ValueText;
    }

    /// <inheritdoc/>
    protected override SyntaxNode ReplaceIdentifier(ParameterSyntax node, string identifier)
    {
        return node.WithIdentifier(SyntaxFactory.Identifier(identifier));
    }

    #endregion // CasingCodeFixProviderBase
}