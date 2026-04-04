using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Naming;

/// <summary>
/// Providing fixes for <see cref="RH0222TupleElementCasingAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0222TupleElementCasingCodeFixProvider))]
public class RH0222TupleElementCasingCodeFixProvider : CasingCodeFixProviderBase<TupleElementSyntax>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0222TupleElementCasingCodeFixProvider()
        : base(RH0222TupleElementCasingAnalyzer.DiagnosticId, CodeFixResources.RH0222Title, CasingUtilities.ToPascalCase)
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
    protected override SyntaxNode ReplaceIdentifier(TupleElementSyntax node, string identifier)
    {
        return node.WithIdentifier(SyntaxFactory.Identifier(identifier));
    }

    #endregion // CasingCodeFixProviderBase
}