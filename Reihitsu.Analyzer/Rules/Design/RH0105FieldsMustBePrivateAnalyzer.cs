using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Design;

/// <summary>
/// RH0105: Fields must be private.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0105FieldsMustBePrivateAnalyzer : DiagnosticAnalyzerBase<RH0105FieldsMustBePrivateAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0105";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0105FieldsMustBePrivateAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Design, nameof(AnalyzerResources.RH0105Title), nameof(AnalyzerResources.RH0105MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determine whether the field declaration should be skipped.
    /// </summary>
    /// <param name="fieldDeclaration">Field declaration</param>
    /// <returns><see langword="true"/> if the declaration should be skipped</returns>
    private static bool ShouldSkip(FieldDeclarationSyntax fieldDeclaration)
    {
        if (fieldDeclaration.Parent is not ClassDeclarationSyntax
            && fieldDeclaration.Parent is not RecordDeclarationSyntax)
        {
            return true;
        }

        if (fieldDeclaration.Modifiers.Any(SyntaxKind.ConstKeyword))
        {
            return true;
        }

        if (fieldDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword)
            && fieldDeclaration.Modifiers.Any(SyntaxKind.ReadOnlyKeyword))
        {
            return true;
        }

        return DeclarationModifierUtilities.HasAccessibilityModifier(fieldDeclaration.Modifiers) == false;
    }

    /// <summary>
    /// Determine whether the modifiers declare only <see langword="private"/> accessibility.
    /// </summary>
    /// <param name="modifiers">Modifiers</param>
    /// <returns><see langword="true"/> if the field is explicitly private</returns>
    private static bool HasOnlyPrivateAccessibility(SyntaxTokenList modifiers)
    {
        return modifiers.Any(SyntaxKind.PrivateKeyword)
               && modifiers.Any(SyntaxKind.PublicKeyword) == false
               && modifiers.Any(SyntaxKind.ProtectedKeyword) == false
               && modifiers.Any(SyntaxKind.InternalKeyword) == false;
    }

    /// <summary>
    /// Analyzing all <see cref="SyntaxKind.FieldDeclaration"/> nodes
    /// </summary>
    /// <param name="context">Context</param>
    private void OnFieldDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not FieldDeclarationSyntax fieldDeclaration)
        {
            return;
        }

        if (ShouldSkip(fieldDeclaration))
        {
            return;
        }

        if (HasOnlyPrivateAccessibility(fieldDeclaration.Modifiers))
        {
            return;
        }

        context.ReportDiagnostic(CreateDiagnostic(fieldDeclaration.Declaration.Variables[0].Identifier.GetLocation()));
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnFieldDeclaration, SyntaxKind.FieldDeclaration);
    }

    #endregion // DiagnosticAnalyzer
}