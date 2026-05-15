using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Base;

/// <summary>
/// Base class for analyzers that check whether type members are organized with regions
/// </summary>
/// <typeparam name="TAnalyzer">Type of the analyzer</typeparam>
public abstract class TypesOrganizedWithRegionsAnalyzerBase<TAnalyzer> : DiagnosticAnalyzerBase<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="diagnosticId">Diagnostic ID</param>
    /// <param name="category">Category</param>
    /// <param name="titleResourceName">Resource name of the title</param>
    /// <param name="messageFormatResourceName">Resource name of the message format</param>
    /// <param name="isEnabledByDefault">Whether the rule is enabled by default</param>
    private protected TypesOrganizedWithRegionsAnalyzerBase(string diagnosticId, DiagnosticCategory category, string titleResourceName, string messageFormatResourceName, bool isEnabledByDefault)
        : base(diagnosticId, category, titleResourceName, messageFormatResourceName, isEnabledByDefault)
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
        if (RegionDirectiveUtilities.IsWithinElementBody(directiveTrivia))
        {
            return false;
        }

        if (typeDeclaration.Span.Contains(directiveTrivia.SpanStart) == false)
        {
            return false;
        }

        return typeDeclaration.DescendantNodes()
                              .OfType<TypeDeclarationSyntax>()
                              .Any(nestedType => nestedType.Span.Contains(directiveTrivia.SpanStart)) == false;
    }

    /// <summary>
    /// Gets the top-level region pairs declared for the current type
    /// </summary>
    /// <param name="typeDeclaration">Type declaration</param>
    /// <returns>Region pairs</returns>
    private static List<(SyntaxTrivia Region, SyntaxTrivia EndRegion)> GetTopLevelRegions(TypeDeclarationSyntax typeDeclaration)
    {
        var regions = new List<(SyntaxTrivia Region, SyntaxTrivia EndRegion)>();
        var regionStack = new Stack<SyntaxTrivia>();

        foreach (var directiveTrivia in typeDeclaration.DescendantTrivia(descendIntoTrivia: true)
                                                       .Where(trivia => trivia.IsKind(SyntaxKind.RegionDirectiveTrivia)
                                                                        || trivia.IsKind(SyntaxKind.EndRegionDirectiveTrivia)))
        {
            if (BelongsToType(typeDeclaration, directiveTrivia) == false)
            {
                continue;
            }

            if (directiveTrivia.IsKind(SyntaxKind.RegionDirectiveTrivia))
            {
                regionStack.Push(directiveTrivia);
            }
            else if (regionStack.Count > 1)
            {
                regionStack.Pop();
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
    protected static bool IsWithinRegion(MemberDeclarationSyntax memberDeclaration, IReadOnlyList<(SyntaxTrivia Region, SyntaxTrivia EndRegion)> regions)
    {
        return regions.Any(obj => memberDeclaration.SpanStart >= obj.Region.Span.End
                                  && memberDeclaration.Span.End <= obj.EndRegion.SpanStart);
    }

    /// <summary>
    /// Returns a numeric kind index for the given member, grouping related declaration types together
    /// </summary>
    /// <param name="memberDeclaration">Member declaration</param>
    /// <returns>Kind index</returns>
    protected static int GetMemberKind(MemberDeclarationSyntax memberDeclaration)
    {
        return memberDeclaration switch
               {
                   BaseTypeDeclarationSyntax or DelegateDeclarationSyntax => 0,
                   FieldDeclarationSyntax => 1,
                   ConstructorDeclarationSyntax => 2,
                   DestructorDeclarationSyntax => 3,
                   PropertyDeclarationSyntax => 4,
                   IndexerDeclarationSyntax => 5,
                   EventDeclarationSyntax or EventFieldDeclarationSyntax => 6,
                   MethodDeclarationSyntax => 7,
                   OperatorDeclarationSyntax or ConversionOperatorDeclarationSyntax => 8,
                   _ => -1
               };
    }

    /// <summary>
    /// Determines whether a diagnostic should be reported for the given members and regions
    /// </summary>
    /// <param name="relevantMembers">Relevant member declarations of the type</param>
    /// <param name="regions">Top-level region pairs of the type</param>
    /// <returns><see langword="true"/> if a diagnostic should be reported</returns>
    protected abstract bool ShouldReport(MemberDeclarationSyntax[] relevantMembers, IReadOnlyList<(SyntaxTrivia Region, SyntaxTrivia EndRegion)> regions);

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

        if (ShouldReport(relevantMembers, regions))
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