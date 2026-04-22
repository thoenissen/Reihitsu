using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Clarity;

/// <summary>
/// RH0007: Use shorthand for nullable types.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0007UseShorthandForNullableTypesAnalyzer : DiagnosticAnalyzerBase<RH0007UseShorthandForNullableTypesAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0007";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0007UseShorthandForNullableTypesAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Clarity, nameof(AnalyzerResources.RH0007Title), nameof(AnalyzerResources.RH0007MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determine whether the node should be skipped.
    /// </summary>
    /// <param name="genericName">Generic name</param>
    /// <returns><see langword="true"/> if the node should be skipped</returns>
    private static bool ShouldSkip(GenericNameSyntax genericName)
    {
        return genericName.Ancestors().Any(ancestor => ancestor is TypeOfExpressionSyntax);
    }

    /// <summary>
    /// Analyze all matching generic names.
    /// </summary>
    /// <param name="context">Context</param>
    private void OnGenericName(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not GenericNameSyntax genericName
            || genericName.TypeArgumentList.Arguments.Count != 1
            || genericName.TypeArgumentList.Arguments[0].IsMissing
            || ShouldSkip(genericName))
        {
            return;
        }

        if (context.SemanticModel.GetTypeInfo(genericName, context.CancellationToken).Type is not INamedTypeSymbol namedTypeSymbol
            || namedTypeSymbol.OriginalDefinition.SpecialType != SpecialType.System_Nullable_T)
        {
            return;
        }

        context.ReportDiagnostic(CreateDiagnostic(genericName.Parent is QualifiedNameSyntax qualifiedName && qualifiedName.Right == genericName
                                                      ? qualifiedName.GetLocation()
                                                      : genericName.GetLocation()));
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnGenericName, SyntaxKind.GenericName);
    }

    #endregion // DiagnosticAnalyzer
}