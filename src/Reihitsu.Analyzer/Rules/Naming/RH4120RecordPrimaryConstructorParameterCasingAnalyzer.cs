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
/// RH4120: Primary constructor parameter names on records should be in PascalCase
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH4120RecordPrimaryConstructorParameterCasingAnalyzer : CasingAnalyzerBase
{
    #region Fields

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH4120";

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH4120RecordPrimaryConstructorParameterCasingAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Naming, SyntaxKind.Parameter, CasingUtilities.IsPascalCase)
    {
    }

    #endregion // Constructor

    #region CasingAnalyzerBase

    /// <inheritdoc/>
    protected override IEnumerable<(string Name, Location Location)> GetLocations(SyntaxNode node)
    {
        if (node is ParameterSyntax parameter
            && parameter.Identifier.ValueText != "_"
            && parameter.Parent is ParameterListSyntax { Parent: RecordDeclarationSyntax })
        {
            yield return (parameter.Identifier.ValueText, parameter.Identifier.GetLocation());
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// The parameter's name is dictated when the member it stands for is dictated: the property the compiler declares for
    /// it, or the member of its name the record declares or inherits and can access
    /// </remarks>
    private protected override bool IsDictatedName(SyntaxNode node, Location location, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        return node is ParameterSyntax parameter
               && semanticModel.GetDeclaredSymbol(parameter, cancellationToken) is IParameterSymbol { ContainingSymbol: IMethodSymbol { ContainingType: { } recordType } }
               && IsDictatedMember(RecordPositionalParameterUtilities.GetMember(semanticModel.Compilation, recordType, parameter));
    }

    #endregion // CasingAnalyzerBase
}