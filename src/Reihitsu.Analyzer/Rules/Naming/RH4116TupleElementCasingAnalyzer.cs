using System.Collections.Generic;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Naming;

/// <summary>
/// RH4116: Named tuple elements should be in PascalCase
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH4116TupleElementCasingAnalyzer : CasingAnalyzerBase
{
    #region Fields

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH4116";

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH4116TupleElementCasingAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Naming, SyntaxKind.TupleType, CasingUtilities.IsPascalCase)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Gets the outermost type that a type is part of: the type itself, or the type it stands in at any depth, as a type
    /// argument, an element type, a tuple element, or a function pointer parameter
    /// </summary>
    /// <param name="type">The type</param>
    /// <returns>The outermost type</returns>
    private static TypeSyntax GetOutermostType(TypeSyntax type)
    {
        var outermostType = type;

        for (var current = type.Parent; current is TypeSyntax or TupleElementSyntax or TypeArgumentListSyntax or FunctionPointerParameterSyntax or FunctionPointerParameterListSyntax; current = current.Parent)
        {
            if (current is TypeSyntax currentType)
            {
                outermostType = currentType;
            }
        }

        return outermostType;
    }

    /// <summary>
    /// Gets the members the compiler declares whose signature holds a type: the member a parameter belongs to, and for a
    /// record's positional parameter also the property and the <c>Deconstruct</c> method the compiler declares from it;
    /// each event, field, or local a variable declaration declares; and the member whose own type or return type it is
    /// </summary>
    /// <param name="type">The outermost type</param>
    /// <param name="semanticModel">Semantic model</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The declared members</returns>
    private static IEnumerable<ISymbol> GetDeclaredMembers(TypeSyntax type, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        switch (type.Parent)
        {
            case ParameterSyntax parameter when parameter.Type == type:
                {
                    if (semanticModel.GetDeclaredSymbol(parameter, cancellationToken) is IParameterSymbol parameterSymbol)
                    {
                        yield return parameterSymbol.ContainingSymbol;

                        foreach (var declaredMember in RecordPositionalParameterUtilities.GetDeclaredMembers(parameterSymbol, parameter))
                        {
                            yield return declaredMember;
                        }
                    }
                }
                break;

            case VariableDeclarationSyntax variableDeclaration when variableDeclaration.Type == type:
                {
                    foreach (var variable in variableDeclaration.Variables)
                    {
                        yield return semanticModel.GetDeclaredSymbol(variable, cancellationToken);
                    }
                }
                break;

            case MemberDeclarationSyntax member:
                {
                    yield return semanticModel.GetDeclaredSymbol(member, cancellationToken);
                }
                break;
        }
    }

    #endregion // Methods

    #region CasingAnalyzerBase

    /// <inheritdoc/>
    protected override IEnumerable<(string Name, Location Location)> GetLocations(SyntaxNode node)
    {
        if (node is TupleTypeSyntax tupleType)
        {
            foreach (var identifier in tupleType.Elements
                                                .Where(element => element.Identifier != default)
                                                .Select(element => element.Identifier))
            {
                yield return (identifier.ValueText, identifier.GetLocation());
            }
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// A tuple type is in the signature of a member when it stands, at any depth, in the type the compiler declares the
    /// member with: its return type, the type of one of its parameters, or the type of a property, indexer, event, or
    /// field. The element names there are dictated when such a member is; a record's positional parameter is also in the
    /// signature of the property and the <c>Deconstruct</c> method the compiler declares from it, and the type of an event
    /// field in that of each event it declares (SHARED-CASING-014)
    /// </remarks>
    private protected override bool IsDictatedName(SyntaxNode node, Location location, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        return node is TypeSyntax type
               && GetDeclaredMembers(GetOutermostType(type), semanticModel, cancellationToken).Any(IsDictatedMember);
    }

    #endregion // CasingAnalyzerBase
}