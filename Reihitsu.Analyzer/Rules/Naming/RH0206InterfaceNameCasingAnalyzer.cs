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
/// RH0206: Interface names should be in PascalCase
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0206InterfaceNameCasingAnalyzer : CasingAnalyzerBase<RH0206InterfaceNameCasingAnalyzer>
{
    #region Fields

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0206";

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0206InterfaceNameCasingAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Naming, nameof(AnalyzerResources.RH0206Title), nameof(AnalyzerResources.RH0206MessageFormat), SyntaxKind.InterfaceDeclaration, IsInterfacePascalCase)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Checks whether the given interface name uses the required <c>I</c> prefix and PascalCase.
    /// </summary>
    /// <param name="name">The interface name.</param>
    /// <returns><see langword="true"/> if the name is valid; otherwise <see langword="false"/>.</returns>
    private static bool IsInterfacePascalCase(string name)
    {
        return string.IsNullOrEmpty(name) == false
               && name.StartsWith("I", StringComparison.InvariantCulture)
               && CasingUtilities.IsPascalCase(name);
    }

    #endregion // Methods

    #region CasingAnalyzerBase

    /// <inheritdoc/>
    protected override IEnumerable<(string Name, Location Location)> GetLocations(SyntaxNode node)
    {
        if (node is InterfaceDeclarationSyntax declaration)
        {
            yield return (declaration.Identifier.ValueText, declaration.Identifier.GetLocation());
        }
    }

    #endregion // CasingAnalyzerBase
}