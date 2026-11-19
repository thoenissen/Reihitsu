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
        : base(DiagnosticId, DiagnosticCategory.Organization)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyze the declaration, and for a type declaration the defining parts of its partial constructors, which no
    /// syntax-node action receives on their own (RH7106-001, SHARED-DIAG-004)
    /// </summary>
    /// <param name="context">Context</param>
    private void OnDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not MemberDeclarationSyntax memberDeclaration)
        {
            return;
        }

        foreach (var judgedDeclaration in DeclarationModifierUtilities.GetDeclarationsJudgedWith(memberDeclaration))
        {
            AnalyzeModifiers(context, judgedDeclaration);
        }
    }

    /// <summary>
    /// Analyze the modifiers of one declaration
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="memberDeclaration">Declaration</param>
    private void AnalyzeModifiers(SyntaxNodeAnalysisContext context, MemberDeclarationSyntax memberDeclaration)
    {
        // The modifiers are judged as the analyzed build compiles them; a conditional directive between them does not
        // keep the declaration from being reported, and the fix withholds itself there (RH7106-001, SHARED-DIAG-008)
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
                                         SyntaxKind.UnionDeclaration,
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