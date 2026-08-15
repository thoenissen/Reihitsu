using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Performance;

/// <summary>
/// RH1001: Types used as keys must implement equality members
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH1001TypesUsedAsKeysMustImplementEqualityMembersAnalyzer : StructEqualityPerformanceAnalyzerBase
{
    #region Fields

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH1001";

    /// <summary>
    /// Relevant collection types
    /// </summary>
    private static readonly string[] _collectionTypes = [
                                                            "System.Collections.Generic.Dictionary`2",
                                                            "System.Collections.Generic.HashSet`1",
                                                            "System.Collections.Concurrent.ConcurrentDictionary`2",
                                                            "System.Collections.Immutable.ImmutableDictionary`2",
                                                            "System.Collections.Immutable.ImmutableHashSet`1",
                                                            "System.Collections.Frozen.FrozenDictionary`2",
                                                            "System.Collections.Frozen.FrozenSet`1"
                                                        ];

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH1001TypesUsedAsKeysMustImplementEqualityMembersAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Performance, nameof(AnalyzerResources.RH1001Title), nameof(AnalyzerResources.RH1001MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Is the type a relevant collection type?
    /// </summary>
    /// <param name="compilation">Compilation</param>
    /// <param name="genericType">Generic collection type</param>
    /// <returns>Is the type check relevant?</returns>
    private static bool IsRelevantCollectionType(Compilation compilation, INamedTypeSymbol genericType)
    {
        if (genericType.IsGenericType == false)
        {
            return false;
        }

        var unboundGenericType = genericType.ConstructUnboundGenericType();

        foreach (var collectionType in _collectionTypes)
        {
            var collectionTypeSymbol = compilation.GetTypeByMetadataName(collectionType)?.ConstructUnboundGenericType();

            if (collectionTypeSymbol != null
                && SymbolEqualityComparer.Default.Equals(unboundGenericType, collectionTypeSymbol))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether a collection type should produce the diagnostic
    /// </summary>
    /// <param name="compilation">Compilation</param>
    /// <param name="collectionType">Collection type</param>
    /// <returns><see langword="true"/> if the key is a struct without equality members</returns>
    private static bool ShouldReportDiagnostic(Compilation compilation, INamedTypeSymbol collectionType)
    {
        // Only the key position is hashed. For every recognized collection type the key is the first
        // type argument (dictionaries: key, value; sets: element), so restrict the check to index 0.
        var keyType = collectionType.TypeArguments[0];

        return keyType.TypeKind == TypeKind.Structure
               && AreEqualityMembersImplemented(compilation, keyType) == false;
    }

    /// <summary>
    /// Determines whether a type reference is the collection type being explicitly constructed
    /// </summary>
    /// <param name="semanticModel">Semantic model</param>
    /// <param name="typeReference">Collection type reference</param>
    /// <param name="collectionType">Bound collection type</param>
    /// <returns><see langword="true"/> if the type reference is the explicitly constructed type</returns>
    private static bool IsExplicitlyConstructedType(SemanticModel semanticModel, TypeSyntax typeReference, INamedTypeSymbol collectionType)
    {
        var objectCreation = typeReference.FirstAncestorOrSelf<ObjectCreationExpressionSyntax>();

        return objectCreation != null
               && objectCreation.Type.Span.Contains(typeReference.Span)
               && semanticModel.GetTypeInfo(objectCreation).Type is INamedTypeSymbol createdType
               && SymbolEqualityComparer.Default.Equals(createdType, collectionType);
    }

    /// <summary>
    /// Determines whether every object creation directly initialized through a generic type reference supplies
    /// an explicit custom equality comparer
    /// </summary>
    /// <param name="semanticModel">Semantic model</param>
    /// <param name="typeReference">Collection type reference</param>
    /// <param name="collectionType">Bound collection type</param>
    /// <returns><see langword="true"/> if all associated creations supply custom comparers</returns>
    private static bool AreInitializerCreationsUsingExplicitEqualityComparers(SemanticModel semanticModel, TypeSyntax typeReference, INamedTypeSymbol collectionType)
    {
        if (typeReference.FirstAncestorOrSelf<VariableDeclarationSyntax>() is { } variableDeclaration
            && variableDeclaration.Type.Span.Contains(typeReference.Span)
            && variableDeclaration.Variables.Count > 0)
        {
            return variableDeclaration.Variables.All(variable => HasExplicitEqualityComparerInitializer(semanticModel, variable.Initializer?.Value, collectionType));
        }

        if (typeReference.FirstAncestorOrSelf<PropertyDeclarationSyntax>() is { } propertyDeclaration
            && propertyDeclaration.Type.Span.Contains(typeReference.Span))
        {
            return HasExplicitEqualityComparerInitializer(semanticModel, propertyDeclaration.Initializer?.Value, collectionType);
        }

        if (typeReference.FirstAncestorOrSelf<CastExpressionSyntax>() is { } castExpression
            && castExpression.Type.Span.Contains(typeReference.Span))
        {
            return HasExplicitEqualityComparerInitializer(semanticModel, castExpression.Expression, collectionType);
        }

        return false;
    }

    /// <summary>
    /// Determines whether an initializer is an explicit or implicit object creation supplying a custom comparer
    /// </summary>
    /// <param name="semanticModel">Semantic model</param>
    /// <param name="initializer">Initializer expression</param>
    /// <param name="expectedType">Expected collection type</param>
    /// <returns><see langword="true"/> if the initializer construction receives a custom comparer</returns>
    private static bool HasExplicitEqualityComparerInitializer(SemanticModel semanticModel, ExpressionSyntax initializer, INamedTypeSymbol expectedType)
    {
        switch (initializer)
        {
            case ParenthesizedExpressionSyntax parenthesized:
                {
                    return HasExplicitEqualityComparerInitializer(semanticModel, parenthesized.Expression, expectedType);
                }

            case PostfixUnaryExpressionSyntax postfixUnary when postfixUnary.IsKind(SyntaxKind.SuppressNullableWarningExpression):
                {
                    return HasExplicitEqualityComparerInitializer(semanticModel, postfixUnary.Operand, expectedType);
                }

            case CastExpressionSyntax castExpression when semanticModel.GetOperation(castExpression) is IConversionOperation
                                                                                                     {
                                                                                                         Conversion.IsUserDefined: false
                                                                                                     }:
                {
                    return HasExplicitEqualityComparerInitializer(semanticModel, castExpression.Expression, expectedType);
                }

            case ConditionalExpressionSyntax conditional:
                {
                    if (semanticModel.GetConstantValue(conditional.Condition) is { HasValue: true, Value: bool conditionValue })
                    {
                        return HasExplicitEqualityComparerInitializer(semanticModel,
                                                                      conditionValue
                                                                          ? conditional.WhenTrue
                                                                          : conditional.WhenFalse,
                                                                      expectedType);
                    }

                    return HasExplicitEqualityComparerInitializer(semanticModel, conditional.WhenTrue, expectedType)
                           && HasExplicitEqualityComparerInitializer(semanticModel, conditional.WhenFalse, expectedType);
                }

            case SwitchExpressionSyntax { Arms.Count: > 0 } switchExpression:
                {
                    return AreSwitchExpressionArmsUsingExplicitEqualityComparers(semanticModel, switchExpression, expectedType);
                }

            case ObjectCreationExpressionSyntax { ArgumentList: not null } objectCreation:
                {
                    return HasExplicitEqualityComparer(semanticModel, objectCreation, objectCreation.ArgumentList, expectedType);
                }

            case ImplicitObjectCreationExpressionSyntax implicitCreation:
                {
                    return HasExplicitEqualityComparer(semanticModel, implicitCreation, implicitCreation.ArgumentList, expectedType);
                }

            default:
                {
                    return false;
                }
        }
    }

    /// <summary>
    /// Determines whether every switch-expression arm constructs the expected collection with a custom comparer
    /// </summary>
    /// <param name="semanticModel">Semantic model</param>
    /// <param name="switchExpression">Switch expression</param>
    /// <param name="expectedType">Expected collection type</param>
    /// <returns><see langword="true"/> if every arm constructs the collection with a custom comparer</returns>
    private static bool AreSwitchExpressionArmsUsingExplicitEqualityComparers(SemanticModel semanticModel, SwitchExpressionSyntax switchExpression, INamedTypeSymbol expectedType)
    {
        return switchExpression.Arms.All(arm => HasExplicitEqualityComparerInitializer(semanticModel, arm.Expression, expectedType));
    }

    /// <summary>
    /// Determines whether an object creation supplies an explicit custom equality comparer
    /// </summary>
    /// <param name="semanticModel">Semantic model</param>
    /// <param name="objectCreation">Object creation</param>
    /// <param name="argumentList">Argument list</param>
    /// <param name="expectedType">Expected collection type</param>
    /// <returns><see langword="true"/> if the bound constructor receives a custom comparer</returns>
    private static bool HasExplicitEqualityComparer(SemanticModel semanticModel, ExpressionSyntax objectCreation, ArgumentListSyntax argumentList, INamedTypeSymbol expectedType)
    {
        return semanticModel.GetTypeInfo(objectCreation).Type is INamedTypeSymbol createdType
               && SymbolEqualityComparer.Default.Equals(createdType, expectedType)
               && semanticModel.GetSymbolInfo(objectCreation).Symbol is IMethodSymbol constructor
               && EqualityComparerArgumentUtilities.HasExplicitEqualityComparerArgument(semanticModel, argumentList, constructor.Parameters);
    }

    /// <summary>
    /// Determines whether a creation is inside a collection-typed variable, property, or cast initializer whose
    /// type reference owns analysis of all initializer branches
    /// </summary>
    /// <param name="semanticModel">Semantic model</param>
    /// <param name="objectCreation">Object creation</param>
    /// <param name="collectionType">Created collection type</param>
    /// <returns><see langword="true"/> if a target type reference owns analysis of the creation</returns>
    private static bool IsAnalyzedByTargetTypeReference(SemanticModel semanticModel, ExpressionSyntax objectCreation, INamedTypeSymbol collectionType)
    {
        SyntaxNode current = objectCreation;

        while (current.Parent != null)
        {
            switch (current.Parent)
            {
                case ParenthesizedExpressionSyntax parenthesized when parenthesized.Expression == current:
                    {
                        current = parenthesized;
                    }
                    break;

                case PostfixUnaryExpressionSyntax postfixUnary when postfixUnary.Operand == current
                                                                    && postfixUnary.IsKind(SyntaxKind.SuppressNullableWarningExpression):
                    {
                        current = postfixUnary;
                    }
                    break;

                case ConditionalExpressionSyntax conditional when conditional.WhenTrue == current
                                                                  || conditional.WhenFalse == current:
                    {
                        current = conditional;
                    }
                    break;

                case SwitchExpressionArmSyntax switchArm when switchArm.Expression == current:
                    {
                        current = switchArm;
                    }
                    break;

                case SwitchExpressionSyntax switchExpression when current is SwitchExpressionArmSyntax:
                    {
                        current = switchExpression;
                    }
                    break;

                case CastExpressionSyntax castExpression when castExpression.Expression == current:
                    {
                        return semanticModel.GetTypeInfo(castExpression.Type).Type is INamedTypeSymbol castType
                               && SymbolEqualityComparer.Default.Equals(castType, collectionType);
                    }

                case EqualsValueClauseSyntax equalsValue when equalsValue.Value == current:
                    {
                        var targetType = equalsValue.Parent switch
                                         {
                                             VariableDeclaratorSyntax
                                             {
                                                 Parent: VariableDeclarationSyntax variableDeclaration
                                             } => variableDeclaration.Type,
                                             PropertyDeclarationSyntax propertyDeclaration => propertyDeclaration.Type,
                                             _ => null
                                         };

                        return targetType != null
                               && semanticModel.GetTypeInfo(targetType).Type is INamedTypeSymbol targetTypeSymbol
                               && SymbolEqualityComparer.Default.Equals(targetTypeSymbol, collectionType);
                    }

                default:
                    {
                        return false;
                    }
            }
        }

        return false;
    }

    /// <summary>
    /// Gets the diagnostic location for an explicit object creation
    /// </summary>
    /// <param name="semanticModel">Semantic model</param>
    /// <param name="objectCreation">Object creation</param>
    /// <param name="collectionType">Created collection type</param>
    /// <returns>Diagnostic location</returns>
    private static Location GetObjectCreationDiagnosticLocation(SemanticModel semanticModel, ObjectCreationExpressionSyntax objectCreation, INamedTypeSymbol collectionType)
    {
        foreach (var node in objectCreation.Type.DescendantNodesAndSelf())
        {
            if (node is GenericNameSyntax genericName
                && semanticModel.GetSymbolInfo(genericName).Symbol is INamedTypeSymbol typeSymbol
                && SymbolEqualityComparer.Default.Equals(typeSymbol, collectionType))
            {
                return genericName.TypeArgumentList.Arguments.Count > 0
                           ? genericName.TypeArgumentList.Arguments[0].GetLocation()
                           : genericName.GetLocation();
            }
        }

        return objectCreation.Type.GetLocation();
    }

    /// <summary>
    /// Analyzing all <see cref="SyntaxKind.GenericName"/> occurrences
    /// </summary>
    /// <param name="context">Context</param>
    private void OnGenericName(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not GenericNameSyntax genericName)
        {
            return;
        }

        if (context.SemanticModel.GetSymbolInfo(genericName).Symbol is not INamedTypeSymbol namedTypeSymbol)
        {
            return;
        }

        if (IsRelevantCollectionType(context.Compilation, namedTypeSymbol) == false)
        {
            return;
        }

        if (genericName.FirstAncestorOrSelf<UsingDirectiveSyntax>() is { Alias: not null })
        {
            return;
        }

        if (IsExplicitlyConstructedType(context.SemanticModel, genericName, namedTypeSymbol))
        {
            return;
        }

        if (AreInitializerCreationsUsingExplicitEqualityComparers(context.SemanticModel, genericName, namedTypeSymbol))
        {
            return;
        }

        if (ShouldReportDiagnostic(context.Compilation, namedTypeSymbol))
        {
            var location = genericName.TypeArgumentList.Arguments.Count > 0
                               ? genericName.TypeArgumentList.Arguments[0].GetLocation()
                               : genericName.GetLocation();

            context.ReportDiagnostic(CreateDiagnostic(location));
        }
    }

    /// <summary>
    /// Analyzing collection type references made through aliases
    /// </summary>
    /// <param name="context">Context</param>
    private void OnIdentifierName(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not IdentifierNameSyntax identifierName
            || identifierName.FirstAncestorOrSelf<UsingDirectiveSyntax>() != null
            || context.SemanticModel.GetAliasInfo(identifierName)?.Target is not ITypeSymbol aliasedType)
        {
            return;
        }

        AnalyzeAliasedTypeReference(context, identifierName, aliasedType);
    }

    /// <summary>
    /// Analyzes every collection type represented by an alias use, including collections nested inside another
    /// generic type or array
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="identifierName">Alias use</param>
    /// <param name="aliasedType">Type represented by the alias</param>
    private void AnalyzeAliasedTypeReference(SyntaxNodeAnalysisContext context, IdentifierNameSyntax identifierName, ITypeSymbol aliasedType)
    {
        if (aliasedType is IArrayTypeSymbol arrayType)
        {
            AnalyzeAliasedTypeReference(context, identifierName, arrayType.ElementType);

            return;
        }

        if (aliasedType is IPointerTypeSymbol pointerType)
        {
            AnalyzeAliasedTypeReference(context, identifierName, pointerType.PointedAtType);

            return;
        }

        if (aliasedType is not INamedTypeSymbol namedType)
        {
            return;
        }

        if (IsRelevantCollectionType(context.Compilation, namedType)
            && IsExplicitlyConstructedType(context.SemanticModel, identifierName, namedType) == false
            && AreInitializerCreationsUsingExplicitEqualityComparers(context.SemanticModel, identifierName, namedType) == false
            && ShouldReportDiagnostic(context.Compilation, namedType))
        {
            context.ReportDiagnostic(CreateDiagnostic(identifierName.GetLocation()));
        }

        foreach (var typeArgument in namedType.TypeArguments)
        {
            AnalyzeAliasedTypeReference(context, identifierName, typeArgument);
        }
    }

    /// <summary>
    /// Analyzing explicit object creations, including types referenced through aliases
    /// </summary>
    /// <param name="context">Context</param>
    private void OnObjectCreation(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ObjectCreationExpressionSyntax objectCreation
            || context.SemanticModel.GetTypeInfo(objectCreation).Type is not INamedTypeSymbol collectionType
            || IsRelevantCollectionType(context.Compilation, collectionType) == false
            || ShouldReportDiagnostic(context.Compilation, collectionType) == false)
        {
            return;
        }

        if (objectCreation.ArgumentList != null
            && HasExplicitEqualityComparer(context.SemanticModel, objectCreation, objectCreation.ArgumentList, collectionType))
        {
            return;
        }

        context.ReportDiagnostic(CreateDiagnostic(GetObjectCreationDiagnosticLocation(context.SemanticModel, objectCreation, collectionType)));
    }

    /// <summary>
    /// Analyzing implicit object creations whose target collection type is referenced through an alias
    /// </summary>
    /// <param name="context">Context</param>
    private void OnImplicitObjectCreation(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ImplicitObjectCreationExpressionSyntax objectCreation
            || context.SemanticModel.GetTypeInfo(objectCreation).Type is not INamedTypeSymbol collectionType
            || IsRelevantCollectionType(context.Compilation, collectionType) == false
            || ShouldReportDiagnostic(context.Compilation, collectionType) == false
            || IsAnalyzedByTargetTypeReference(context.SemanticModel, objectCreation, collectionType)
            || HasExplicitEqualityComparer(context.SemanticModel, objectCreation, objectCreation.ArgumentList, collectionType))
        {
            return;
        }

        context.ReportDiagnostic(CreateDiagnostic(objectCreation.NewKeyword.GetLocation()));
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnGenericName, SyntaxKind.GenericName);
        context.RegisterSyntaxNodeAction(OnIdentifierName, SyntaxKind.IdentifierName);
        context.RegisterSyntaxNodeAction(OnObjectCreation, SyntaxKind.ObjectCreationExpression);
        context.RegisterSyntaxNodeAction(OnImplicitObjectCreation, SyntaxKind.ImplicitObjectCreationExpression);
    }

    #endregion // DiagnosticAnalyzer
}