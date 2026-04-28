using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0384: Generic type constraints should be on their own line with proper indentation
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0384GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzer : DiagnosticAnalyzerBase<RH0384GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0384";

    /// <summary>
    /// Indentation size for generic constraint clauses
    /// </summary>
    private const int IndentationSize = 4;

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0384GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0384Title), nameof(AnalyzerResources.RH0384MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzes constraint clauses for a declaration
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="declaration">Declaration</param>
    /// <param name="constraintClauses">Constraint clauses</param>
    private void AnalyzeConstraintClauses(SyntaxNodeAnalysisContext context, SyntaxNode declaration, SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses)
    {
        if (constraintClauses.Count == 0)
        {
            return;
        }

        var expectedColumn = declaration.GetFirstToken().GetLocation().GetLineSpan().StartLinePosition.Character + IndentationSize;

        foreach (var constraintClause in constraintClauses)
        {
            var whereLineSpan = constraintClause.WhereKeyword.GetLocation().GetLineSpan();
            var previousToken = constraintClause.WhereKeyword.GetPreviousToken();

            if (previousToken.RawKind == 0)
            {
                continue;
            }

            var previousLineSpan = previousToken.GetLocation().GetLineSpan();
            var isOnOwnLine = whereLineSpan.StartLinePosition.Line > previousLineSpan.EndLinePosition.Line;
            var hasExpectedIndentation = whereLineSpan.StartLinePosition.Character == expectedColumn;

            if (isOnOwnLine == false
                || hasExpectedIndentation == false)
            {
                context.ReportDiagnostic(CreateDiagnostic(constraintClause.WhereKeyword.GetLocation()));

                break;
            }
        }
    }

    /// <summary>
    /// Analyzes class declarations
    /// </summary>
    /// <param name="context">Context</param>
    private void OnClassDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ClassDeclarationSyntax classDeclaration)
        {
            return;
        }

        AnalyzeConstraintClauses(context, classDeclaration, classDeclaration.ConstraintClauses);
    }

    /// <summary>
    /// Analyzes delegate declarations
    /// </summary>
    /// <param name="context">Context</param>
    private void OnDelegateDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not DelegateDeclarationSyntax delegateDeclaration)
        {
            return;
        }

        AnalyzeConstraintClauses(context, delegateDeclaration, delegateDeclaration.ConstraintClauses);
    }

    /// <summary>
    /// Analyzes interface declarations
    /// </summary>
    /// <param name="context">Context</param>
    private void OnInterfaceDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not InterfaceDeclarationSyntax interfaceDeclaration)
        {
            return;
        }

        AnalyzeConstraintClauses(context, interfaceDeclaration, interfaceDeclaration.ConstraintClauses);
    }

    /// <summary>
    /// Analyzes local function statements
    /// </summary>
    /// <param name="context">Context</param>
    private void OnLocalFunctionStatement(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not LocalFunctionStatementSyntax localFunctionStatement)
        {
            return;
        }

        AnalyzeConstraintClauses(context, localFunctionStatement, localFunctionStatement.ConstraintClauses);
    }

    /// <summary>
    /// Analyzes method declarations
    /// </summary>
    /// <param name="context">Context</param>
    private void OnMethodDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not MethodDeclarationSyntax methodDeclaration)
        {
            return;
        }

        AnalyzeConstraintClauses(context, methodDeclaration, methodDeclaration.ConstraintClauses);
    }

    /// <summary>
    /// Analyzes record declarations
    /// </summary>
    /// <param name="context">Context</param>
    private void OnRecordDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not RecordDeclarationSyntax recordDeclaration)
        {
            return;
        }

        AnalyzeConstraintClauses(context, recordDeclaration, recordDeclaration.ConstraintClauses);
    }

    /// <summary>
    /// Analyzes struct declarations
    /// </summary>
    /// <param name="context">Context</param>
    private void OnStructDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not StructDeclarationSyntax structDeclaration)
        {
            return;
        }

        AnalyzeConstraintClauses(context, structDeclaration, structDeclaration.ConstraintClauses);
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnClassDeclaration, SyntaxKind.ClassDeclaration);
        context.RegisterSyntaxNodeAction(OnStructDeclaration, SyntaxKind.StructDeclaration);
        context.RegisterSyntaxNodeAction(OnInterfaceDeclaration, SyntaxKind.InterfaceDeclaration);
        context.RegisterSyntaxNodeAction(OnRecordDeclaration, SyntaxKind.RecordDeclaration);
        context.RegisterSyntaxNodeAction(OnMethodDeclaration, SyntaxKind.MethodDeclaration);
        context.RegisterSyntaxNodeAction(OnDelegateDeclaration, SyntaxKind.DelegateDeclaration);
        context.RegisterSyntaxNodeAction(OnLocalFunctionStatement, SyntaxKind.LocalFunctionStatement);
    }

    #endregion // DiagnosticAnalyzer
}