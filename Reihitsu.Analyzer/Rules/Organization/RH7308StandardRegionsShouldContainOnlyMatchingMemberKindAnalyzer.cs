using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Organization;

/// <summary>
/// RH7308: Standard regions should contain only their matching member kind
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH7308StandardRegionsShouldContainOnlyMatchingMemberKindAnalyzer : DiagnosticAnalyzerBase<RH7308StandardRegionsShouldContainOnlyMatchingMemberKindAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH7308";

    #endregion // Constants

    #region Fields

    /// <summary>
    /// Characters that separate the words of a region label
    /// </summary>
    private static readonly char[] _labelSeparators = [' ', '\t', '/', '&', ',', '-'];

    /// <summary>
    /// Maps singular and plural region label nouns to the canonical member kind they imply
    /// </summary>
    private static readonly Dictionary<string, string> _kindNouns = new(StringComparer.OrdinalIgnoreCase)
                                                                    {
                                                                        ["field"] = "field",
                                                                        ["fields"] = "field",
                                                                        ["const"] = "field",
                                                                        ["constant"] = "field",
                                                                        ["constants"] = "field",
                                                                        ["property"] = "property",
                                                                        ["properties"] = "property",
                                                                        ["method"] = "method",
                                                                        ["methods"] = "method",
                                                                        ["constructor"] = "constructor",
                                                                        ["constructors"] = "constructor",
                                                                        ["ctor"] = "constructor",
                                                                        ["ctors"] = "constructor",
                                                                        ["finalizer"] = "finalizer",
                                                                        ["finalizers"] = "finalizer",
                                                                        ["destructor"] = "finalizer",
                                                                        ["destructors"] = "finalizer",
                                                                        ["event"] = "event",
                                                                        ["events"] = "event",
                                                                        ["indexer"] = "indexer",
                                                                        ["indexers"] = "indexer",
                                                                        ["operator"] = "operator",
                                                                        ["operators"] = "operator"
                                                                    };

    /// <summary>
    /// Words that qualify a region label without changing the implied member kind
    /// </summary>
    private static readonly HashSet<string> _modifierWords = new(StringComparer.OrdinalIgnoreCase)
                                                             {
                                                                 "public",
                                                                 "private",
                                                                 "protected",
                                                                 "internal",
                                                                 "static",
                                                                 "instance",
                                                                 "abstract",
                                                                 "virtual",
                                                                 "override",
                                                                 "sealed",
                                                                 "readonly",
                                                                 "partial",
                                                                 "extern",
                                                                 "async",
                                                                 "unsafe",
                                                                 "new",
                                                                 "explicit",
                                                                 "implicit",
                                                                 "generic",
                                                                 "and",
                                                                 "or"
                                                             };

    /// <summary>
    /// Canonical member kinds in the order used to build diagnostic messages
    /// </summary>
    private static readonly string[] _canonicalKindOrder = ["field", "property", "constructor", "finalizer", "event", "indexer", "operator", "method"];

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH7308StandardRegionsShouldContainOnlyMatchingMemberKindAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Organization, nameof(AnalyzerResources.RH7308Title), nameof(AnalyzerResources.RH7308MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Gets the canonical member kind for the declaration
    /// </summary>
    /// <param name="memberDeclaration">Member declaration</param>
    /// <returns>The canonical member kind, or <see cref="string.Empty"/> if the member kind is not tracked</returns>
    private static string GetMemberKind(MemberDeclarationSyntax memberDeclaration)
    {
        return memberDeclaration switch
               {
                   FieldDeclarationSyntax => "field",
                   PropertyDeclarationSyntax => "property",
                   MethodDeclarationSyntax => "method",
                   ConstructorDeclarationSyntax => "constructor",
                   DestructorDeclarationSyntax => "finalizer",
                   EventDeclarationSyntax or EventFieldDeclarationSyntax => "event",
                   IndexerDeclarationSyntax => "indexer",
                   OperatorDeclarationSyntax or ConversionOperatorDeclarationSyntax => "operator",
                   _ => string.Empty
               };
    }

    /// <summary>
    /// Tries to resolve the set of member kinds a standard region label is expected to contain
    /// </summary>
    /// <param name="regionName">Region label</param>
    /// <param name="expectedKinds">Resolved set of canonical member kinds</param>
    /// <returns><see langword="true"/> if the label is a recognized standard region</returns>
    private static bool TryResolveExpectedKinds(string regionName, out HashSet<string> expectedKinds)
    {
        expectedKinds = new HashSet<string>(StringComparer.Ordinal);

        foreach (var token in regionName.Split(_labelSeparators, StringSplitOptions.RemoveEmptyEntries))
        {
            if (_kindNouns.TryGetValue(token, out var kind))
            {
                expectedKinds.Add(kind);
            }
            else if (_modifierWords.Contains(token) == false)
            {
                expectedKinds.Clear();

                return false;
            }
        }

        return expectedKinds.Count > 0;
    }

    /// <summary>
    /// Builds the human-readable description of the member kinds a region is expected to contain
    /// </summary>
    /// <param name="expectedKinds">Resolved set of canonical member kinds</param>
    /// <returns>Description such as <c>field declarations</c> or <c>constructor or finalizer declarations</c></returns>
    private static string BuildExpectedKindDescription(HashSet<string> expectedKinds)
    {
        var orderedKinds = _canonicalKindOrder.Where(expectedKinds.Contains);

        return $"{string.Join(" or ", orderedKinds)} declarations";
    }

    /// <summary>
    /// Tries to get the containing top-level region name for the member
    /// </summary>
    /// <param name="memberDeclaration">Member declaration</param>
    /// <param name="regions">Top-level regions</param>
    /// <param name="regionName">Containing region name</param>
    /// <returns><see langword="true"/> if a containing top-level region exists</returns>
    private static bool TryGetContainingRegionName(MemberDeclarationSyntax memberDeclaration, IReadOnlyList<(SyntaxTrivia Region, SyntaxTrivia EndRegion)> regions, out string regionName)
    {
        regionName = string.Empty;

        if (RegionDirectiveUtilities.TryFindContainingRegion(memberDeclaration, regions, out var region) == false
            || region.Region.GetStructure() is not RegionDirectiveTriviaSyntax regionDirective)
        {
            return false;
        }

        regionName = RegionDirectiveUtilities.GetRegionDescription(regionDirective);

        return string.IsNullOrEmpty(regionName) == false;
    }

    /// <summary>
    /// Analyzes the type declaration
    /// </summary>
    /// <param name="context">Context</param>
    private void OnTypeDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not TypeDeclarationSyntax typeDeclaration)
        {
            return;
        }

        var regions = RegionDirectiveUtilities.GetTopLevelRegions(typeDeclaration);

        if (regions.Count == 0)
        {
            return;
        }

        foreach (var memberDeclaration in typeDeclaration.Members)
        {
            var memberKind = GetMemberKind(memberDeclaration);

            if (string.IsNullOrEmpty(memberKind)
                || TryGetContainingRegionName(memberDeclaration, regions, out var regionName) == false
                || TryResolveExpectedKinds(regionName, out var expectedKinds) == false
                || expectedKinds.Contains(memberKind))
            {
                continue;
            }

            context.ReportDiagnostic(CreateDiagnostic(OrderingDeclarationUtilities.GetDiagnosticLocation(memberDeclaration), regionName, BuildExpectedKindDescription(expectedKinds)));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnTypeDeclaration,
                                         SyntaxKind.ClassDeclaration,
                                         SyntaxKind.StructDeclaration,
                                         SyntaxKind.RecordDeclaration,
                                         SyntaxKind.RecordStructDeclaration);
    }

    #endregion // DiagnosticAnalyzer
}