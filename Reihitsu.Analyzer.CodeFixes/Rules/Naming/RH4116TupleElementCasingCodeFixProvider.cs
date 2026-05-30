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
/// Providing fixes for <see cref="RH4116TupleElementCasingAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH4116TupleElementCasingCodeFixProvider))]
public class RH4116TupleElementCasingCodeFixProvider : CasingCodeFixProviderBase<TupleElementSyntax>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH4116TupleElementCasingCodeFixProvider()
        : base(RH4116TupleElementCasingAnalyzer.DiagnosticId, CodeFixResources.RH4116Title, CasingUtilities.ToPascalCase)
    {
    }

    #endregion // Constructor

    #region CasingCodeFixProviderBase

    /// <inheritdoc/>
    protected override string GetIdentifier(TupleElementSyntax node)
    {
        return node.Identifier.ValueText;
    }

    /// <inheritdoc/>
    protected override bool CanRegisterCodeFix(TupleElementSyntax node)
    {
        return false;
    }

    /// <inheritdoc/>
    protected override SyntaxNode ReplaceIdentifier(TupleElementSyntax node, string identifier)
    {
        return node.WithIdentifier(SyntaxFactory.Identifier(identifier));
    }

    #endregion // CasingCodeFixProviderBase
}