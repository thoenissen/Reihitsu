using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0387: Types should be organized with regions
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0387TypesShouldBeOrganizedWithRegionsAnalyzer : DiagnosticAnalyzerBase<RH0387TypesShouldBeOrganizedWithRegionsAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0387";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0387TypesShouldBeOrganizedWithRegionsAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0387Title), nameof(AnalyzerResources.RH0387MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether the directive belongs to the current type declaration
    /// </summary>
    /// <param name="typeDeclaration">Type declaration</param>
    /// <param name="directiveTrivia">Directive trivia</param>
    /// <returns><see langword="true"/> if the directive belongs to the current type</returns>
    private static bool BelongsToType(TypeDeclarationSyntax typeDeclaration, SyntaxTrivia directiveTrivia)
    {
        if ((directiveTrivia.IsKind(SyntaxKind.RegionDirectiveTrivia) == false
             && directiveTrivia.IsKind(SyntaxKind.EndRegionDirectiveTrivia) == false)
            || RegionDirectiveUtilities.IsWithinElementBody(directiveTrivia))
        {
            return false;
        }

        var containingType = directiveTrivia.Token.Parent?.AncestorsAndSelf().OfType<TypeDeclarationSyntax>().FirstOrDefault();

        return containingType == typeDeclaration;
    }

    /// <summary>
    /// Gets the top-level region pairs declared for the current type
    /// </summary>
    /// <param name="typeDeclaration">Type declaration</param>
    /// <returns>Region pairs</returns>
    private static IReadOnlyList<(SyntaxTrivia Region, SyntaxTrivia EndRegion)> GetTopLevelRegions(TypeDeclarationSyntax typeDeclaration)
    {
        var regions = new List<(SyntaxTrivia Region, SyntaxTrivia EndRegion)>();
        var regionStack = new Stack<SyntaxTrivia>();

        foreach (var directiveTrivia in typeDeclaration.DescendantTrivia(descendIntoTrivia: true))
        {
            if (BelongsToType(typeDeclaration, directiveTrivia) == false)
            {
                continue;
            }

            if (directiveTrivia.IsKind(SyntaxKind.RegionDirectiveTrivia))
            {
                regionStack.Push(directiveTrivia);
            }
            else if (regionStack.Count > 0)
            {
                regions.Add((regionStack.Pop(), directiveTrivia));
            }
        }

        return regions;
    }

    /// <summary>
    /// Determines whether the member must be organized by this rule
    /// </summary>
    /// <param name="memberDeclaration">Member declaration</param>
    /// <returns><see langword="true"/> if the member is relevant</returns>
    private static bool IsRelevantMember(MemberDeclarationSyntax memberDeclaration)
    {
        return memberDeclaration is BaseTypeDeclarationSyntax
                                 or DelegateDeclarationSyntax
                                 or FieldDeclarationSyntax
                                 or ConstructorDeclarationSyntax
                                 or DestructorDeclarationSyntax
                                 or PropertyDeclarationSyntax
                                 or IndexerDeclarationSyntax
                                 or EventDeclarationSyntax
                                 or EventFieldDeclarationSyntax
                                 or MethodDeclarationSyntax
                                 or OperatorDeclarationSyntax
                                 or ConversionOperatorDeclarationSyntax;
    }

    /// <summary>
    /// Determines whether the member is contained in a top-level region
    /// </summary>
    /// <param name="memberDeclaration">Member declaration</param>
    /// <param name="regions">Region pairs</param>
    /// <returns><see langword="true"/> if contained in a region</returns>
    private static bool IsWithinRegion(MemberDeclarationSyntax memberDeclaration, IReadOnlyList<(SyntaxTrivia Region, SyntaxTrivia EndRegion)> regions)
    {
        return regions.Any(obj => memberDeclaration.SpanStart >= obj.Region.Span.End
                                  && memberDeclaration.Span.End <= obj.EndRegion.SpanStart);
    }

    /// <summary>
    /// Analyze the type declaration
    /// </summary>
    /// <param name="context">Context</param>
    private void OnTypeDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not TypeDeclarationSyntax typeDeclaration)
        {
            return;
        }

        // Skip nested types
        if (typeDeclaration.Parent is TypeDeclarationSyntax)
        {
            return;
        }

        var relevantMembers = typeDeclaration.Members.Where(IsRelevantMember)
                                                     .ToArray();

        if (relevantMembers.Length == 0)
        {
            return;
        }

        var regions = GetTopLevelRegions(typeDeclaration);

        if (regions.Count == 0
            || relevantMembers.Any(memberDeclaration => IsWithinRegion(memberDeclaration, regions) == false))
        {
            context.ReportDiagnostic(CreateDiagnostic(typeDeclaration.Identifier.GetLocation()));
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
                                         SyntaxKind.InterfaceDeclaration,
                                         SyntaxKind.RecordDeclaration,
                                         SyntaxKind.RecordStructDeclaration);
    }

    #endregion // DiagnosticAnalyzer
}