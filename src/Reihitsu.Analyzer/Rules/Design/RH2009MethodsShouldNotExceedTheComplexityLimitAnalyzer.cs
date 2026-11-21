using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Design;

/// <summary>
/// RH2009: Methods should not exceed the complexity limit
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH2009MethodsShouldNotExceedTheComplexityLimitAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH2009";

    /// <summary>
    /// Default limit of the cognitive complexity of a member's body
    /// </summary>
    private const int DefaultLimit = 15;

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH2009MethodsShouldNotExceedTheComplexityLimitAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Design)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Start of the compilation
    /// </summary>
    /// <param name="context">Context</param>
    private void OnCompilationStart(CompilationStartAnalysisContext context)
    {
        var limit = ConfiguredLimitUtilities.ResolveLimit(ConfiguredLimitUtilities.GetDesignSection(context.Options),
                                                          design => design.MaximumMethodComplexity,
                                                          DefaultLimit);

        context.RegisterSyntaxNodeAction(analysisContext => OnMember(analysisContext, limit), MemberBodyUtilities.MemberKinds);
    }

    /// <summary>
    /// Analyzing a judged member
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="limit">Limit that applies</param>
    private void OnMember(SyntaxNodeAnalysisContext context, int limit)
    {
        if (MemberBodyUtilities.TryGetBody(context.Node, out var body, out var expressionBody, out var nameToken) == false)
        {
            return;
        }

        var member = context.Node is MethodDeclarationSyntax or LocalFunctionStatementSyntax
                         ? context.SemanticModel.GetDeclaredSymbol(context.Node, context.CancellationToken) as IMethodSymbol
                         : null;
        var complexity = CognitiveComplexityWalker.Compute((SyntaxNode)body ?? expressionBody.Expression,
                                                           context.SemanticModel,
                                                           member,
                                                           context.CancellationToken,
                                                           out var hasNestedStructure);

        if (hasNestedStructure
            && ConfiguredLimitUtilities.ExceedsLimit(complexity, limit))
        {
            context.ReportDiagnostic(CreateDiagnostic(nameToken.GetLocation(), complexity, limit));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterCompilationStartAction(OnCompilationStart);
    }

    #endregion // DiagnosticAnalyzer
}