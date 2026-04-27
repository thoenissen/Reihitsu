using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0384: Generic type constraints should be on their own line with proper indentation.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0384GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzer : DiagnosticAnalyzerBase<RH0384GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0384";

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
    /// Analyzes type parameter constraint clauses.
    /// </summary>
    /// <param name="context">Context</param>
    private void OnTypeParameterConstraintClause(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not TypeParameterConstraintClauseSyntax constraintClause
            || TryGetExpectedColumn(constraintClause, out var expectedColumn) == false)
        {
            return;
        }

        var whereLineSpan = constraintClause.WhereKeyword.GetLocation().GetLineSpan();
        var previousToken = constraintClause.WhereKeyword.GetPreviousToken();

        if (previousToken.RawKind == 0)
        {
            return;
        }

        var previousLineSpan = previousToken.GetLocation().GetLineSpan();
        var isOnOwnLine = whereLineSpan.StartLinePosition.Line > previousLineSpan.EndLinePosition.Line;
        var hasExpectedIndentation = whereLineSpan.StartLinePosition.Character == expectedColumn;

        if (isOnOwnLine == false
            || hasExpectedIndentation == false)
        {
            context.ReportDiagnostic(CreateDiagnostic(constraintClause.WhereKeyword.GetLocation()));
        }
    }

    /// <summary>
    /// Gets the expected indentation column for a generic constraint clause.
    /// </summary>
    /// <param name="constraintClause">Constraint clause</param>
    /// <param name="expectedColumn">Expected column</param>
    /// <returns><see langword="true"/> if the clause belongs to a supported declaration; otherwise, <see langword="false"/>.</returns>
    private static bool TryGetExpectedColumn(TypeParameterConstraintClauseSyntax constraintClause, out int expectedColumn)
    {
        SyntaxNode declaration = constraintClause.Parent switch
        {
            ClassDeclarationSyntax classDeclaration => classDeclaration,
            StructDeclarationSyntax structDeclaration => structDeclaration,
            InterfaceDeclarationSyntax interfaceDeclaration => interfaceDeclaration,
            RecordDeclarationSyntax recordDeclaration => recordDeclaration,
            MethodDeclarationSyntax methodDeclaration => methodDeclaration,
            DelegateDeclarationSyntax delegateDeclaration => delegateDeclaration,
            LocalFunctionStatementSyntax localFunctionStatement => localFunctionStatement,
            _ => null
        };

        if (declaration == null)
        {
            expectedColumn = 0;
            return false;
        }

        expectedColumn = declaration.GetFirstToken().GetLocation().GetLineSpan().StartLinePosition.Character + 4;

        return true;
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnTypeParameterConstraintClause, SyntaxKind.TypeParameterConstraintClause);
    }

    #endregion // DiagnosticAnalyzer
}
