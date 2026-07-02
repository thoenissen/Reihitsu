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
/// RH4106: Private field names should be in _camelCase
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH4106PrivateFieldCasingAnalyzer : CasingAnalyzerBase<RH4106PrivateFieldCasingAnalyzer>
{
    #region Fields

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH4106";

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH4106PrivateFieldCasingAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Naming, nameof(AnalyzerResources.RH4106Title), nameof(AnalyzerResources.RH4106MessageFormat), SyntaxKind.FieldDeclaration, CasingUtilities.IsUnderlineCamelCase)
    {
    }

    #endregion // Constructor

    #region CasingAnalyzerBase

    /// <inheritdoc/>
    protected override IEnumerable<(string Name, Location Location)> GetLocations(SyntaxNode node)
    {
        if (node is FieldDeclarationSyntax declaration
            && declaration.Modifiers.Any(SyntaxKind.ConstKeyword) == false
            && declaration.Modifiers.Any(SyntaxKind.PrivateKeyword)
            && declaration.Modifiers.Any(SyntaxKind.ProtectedKeyword) == false)
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