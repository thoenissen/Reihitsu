using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Design;

/// <summary>
/// RH2004: Access modifier must be declared
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH2004AccessModifierMustBeDeclaredAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH2004";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH2004AccessModifierMustBeDeclaredAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Design, nameof(AnalyzerResources.RH2004Title), nameof(AnalyzerResources.RH2004MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determine whether the declaration should be skipped
    /// </summary>
    /// <param name="memberDeclaration">Declaration</param>
    /// <returns><see langword="true"/> if the declaration should be skipped</returns>
    private static bool ShouldSkip(MemberDeclarationSyntax memberDeclaration)
    {
        if (memberDeclaration is TypeDeclarationSyntax typeDeclaration
            && typeDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword))
        {
            return true;
        }

        if (memberDeclaration.Parent is InterfaceDeclarationSyntax
            && memberDeclaration is not BaseTypeDeclarationSyntax
            && memberDeclaration is not DelegateDeclarationSyntax)
        {
            return true;
        }

        return memberDeclaration switch
               {
                   MethodDeclarationSyntax methodDeclaration when methodDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword) => true,
                   ConstructorDeclarationSyntax constructorDeclaration when constructorDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword) => true,
                   MethodDeclarationSyntax { ExplicitInterfaceSpecifier: not null } => true,
                   PropertyDeclarationSyntax { ExplicitInterfaceSpecifier: not null } => true,
                   IndexerDeclarationSyntax { ExplicitInterfaceSpecifier: not null } => true,
                   EventDeclarationSyntax { ExplicitInterfaceSpecifier: not null } => true,
                   OperatorDeclarationSyntax { ExplicitInterfaceSpecifier: not null } => true,
                   ConversionOperatorDeclarationSyntax { ExplicitInterfaceSpecifier: not null } => true,
                   _ => false
               };
    }

    /// <summary>
    /// Checks whether a part of a partial member qualifies for the member's one diagnostic: it states no accessibility
    /// </summary>
    /// <param name="part">Part of a partial member</param>
    /// <returns><see langword="true"/> if the part states no accessibility</returns>
    private static bool StatesNoAccessibility(MemberDeclarationSyntax part)
    {
        return DeclarationModifierUtilities.HasAccessibilityModifier(part.Modifiers) == false;
    }

    /// <summary>
    /// Get the diagnostic location
    /// </summary>
    /// <param name="memberDeclaration">Declaration</param>
    /// <returns>Diagnostic location</returns>
    private static Location GetDiagnosticLocation(MemberDeclarationSyntax memberDeclaration)
    {
        return memberDeclaration switch
               {
                   BaseTypeDeclarationSyntax baseTypeDeclaration => baseTypeDeclaration.Identifier.GetLocation(),
                   DelegateDeclarationSyntax delegateDeclaration => delegateDeclaration.Identifier.GetLocation(),
                   MethodDeclarationSyntax methodDeclaration => methodDeclaration.Identifier.GetLocation(),
                   PropertyDeclarationSyntax propertyDeclaration => propertyDeclaration.Identifier.GetLocation(),
                   FieldDeclarationSyntax fieldDeclaration => fieldDeclaration.Declaration.Variables[0].Identifier.GetLocation(),
                   EventDeclarationSyntax eventDeclaration => eventDeclaration.Identifier.GetLocation(),
                   EventFieldDeclarationSyntax eventFieldDeclaration => eventFieldDeclaration.Declaration.Variables[0].Identifier.GetLocation(),
                   ConstructorDeclarationSyntax constructorDeclaration => constructorDeclaration.Identifier.GetLocation(),
                   IndexerDeclarationSyntax indexerDeclaration => indexerDeclaration.ThisKeyword.GetLocation(),
                   OperatorDeclarationSyntax operatorDeclaration => operatorDeclaration.OperatorToken.GetLocation(),
                   ConversionOperatorDeclarationSyntax conversionOperatorDeclaration => conversionOperatorDeclaration.OperatorKeyword.GetLocation(),
                   _ => memberDeclaration.GetLocation()
               };
    }

    /// <summary>
    /// Analyzing all supported declaration nodes
    /// </summary>
    /// <param name="context">Context</param>
    private void OnDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not MemberDeclarationSyntax memberDeclaration)
        {
            return;
        }

        if (ShouldSkip(memberDeclaration))
        {
            return;
        }

        var modifiers = DeclarationModifierUtilities.GetModifiers(memberDeclaration);

        if (DeclarationModifierUtilities.HasAccessibilityModifier(modifiers))
        {
            return;
        }

        // A member declared in a defining and an implementing part is reported from its containing type (OnNamedType)
        if (modifiers.Any(SyntaxKind.PartialKeyword)
            && PartialMemberUtilities.GetDeclaredSymbol(memberDeclaration, context.SemanticModel, context.CancellationToken) is { } symbol
            && PartialMemberUtilities.TryGetParts(symbol, context.CancellationToken, out _, out _))
        {
            return;
        }

        context.ReportDiagnostic(CreateDiagnostic(GetDiagnosticLocation(memberDeclaration)));
    }

    /// <summary>
    /// Reports each partial property, indexer, event, and constructor of a type once, on the part SHARED-DIAG-012 selects
    /// (SHARED-DIAG-004). The type sees every part of its members, also a part the analysis of declarations does not
    /// reach, so the choice is made here once per member from all of its parts
    /// </summary>
    /// <param name="context">Context</param>
    private void OnNamedType(SymbolAnalysisContext context)
    {
        if (context.Symbol is not INamedTypeSymbol typeSymbol)
        {
            return;
        }

        var definingParts = new HashSet<MemberDeclarationSyntax>();

        foreach (var member in typeSymbol.GetMembers())
        {
            if (PartialMemberUtilities.TryGetParts(member, context.CancellationToken, out var definingPart, out var implementingPart) == false
                || definingParts.Add(definingPart) == false
                || ShouldSkip(definingPart))
            {
                continue;
            }

            var reportedPart = PartialMemberUtilities.SelectReportedPart(definingPart, implementingPart, StatesNoAccessibility, context.Compilation, context.CancellationToken);

            if (reportedPart != null
                && StatesNoAccessibility(reportedPart))
            {
                context.ReportDiagnostic(CreateDiagnostic(GetDiagnosticLocation(reportedPart)));
            }
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnDeclaration,
                                         SyntaxKind.ClassDeclaration,
                                         SyntaxKind.StructDeclaration,
                                         SyntaxKind.InterfaceDeclaration,
                                         SyntaxKind.EnumDeclaration,
                                         SyntaxKind.RecordDeclaration,
                                         SyntaxKind.RecordStructDeclaration,
                                         SyntaxKind.DelegateDeclaration,
                                         SyntaxKind.MethodDeclaration,
                                         SyntaxKind.PropertyDeclaration,
                                         SyntaxKind.FieldDeclaration,
                                         SyntaxKind.EventDeclaration,
                                         SyntaxKind.EventFieldDeclaration,
                                         SyntaxKind.ConstructorDeclaration,
                                         SyntaxKind.IndexerDeclaration,
                                         SyntaxKind.OperatorDeclaration,
                                         SyntaxKind.ConversionOperatorDeclaration);
        context.RegisterSymbolAction(OnNamedType, SymbolKind.NamedType);
    }

    #endregion // DiagnosticAnalyzer
}