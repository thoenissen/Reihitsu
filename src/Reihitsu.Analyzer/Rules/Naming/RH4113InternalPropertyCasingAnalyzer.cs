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
/// RH4113: Internal property names should be in PascalCase
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH4113InternalPropertyCasingAnalyzer : CasingAnalyzerBase
{
    #region Fields

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH4113";

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH4113InternalPropertyCasingAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Naming, SyntaxKind.PropertyDeclaration, CasingUtilities.IsPascalCase)
    {
    }

    #endregion // Constructor

    #region CasingAnalyzerBase

    /// <inheritdoc/>
    protected override IEnumerable<(string Name, Location Location)> GetLocations(SyntaxNode node)
    {
        if (node is PropertyDeclarationSyntax declaration
            && NamingAccessibilityClassifier.GetEffectiveAccessibility(declaration) == Accessibility.Internal)
        {
            yield return (declaration.Identifier.ValueText, declaration.Identifier.GetLocation());
        }
    }

    /// <inheritdoc/>
    private protected override bool IsDictatedName(SyntaxNode node, Location location, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        return IsDictatedMember(semanticModel.GetDeclaredSymbol(node, cancellationToken));
    }

    #endregion // CasingAnalyzerBase
}