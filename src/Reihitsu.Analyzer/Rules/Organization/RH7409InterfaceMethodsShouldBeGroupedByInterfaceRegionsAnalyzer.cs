using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Organization;

/// <summary>
/// RH7409: Interface implementation methods should be grouped by interface regions
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH7409InterfaceMethodsShouldBeGroupedByInterfaceRegionsAnalyzer : MembersShouldBeGroupedByRegionsAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH7409";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH7409InterfaceMethodsShouldBeGroupedByInterfaceRegionsAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH7409Title), nameof(AnalyzerResources.RH7409MessageFormat))
    {
    }

    #endregion // Constructor

    #region MembersShouldBeGroupedByRegionsAnalyzerBase

    /// <inheritdoc/>
    protected override IReadOnlyList<string> GetExpectedRegionNames(MemberDeclarationSyntax memberDeclaration, ISymbol symbol)
    {
        return memberDeclaration is MethodDeclarationSyntax
                   ? InterfaceImplementationUtilities.GetInterfaceRegionNames(symbol)
                   : [];
    }

    #endregion // MembersShouldBeGroupedByRegionsAnalyzerBase
}