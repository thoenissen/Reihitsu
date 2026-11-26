using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Analyzer.Core;

/// <summary>
/// The one owner of which members the compiler declares from a record's positional parameter, besides the primary
/// constructor the parameter belongs to: the property it declares for the parameter, and the <c>Deconstruct</c> method it
/// declares for the record's parameter list; and of the member the parameter stands for
/// </summary>
internal static class RecordPositionalParameterUtilities
{
    #region Constants

    /// <summary>
    /// Name of the method the compiler declares to deconstruct a record into its positional parameters
    /// </summary>
    private const string DeconstructMethodName = "Deconstruct";

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Gets the property the compiler declares for a record's positional parameter
    /// </summary>
    /// <param name="recordType">The record type</param>
    /// <param name="parameter">The positional parameter</param>
    /// <returns>
    /// The property, or <see langword="null"/> when the record declares or inherits a member of the parameter's
    /// name, so the compiler declares none
    /// </returns>
    /// <remarks>
    /// The compiler declares the property with the parameter itself as its declaring syntax, which tells it apart from a
    /// property of the same name the record declares
    /// </remarks>
    internal static IPropertySymbol GetDeclaredProperty(INamedTypeSymbol recordType, ParameterSyntax parameter)
    {
        return recordType?.GetMembers(parameter.Identifier.ValueText)
                         .OfType<IPropertySymbol>()
                         .FirstOrDefault(property => property.DeclaringSyntaxReferences.Any(reference => reference.SyntaxTree == parameter.SyntaxTree
                                                                                                         && reference.Span == parameter.Span));
    }

    /// <summary>
    /// Gets the member a record's positional parameter stands for: the property the compiler declares for it, or, when
    /// the record declares or inherits a member of the parameter's name, so the compiler declares none, that member
    /// </summary>
    /// <param name="compilation">The compilation the record belongs to</param>
    /// <param name="recordType">The record type</param>
    /// <param name="parameter">The positional parameter</param>
    /// <returns>The member, or <see langword="null"/> when none is found</returns>
    /// <remarks>
    /// The record and its base types are searched from the record outward, and the first type that has a property or a
    /// field of the parameter's name that the record can access gives it, so a member declared in a derived type hides one of
    /// a base type, and an inherited member the record cannot access is skipped, as the compiler skips it. Accessibility is
    /// the compiler's result, and every member the record declares is accessible to it. The property the compiler declares
    /// is a member of the record, so it is found first when it exists
    /// </remarks>
    internal static ISymbol GetMember(Compilation compilation, INamedTypeSymbol recordType, ParameterSyntax parameter)
    {
        for (var type = recordType; type != null; type = type.BaseType)
        {
            var member = type.GetMembers(parameter.Identifier.ValueText)
                             .FirstOrDefault(candidate => candidate is IPropertySymbol or IFieldSymbol
                                                          && compilation.IsSymbolAccessibleWithin(candidate, recordType));

            if (member != null)
            {
                return member;
            }
        }

        return null;
    }

    /// <summary>
    /// Gets the members the compiler declares from a record's positional parameter whose signature holds the
    /// parameter's type, besides the primary constructor: the property it declares for the parameter, and the
    /// <c>Deconstruct</c> method it declares for the record's parameter list
    /// </summary>
    /// <param name="parameterSymbol">The parameter's symbol</param>
    /// <param name="parameter">The parameter</param>
    /// <returns>The declared members; none when the parameter is not a record's positional parameter</returns>
    /// <remarks>
    /// The compiler declares <c>Deconstruct</c> only when the record declares no method of the same signature; it is told
    /// apart from a declared one by being implicitly declared, and from one of another arity by its parameter count. No
    /// other member it declares for a record, such as <c>Equals</c> or the copy constructor, carries a positional
    /// parameter's type
    /// </remarks>
    internal static IEnumerable<ISymbol> GetDeclaredMembers(IParameterSymbol parameterSymbol, ParameterSyntax parameter)
    {
        if (parameter.Parent is not ParameterListSyntax { Parent: RecordDeclarationSyntax } parameterList
            || parameterSymbol.ContainingSymbol is not IMethodSymbol { MethodKind: MethodKind.Constructor } constructor)
        {
            yield break;
        }

        var recordType = constructor.ContainingType;
        var property = GetDeclaredProperty(recordType, parameter);

        if (property != null)
        {
            yield return property;
        }

        foreach (var deconstruct in recordType.GetMembers(DeconstructMethodName)
                                              .OfType<IMethodSymbol>()
                                              .Where(method => method.IsImplicitlyDeclared
                                                               && method.Parameters.Length == parameterList.Parameters.Count))
        {
            yield return deconstruct;
        }
    }

    #endregion // Methods
}