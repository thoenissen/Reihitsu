using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Organization;

/// <summary>
/// RH7104: Partial elements must declare access modifier
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH7104PartialElementsMustDeclareAccessModifierAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH7104";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH7104PartialElementsMustDeclareAccessModifierAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Organization, nameof(AnalyzerResources.RH7104Title), nameof(AnalyzerResources.RH7104MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyze the declaration
    /// </summary>
    /// <param name="context">Context</param>
    private void OnTypeDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not TypeDeclarationSyntax typeDeclaration
            || typeDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword) == false
            || OrderingDeclarationUtilities.HasExplicitAccessibilityModifier(typeDeclaration.Modifiers))
        {
            return;
        }

        context.ReportDiagnostic(CreateDiagnostic(typeDeclaration.Identifier.GetLocation()));
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