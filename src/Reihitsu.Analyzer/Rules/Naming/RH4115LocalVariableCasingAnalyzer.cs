using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
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
               nameof(AnalyzerResources.RH4115Title),
               nameof(AnalyzerResources.RH4115MessageFormat),
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

    #region CasingAnalyzerBase

    /// <inheritdoc/>
    protected override IEnumerable<(string Name, Location Location)> GetLocations(SyntaxNode node)
    {
        var declaration = node switch
                          {
                              LocalDeclarationStatementSyntax localDeclaration => localDeclaration.Declaration,
                              ForStatementSyntax forStatement => forStatement.Declaration,
                              UsingStatementSyntax usingStatement => usingStatement.Declaration,
                              FixedStatementSyntax fixedStatement => fixedStatement.Declaration,
                              _ => null
                          };

        if (declaration != null)
        {
            foreach (var identifier in declaration.Variables
                                                  .Select(variable => variable.Identifier))
            {
                yield return (identifier.ValueText, identifier.GetLocation());
            }
        }

        if (node is ForEachStatementSyntax forEachStatement)
        {
            yield return (forEachStatement.Identifier.ValueText, forEachStatement.Identifier.GetLocation());
        }

        if (node is SingleVariableDesignationSyntax { Parent: not ParenthesizedVariableDesignationSyntax } designation)
        {
            yield return (designation.Identifier.ValueText, designation.Identifier.GetLocation());
        }

        if (node is CatchDeclarationSyntax { Identifier.RawKind: not 0 } catchDeclaration)
        {
            yield return (catchDeclaration.Identifier.ValueText, catchDeclaration.Identifier.GetLocation());
        }
    }

    #endregion // CasingAnalyzerBase
}