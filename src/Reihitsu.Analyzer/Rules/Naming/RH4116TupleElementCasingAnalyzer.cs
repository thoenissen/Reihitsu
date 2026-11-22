using System.Collections.Generic;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
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
    /// Gets the parameters of a property-like declaration: those of an indexer, and none otherwise
    /// </summary>
    /// <param name="property">The property, indexer, or event declaration</param>
    /// <returns>The parameters</returns>
    private static IEnumerable<ParameterSyntax> GetParameters(BasePropertyDeclarationSyntax property)
    {
        return property is IndexerDeclarationSyntax indexer
                   ? indexer.ParameterList.Parameters
                   : [];
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
    /// A tuple type is in the signature of a member when it stands, at any depth, in the member's return type, the type
    /// of one of its parameters, or the type of a property, indexer, or event; the element names there are dictated when
    /// the member is (SHARED-CASING-014)
    /// </remarks>
    private protected override bool IsDictatedName(SyntaxNode node, Location location, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        var member = node.Ancestors().FirstOrDefault(static ancestor => ancestor is MemberDeclarationSyntax);
        var signatureTypes = member switch
                             {
                                 MethodDeclarationSyntax method => method.ParameterList.Parameters.Select(static parameter => parameter.Type).Prepend(method.ReturnType),
                                 BasePropertyDeclarationSyntax property => GetParameters(property).Select(static parameter => parameter.Type).Prepend(property.Type),
                                 EventFieldDeclarationSyntax eventField => [eventField.Declaration.Type],
                                 _ => []
                             };

        if (signatureTypes.Any(type => type != null && type.Span.Contains(node.Span)) == false)
        {
            return false;
        }

        var declaration = member is EventFieldDeclarationSyntax eventFieldDeclaration
                              ? eventFieldDeclaration.Declaration.Variables.FirstOrDefault()
                              : member;

        return declaration != null
               && IsDictatedMember(semanticModel.GetDeclaredSymbol(declaration, cancellationToken));
    }

    #endregion // CasingAnalyzerBase
}