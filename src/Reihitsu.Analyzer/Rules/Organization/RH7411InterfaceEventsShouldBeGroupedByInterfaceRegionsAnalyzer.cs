using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Organization;

/// <summary>
/// RH7411: Interface implementation events should be grouped by interface regions
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH7411InterfaceEventsShouldBeGroupedByInterfaceRegionsAnalyzer : MembersShouldBeGroupedByRegionsAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH7411";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH7411InterfaceEventsShouldBeGroupedByInterfaceRegionsAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH7411Title), nameof(AnalyzerResources.RH7411MessageFormat))
    {
    }

    #endregion // Constructor

    #region MembersShouldBeGroupedByRegionsAnalyzerBase

    /// <inheritdoc/>
    protected override IReadOnlyList<string> GetExpectedRegionNames(MemberDeclarationSyntax memberDeclaration, ISymbol symbol)
    {
        return memberDeclaration is EventDeclarationSyntax or EventFieldDeclarationSyntax
                   ? InterfaceImplementationUtilities.GetInterfaceRegionNames(symbol)
                   : [];
    }

    #endregion // MembersShouldBeGroupedByRegionsAnalyzerBase
}