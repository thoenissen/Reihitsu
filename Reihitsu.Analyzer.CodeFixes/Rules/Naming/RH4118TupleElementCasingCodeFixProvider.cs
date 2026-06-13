using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Naming;

/// <summary>
/// Providing fixes for <see cref="RH4118TupleElementCasingAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH4118TupleElementCasingCodeFixProvider))]
public class RH4118TupleElementCasingCodeFixProvider : CasingCodeFixProviderBase<IdentifierNameSyntax>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH4118TupleElementCasingCodeFixProvider()
        : base(RH4118TupleElementCasingAnalyzer.DiagnosticId, CodeFixResources.RH4118Title, CasingUtilities.ToPascalCase)
    {
    }

    #endregion // Constructor

    #region CasingCodeFixProviderBase

    /// <inheritdoc/>
    protected override string GetIdentifier(IdentifierNameSyntax node)
    {
        return node.Identifier.ValueText;
    }

    /// <inheritdoc/>
    protected override bool CanRegisterCodeFix(IdentifierNameSyntax node)
    {
        return false;
    }

    #endregion // CasingCodeFixProviderBase
}