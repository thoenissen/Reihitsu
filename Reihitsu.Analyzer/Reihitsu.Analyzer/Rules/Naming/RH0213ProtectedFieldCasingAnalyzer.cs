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
/// RH0213: Protected field names should be in _camelCase
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0213ProtectedFieldCasingAnalyzer : CasingAnalyzerBase<RH0213ProtectedFieldCasingAnalyzer>
{
    #region Fields

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0213";

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0213ProtectedFieldCasingAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Naming, nameof(AnalyzerResources.RH0213Title), nameof(AnalyzerResources.RH0213MessageFormat), SyntaxKind.FieldDeclaration, CasingUtilities.IsUnderlineCamelCase)
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
                && declaration.Modifiers.Any(SyntaxKind.ProtectedKeyword))
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