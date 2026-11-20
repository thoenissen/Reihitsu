using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Organization;

/// <summary>
/// RH7410: Interface implementation properties should be grouped by interface regions
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH7410InterfacePropertiesShouldBeGroupedByInterfaceRegionsAnalyzer : MembersShouldBeGroupedByRegionsAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH7410";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH7410InterfacePropertiesShouldBeGroupedByInterfaceRegionsAnalyzer()
        : base(DiagnosticId)
    {
    }

    #endregion // Constructor

    #region MembersShouldBeGroupedByRegionsAnalyzerBase

    /// <inheritdoc/>
    protected override IReadOnlyList<string> GetExpectedRegionNames(MemberDeclarationSyntax memberDeclaration, ISymbol symbol)
    {
        return memberDeclaration is PropertyDeclarationSyntax
                   ? InterfaceImplementationUtilities.GetInterfaceRegionNames(symbol)
                   : [];
    }

    #endregion // MembersShouldBeGroupedByRegionsAnalyzerBase
}