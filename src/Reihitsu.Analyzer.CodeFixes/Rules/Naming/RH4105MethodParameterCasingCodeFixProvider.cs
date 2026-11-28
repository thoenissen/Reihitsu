using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Naming;

/// <summary>
/// Providing fixes for <see cref="RH4105MethodParameterCasingAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH4105MethodParameterCasingCodeFixProvider))]
public class RH4105MethodParameterCasingCodeFixProvider : CasingCodeFixProviderBase<SyntaxNode>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH4105MethodParameterCasingCodeFixProvider()
        : base(new RH4105MethodParameterCasingAnalyzer(), CodeFixResources.RH4105Title, CasingUtilities.ToCamelCase)
    {
    }

    #endregion // Constructor

    #region CasingCodeFixProviderBase

    /// <inheritdoc/>
    protected override string GetIdentifier(SyntaxNode node)
    {
        return node is ParameterSyntax parameter
                   ? parameter.Identifier.ValueText
                   : RangeVariableDeclarationUtilities.GetIdentifier(node).ValueText;
    }

    #endregion // CasingCodeFixProviderBase
}