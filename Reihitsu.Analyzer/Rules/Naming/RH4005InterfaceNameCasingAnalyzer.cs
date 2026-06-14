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
/// RH4005: Interface names should be in PascalCase
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH4005InterfaceNameCasingAnalyzer : CasingAnalyzerBase<RH4005InterfaceNameCasingAnalyzer>
{
    #region Fields

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH4005";

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH4005InterfaceNameCasingAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Naming, nameof(AnalyzerResources.RH4005Title), nameof(AnalyzerResources.RH4005MessageFormat), SyntaxKind.InterfaceDeclaration, IsInterfacePascalCase)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Checks whether the given interface name uses the required <c>I</c> prefix and PascalCase
    /// </summary>
    /// <param name="name">The interface name</param>
    /// <returns><see langword="true"/> if the name is valid; otherwise <see langword="false"/></returns>
    private static bool IsInterfacePascalCase(string name)
    {
        return string.IsNullOrEmpty(name) == false
               && name.Length > 1
               && name[0] == 'I'
               && char.IsUpper(name[1])
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