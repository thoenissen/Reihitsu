using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Design;

/// <summary>
/// RH0104: Access modifier must be declared.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0104AccessModifierMustBeDeclaredAnalyzer : DiagnosticAnalyzerBase<RH0104AccessModifierMustBeDeclaredAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0104";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0104AccessModifierMustBeDeclaredAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Design, nameof(AnalyzerResources.RH0104Title), nameof(AnalyzerResources.RH0104MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determine whether the declaration should be skipped.
    /// </summary>
    /// <param name="memberDeclaration">Declaration</param>
    /// <returns><see langword="true"/> if the declaration should be skipped</returns>
    private static bool ShouldSkip(MemberDeclarationSyntax memberDeclaration)
    {
        if (memberDeclaration.Ancestors().OfType<InterfaceDeclarationSyntax>().Any())
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
                   _ => false
               };
    }

    /// <summary>
    /// Get the diagnostic location.
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

        context.ReportDiagnostic(CreateDiagnostic(GetDiagnosticLocation(memberDeclaration)));
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
    }

    #endregion // DiagnosticAnalyzer
}