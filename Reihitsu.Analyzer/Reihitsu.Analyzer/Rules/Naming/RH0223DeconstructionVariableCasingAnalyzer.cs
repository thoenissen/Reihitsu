using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Naming;

/// <summary>
/// RH0223: Deconstruction variable names should be in camelCase
/// </summary>
public class RH0223DeconstructionVariableCasingAnalyzer : CasingAnalyzerBase<RH0223DeconstructionVariableCasingAnalyzer>
{
    #region Fields

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0223";

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0223DeconstructionVariableCasingAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Naming, nameof(AnalyzerResources.RH0223Title), nameof(AnalyzerResources.RH0223MessageFormat), SyntaxKind.DeclarationExpression, CasingUtilities.IsCamelCase)
    {
    }

    #endregion // Constructor

    #region CasingAnalyzerBase

    /// <inheritdoc/>
    protected override IEnumerable<(string Name, Location Location)> GetLocations(SyntaxNode node)
    {
        if (node is DeclarationExpressionSyntax { Designation: ParenthesizedVariableDesignationSyntax tuple } )
        {
            foreach (var variable in tuple.Variables)
            {
                if (variable is SingleVariableDesignationSyntax singleVariable)
                {
                    yield return (singleVariable.Identifier.ValueText, singleVariable.Identifier.GetLocation());
                }
            }
        }
    }

    #endregion // CasingAnalyzerBase
}