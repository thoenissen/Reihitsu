using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Naming;

/// <summary>
/// RH4115: Local variable names should be in camelCase
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH4115LocalVariableCasingAnalyzer : CasingAnalyzerBase
{
    #region Fields

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH4115";

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH4115LocalVariableCasingAnalyzer()
        : base(DiagnosticId,
               DiagnosticCategory.Naming,
               [
                   SyntaxKind.LocalDeclarationStatement,
                   SyntaxKind.ForStatement,
                   SyntaxKind.ForEachStatement,
                   SyntaxKind.UsingStatement,
                   SyntaxKind.FixedStatement,
                   SyntaxKind.SingleVariableDesignation,
                   SyntaxKind.CatchDeclaration
               ],
               CasingUtilities.IsCamelCase)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Gets the identifiers of the local variables a node declares that this rule checks
    /// </summary>
    /// <param name="node">Node</param>
    /// <returns>The identifiers</returns>
    private static IEnumerable<SyntaxToken> GetDeclaredIdentifiers(SyntaxNode node)
    {
        var declaration = node switch
                          {
                              LocalDeclarationStatementSyntax { IsConst: false } localDeclaration => localDeclaration.Declaration,
                              ForStatementSyntax forStatement => forStatement.Declaration,
                              UsingStatementSyntax usingStatement => usingStatement.Declaration,
                              FixedStatementSyntax fixedStatement => fixedStatement.Declaration,
                              _ => null
                          };

        if (declaration != null)
        {
            foreach (var variable in declaration.Variables)
            {
                yield return variable.Identifier;
            }
        }

        if (node is ForEachStatementSyntax forEachStatement)
        {
            yield return forEachStatement.Identifier;
        }

        if (node is SingleVariableDesignationSyntax designation
            && DeconstructionVariableUtilities.IsDeconstructionVariable(designation) == false)
        {
            yield return designation.Identifier;
        }

        if (node is CatchDeclarationSyntax { Identifier.RawKind: not 0 } catchDeclaration)
        {
            yield return catchDeclaration.Identifier;
        }
    }

    #endregion // Methods

    #region CasingAnalyzerBase

    /// <inheritdoc/>
    /// <remarks>
    /// A local constant is left to RH4128, a variable of a deconstruction to RH4117, as <see cref="DeconstructionVariableUtilities.IsDeconstructionVariable"/>
    /// decides it, and a local named <c>_</c> is not checked
    /// </remarks>
    protected override IEnumerable<(string Name, Location Location)> GetLocations(SyntaxNode node)
    {
        return GetDeclaredIdentifiers(node).Where(static identifier => identifier.ValueText != "_")
                                           .Select(static identifier => (identifier.ValueText, identifier.GetLocation()));
    }

    #endregion // CasingAnalyzerBase
}