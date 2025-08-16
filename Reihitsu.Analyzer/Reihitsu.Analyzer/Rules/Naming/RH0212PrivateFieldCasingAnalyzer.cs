using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Naming;

/// <summary>
/// RH0212: Method parameter names should be in _camelCase
/// </summary>
public class RH0212PrivateFieldCasingAnalyzer : CasingAnalyzerBase<RH0212PrivateFieldCasingAnalyzer>
{
    #region Fields

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0212";

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0212PrivateFieldCasingAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Naming, nameof(AnalyzerResources.RH0212Title), nameof(AnalyzerResources.RH0212MessageFormat), SyntaxKind.FieldDeclaration, CasingUtilities.IsUnderlineCamelCase)
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
                && declaration.Modifiers.Any(SyntaxKind.PrivateKeyword))
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