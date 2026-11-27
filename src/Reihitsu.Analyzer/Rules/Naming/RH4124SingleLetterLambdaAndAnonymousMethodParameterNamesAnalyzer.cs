using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Naming;

/// <summary>
/// Analyzer for single-letter lambda parameter, anonymous method parameter, and query range variable names
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH4124SingleLetterLambdaAndAnonymousMethodParameterNamesAnalyzer : DiagnosticAnalyzerBase
{
    #region Fields

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH4124";

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH4124SingleLetterLambdaAndAnonymousMethodParameterNamesAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Naming)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzes a parameter
    /// </summary>
    /// <param name="context">Context</param>
    private void OnParameter(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is ParameterSyntax parameter
            && context.SemanticModel.GetDeclaredSymbol(parameter, context.CancellationToken) is IParameterSymbol
                                                                                             {
                                                                                                 ContainingSymbol: IMethodSymbol { MethodKind: MethodKind.AnonymousFunction },
                                                                                                 Name: var parameterName
                                                                                             }
            && SingleLetterIdentifierUtilities.HasSingleLetterName(parameterName))
        {
            context.ReportDiagnostic(CreateDiagnostic(parameter.Identifier.GetLocation()));
        }
    }

    /// <summary>
    /// Analyzes a query clause or query continuation that declares a range variable
    /// </summary>
    /// <param name="context">Context</param>
    private void OnRangeVariableDeclaration(SyntaxNodeAnalysisContext context)
    {
        var identifier = context.Node switch
                         {
                             FromClauseSyntax fromClause => fromClause.Identifier,
                             LetClauseSyntax letClause => letClause.Identifier,
                             JoinClauseSyntax joinClause => joinClause.Identifier,
                             JoinIntoClauseSyntax joinIntoClause => joinIntoClause.Identifier,
                             QueryContinuationSyntax queryContinuation => queryContinuation.Identifier,
                             _ => default
                         };

        if (SingleLetterIdentifierUtilities.HasSingleLetterName(identifier.ValueText))
        {
            context.ReportDiagnostic(CreateDiagnostic(identifier.GetLocation()));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnParameter, SyntaxKind.Parameter);
        context.RegisterSyntaxNodeAction(OnRangeVariableDeclaration,
                                         SyntaxKind.FromClause,
                                         SyntaxKind.LetClause,
                                         SyntaxKind.JoinClause,
                                         SyntaxKind.JoinIntoClause,
                                         SyntaxKind.QueryContinuation);
    }

    #endregion // DiagnosticAnalyzer
}