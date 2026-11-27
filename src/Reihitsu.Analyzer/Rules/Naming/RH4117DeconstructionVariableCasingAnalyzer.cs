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
/// RH4117: Deconstruction variable names should be in camelCase
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH4117DeconstructionVariableCasingAnalyzer : CasingAnalyzerBase
{
    #region Fields

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH4117";

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH4117DeconstructionVariableCasingAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Naming, SyntaxKind.SingleVariableDesignation, CasingUtilities.IsCamelCase)
    {
    }

    #endregion // Constructor

    #region CasingAnalyzerBase

    /// <inheritdoc/>
    /// <remarks>
    /// Every variable of a deconstruction is a single variable designation; <see cref="DeconstructionVariableUtilities.IsDeconstructionVariable"/>
    /// decides which ones belong to a deconstruction, in a declaration and in a pattern alike
    /// </remarks>
    protected override IEnumerable<(string Name, Location Location)> GetLocations(SyntaxNode node)
    {
        if (node is SingleVariableDesignationSyntax designation
            && DeconstructionVariableUtilities.IsDeconstructionVariable(designation))
        {
            yield return (designation.Identifier.ValueText, designation.Identifier.GetLocation());
        }
    }

    #endregion // CasingAnalyzerBase
}