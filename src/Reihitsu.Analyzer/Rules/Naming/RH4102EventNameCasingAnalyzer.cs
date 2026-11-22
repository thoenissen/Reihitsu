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
/// RH4102: Event names should be in PascalCase
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH4102EventNameCasingAnalyzer : CasingAnalyzerBase
{
    #region Fields

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH4102";

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH4102EventNameCasingAnalyzer()
        : base(DiagnosticId,
               DiagnosticCategory.Naming,
               [SyntaxKind.EventFieldDeclaration, SyntaxKind.EventDeclaration],
               CasingUtilities.IsPascalCase)
    {
    }

    #endregion // Constructor

    #region CasingAnalyzerBase

    /// <inheritdoc/>
    protected override IEnumerable<(string Name, Location Location)> GetLocations(SyntaxNode node)
    {
        if (node is EventFieldDeclarationSyntax declaration)
        {
            foreach (var identifier in declaration.Declaration
                                                  .Variables
                                                  .Select(variable => variable.Identifier))
            {
                yield return (identifier.ValueText, identifier.GetLocation());
            }
        }
        else if (node is EventDeclarationSyntax { ExplicitInterfaceSpecifier: null } eventDeclaration)
        {
            yield return (eventDeclaration.Identifier.ValueText, eventDeclaration.Identifier.GetLocation());
        }
    }

    /// <inheritdoc/>
    private protected override bool IsDictatedName(SyntaxNode node, Location location, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        var declaration = node is EventFieldDeclarationSyntax fieldDeclaration
                              ? fieldDeclaration.Declaration
                                                .Variables
                                                .FirstOrDefault(variable => variable.Identifier.Span == location.SourceSpan)
                              : node;

        return declaration != null
               && IsDictatedMember(semanticModel.GetDeclaredSymbol(declaration, cancellationToken));
    }

    #endregion // CasingAnalyzerBase
}