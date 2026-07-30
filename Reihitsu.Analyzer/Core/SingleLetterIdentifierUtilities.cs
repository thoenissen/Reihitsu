using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Analyzer.Core;

/// <summary>
/// Utilities for analyzers that reject single-letter identifiers
/// </summary>
internal static class SingleLetterIdentifierUtilities
{
    #region Methods

    /// <summary>
    /// Checks whether an identifier uses a single-letter name
    /// </summary>
    /// <param name="name">Identifier name</param>
    /// <returns>True if the identifier uses a single-letter name, otherwise false</returns>
    internal static bool HasSingleLetterName(string name)
    {
        return name.Length == 1
               && name != "_";
    }

    /// <summary>
    /// Checks whether the parameter belongs to a signature that is not practical to rename
    /// </summary>
    /// <param name="parameterSymbol">Parameter symbol</param>
    /// <returns>True if the signature should be excluded, otherwise false</returns>
    internal static bool IsFrameworkMandatedSignature(IParameterSymbol parameterSymbol)
    {
        if (parameterSymbol.ContainingSymbol is IMethodSymbol methodSymbol)
        {
            return methodSymbol.IsOverride
                   || methodSymbol.ExplicitInterfaceImplementations.Length > 0;
        }

        if (parameterSymbol.ContainingSymbol is IPropertySymbol propertySymbol)
        {
            return propertySymbol.IsOverride
                   || propertySymbol.ExplicitInterfaceImplementations.Length > 0;
        }

        return false;
    }

    /// <summary>
    /// Checks whether a designation declares part of a deconstructed foreach iteration variable
    /// </summary>
    /// <param name="designation">Variable designation</param>
    /// <returns>True if the designation belongs to a foreach iteration variable, otherwise false</returns>
    internal static bool IsForEachIterationVariable(SingleVariableDesignationSyntax designation)
    {
        return designation.Ancestors()
                          .OfType<ForEachVariableStatementSyntax>()
                          .Any(forEachStatement => forEachStatement.Variable.Span.Contains(designation.Span));
    }

    /// <summary>
    /// Checks whether a parameter belongs to a record primary constructor
    /// </summary>
    /// <param name="parameterSymbol">Parameter symbol</param>
    /// <returns>True if the parameter belongs to a record primary constructor, otherwise false</returns>
    internal static bool IsRecordPrimaryConstructorParameter(IParameterSymbol parameterSymbol)
    {
        return parameterSymbol.DeclaringSyntaxReferences.Any(syntaxReference => syntaxReference.GetSyntax() is ParameterSyntax { Parent: ParameterListSyntax { Parent: RecordDeclarationSyntax } });
    }

    #endregion // Methods
}