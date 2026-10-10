using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Organization;

/// <summary>
/// RH7106: Protected must come before internal
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH7106ProtectedMustComeBeforeInternalAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH7106";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH7106ProtectedMustComeBeforeInternalAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Organization, nameof(AnalyzerResources.RH7106Title), nameof(AnalyzerResources.RH7106MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyze the declaration
    /// </summary>
    /// <param name="context">Context</param>
    private void OnDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not MemberDeclarationSyntax memberDeclaration)
        {
            return;
        }

        var modifiers = DeclarationModifierUtilities.GetModifiers(memberDeclaration);

        if (ModifierOrderingUtilities.TryGetRh7106Violation(modifiers, out var diagnosticToken))
        {
            context.ReportDiagnostic(CreateDiagnostic(diagnosticToken.GetLocation()));
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
                                         SyntaxKind.RecordDeclaration,
                                         SyntaxKind.RecordStructDeclaration,
                                         SyntaxKind.DelegateDeclaration,
                                         SyntaxKind.FieldDeclaration,
                                         SyntaxKind.ConstructorDeclaration,
                                         SyntaxKind.PropertyDeclaration,
                                         SyntaxKind.EventDeclaration,
                                         SyntaxKind.EventFieldDeclaration,
                                         SyntaxKind.MethodDeclaration,
                                         SyntaxKind.IndexerDeclaration,
                                         SyntaxKind.OperatorDeclaration,
                                         SyntaxKind.ConversionOperatorDeclaration);
    }

    #endregion // DiagnosticAnalyzer
}