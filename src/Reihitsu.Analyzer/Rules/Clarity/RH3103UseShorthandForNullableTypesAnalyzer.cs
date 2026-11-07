using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Clarity;

/// <summary>
/// RH3103: Use shorthand for nullable types
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH3103UseShorthandForNullableTypesAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH3103";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH3103UseShorthandForNullableTypesAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Clarity, nameof(AnalyzerResources.RH3103Title), nameof(AnalyzerResources.RH3103MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determine whether the node should be skipped because <c>T?</c> is not valid C# at its position: as the argument
    /// of <c>nameof</c>, as the receiver of a member access, as the type of a <c>using static</c> directive, and as the
    /// target of a using alias when the code is compiled with C# 11 or earlier. A generic name that is a type argument
    /// stands where <c>T?</c> is valid and is not skipped
    /// </summary>
    /// <param name="genericName">Generic name</param>
    /// <returns><see langword="true"/> if the node should be skipped</returns>
    private static bool ShouldSkip(GenericNameSyntax genericName)
    {
        SyntaxNode nullableType = genericName;

        while (nullableType.Parent is QualifiedNameSyntax or AliasQualifiedNameSyntax
               || (nullableType.Parent is MemberAccessExpressionSyntax qualifyingAccess
                   && qualifyingAccess.Name == nullableType))
        {
            nullableType = nullableType.Parent;
        }

        return nullableType.Parent switch
               {
                   ArgumentSyntax
                   {
                       Parent: ArgumentListSyntax
                       {
                           Parent: InvocationExpressionSyntax
                           {
                               Expression: IdentifierNameSyntax { Identifier.Text: "nameof" }
                           }
                       }
                   } => true,
                   MemberAccessExpressionSyntax memberAccessExpression => memberAccessExpression.Expression == nullableType,
                   UsingDirectiveSyntax usingDirective => usingDirective.StaticKeyword.IsKind(SyntaxKind.StaticKeyword)
                                                          || (usingDirective.Alias != null
                                                              && genericName.SyntaxTree.Options is CSharpParseOptions { LanguageVersion: < LanguageVersion.CSharp12 }),
                   _ => false
               };
    }

    /// <summary>
    /// Analyze all matching generic names
    /// </summary>
    /// <param name="context">Context</param>
    private void OnGenericName(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not GenericNameSyntax genericName
            || genericName.TypeArgumentList.Arguments.Count != 1
            || genericName.TypeArgumentList.Arguments[0].IsMissing
            || genericName.TypeArgumentList.Arguments[0] is OmittedTypeArgumentSyntax
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