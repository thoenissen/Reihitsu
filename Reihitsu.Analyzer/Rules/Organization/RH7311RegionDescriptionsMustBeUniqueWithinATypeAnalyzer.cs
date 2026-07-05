using System;
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
/// RH7311: Region descriptions must be unique within a type
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH7311RegionDescriptionsMustBeUniqueWithinATypeAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH7311";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH7311RegionDescriptionsMustBeUniqueWithinATypeAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Organization, nameof(AnalyzerResources.RH7311Title), nameof(AnalyzerResources.RH7311MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

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

        if (regions.Count < 2)
        {
            return;
        }

        var seenDescriptions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var region in regions)
        {
            if (region.Region.GetStructure() is not RegionDirectiveTriviaSyntax regionDirective)
            {
                continue;
            }

            var description = RegionDirectiveUtilities.GetRegionDescription(regionDirective);

            if (string.IsNullOrEmpty(description))
            {
                continue;
            }

            if (seenDescriptions.Add(description) == false)
            {
                context.ReportDiagnostic(CreateDiagnostic(regionDirective.GetLocation(), description));
            }
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