using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Naming;

/// <summary>
/// RH0214: Internal field names should be in PascalCase
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0214InternalFieldCasingAnalyzer : CasingAnalyzerBase<RH0214InternalFieldCasingAnalyzer>
{
    #region Fields

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0214";

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0214InternalFieldCasingAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Naming, nameof(AnalyzerResources.RH0214Title), nameof(AnalyzerResources.RH0214MessageFormat), SyntaxKind.FieldDeclaration, CasingUtilities.IsPascalCase)
    {
    }

    #endregion // Constructor

    #region CasingAnalyzerBase

    /// <inheritdoc/>
    protected override IEnumerable<(string Name, Location Location)> GetLocations(SyntaxNode node)
    {
        if (node is FieldDeclarationSyntax declaration
            && declaration.Modifiers.Any(SyntaxKind.ConstKeyword) == false
            && declaration.Modifiers.Any(SyntaxKind.InternalKeyword))
        {
            foreach (var identifier in declaration.Declaration
                                                  .Variables
                                                  .Select(variable => variable.Identifier))
            {
                yield return (identifier.ValueText, identifier.GetLocation());
            }
        }
    }

    #endregion // CasingAnalyzerBase
}