using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Naming;

/// <summary>
/// Providing fixes for <see cref="RH4115LocalVariableCasingAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH4115LocalVariableCasingCodeFixProvider))]
public class RH4115LocalVariableCasingCodeFixProvider : CasingCodeFixProviderBase<SyntaxNode>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH4115LocalVariableCasingCodeFixProvider()
        : base(RH4115LocalVariableCasingAnalyzer.DiagnosticId, CodeFixResources.RH4115Title, CasingUtilities.ToCamelCase)
    {
    }

    #endregion // Constructor

    #region CasingCodeFixProviderBase

    /// <inheritdoc/>
    protected override string GetIdentifier(SyntaxNode node)
    {
        return node switch
               {
                   VariableDeclaratorSyntax variableDeclarator => variableDeclarator.Identifier.ValueText,
                   ForEachStatementSyntax forEachStatement => forEachStatement.Identifier.ValueText,
                   SingleVariableDesignationSyntax designation => designation.Identifier.ValueText,
                   CatchDeclarationSyntax catchDeclaration => catchDeclaration.Identifier.ValueText,
                   _ => string.Empty
               };
    }

    #endregion // CasingCodeFixProviderBase
}