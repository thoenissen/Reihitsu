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
/// RH0215: Public field names should be in PascalCase
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0215PublicFieldCasingAnalyzer : CasingAnalyzerBase<RH0215PublicFieldCasingAnalyzer>
{
    #region Fields

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0215";

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0215PublicFieldCasingAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Naming, nameof(AnalyzerResources.RH0215Title), nameof(AnalyzerResources.RH0215MessageFormat), SyntaxKind.FieldDeclaration, CasingUtilities.IsPascalCase)
    {
    }

    #endregion // Constructor

    #region CasingAnalyzerBase

    /// <inheritdoc/>
    protected override IEnumerable<(string Name, Location Location)> GetLocations(SyntaxNode node)
    {
        if (node is FieldDeclarationSyntax declaration)
        {
            if (declaration.Modifiers.Any(SyntaxKind.ConstKeyword) == false
                && declaration.Modifiers.Any(SyntaxKind.PublicKeyword))
            {
                foreach (var variable in declaration.Declaration.Variables)
                {
                    yield return (variable.Identifier.ValueText, variable.Identifier.GetLocation());
                }
            }
        }
    }

    #endregion // CasingAnalyzerBase
}