using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Organization;

/// <summary>
/// RH7401: Override methods should be grouped by base type regions
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH7401OverrideMethodsShouldBeGroupedByBaseTypeRegionsAnalyzer : MembersShouldBeGroupedByRegionsAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH7401";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH7401OverrideMethodsShouldBeGroupedByBaseTypeRegionsAnalyzer()
        : base(DiagnosticId)
    {
    }

    #endregion // Constructor

    #region MembersShouldBeGroupedByRegionsAnalyzerBase

    /// <inheritdoc/>
    protected override IReadOnlyList<string> GetExpectedRegionNames(MemberDeclarationSyntax memberDeclaration, ISymbol symbol)
    {
        return memberDeclaration is MethodDeclarationSyntax
                   ? OverrideMemberUtilities.GetOverrideRegionNames(symbol)
                   : [];
    }

    #endregion // MembersShouldBeGroupedByRegionsAnalyzerBase
}