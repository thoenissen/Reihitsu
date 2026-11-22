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
/// RH4104: Local function names should be in PascalCase
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH4104LocalFunctionNameCasingAnalyzer : CasingAnalyzerBase
{
    #region Fields

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH4104";

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH4104LocalFunctionNameCasingAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Naming, SyntaxKind.LocalFunctionStatement, CasingUtilities.IsPascalCase)
    {
    }

    #endregion // Constructor

    #region CasingAnalyzerBase

    /// <inheritdoc/>
    protected override IEnumerable<(string Name, Location Location)> GetLocations(SyntaxNode node)
    {
        if (node is LocalFunctionStatementSyntax declaration)
        {
            yield return (declaration.Identifier.ValueText, declaration.Identifier.GetLocation());
        }
    }

    /// <inheritdoc/>
    private protected override bool IsDictatedName(SyntaxNode node, Location location, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        return node is LocalFunctionStatementSyntax declaration
               && HasNativeEntryPointFromName(declaration.AttributeLists, semanticModel, cancellationToken);
    }

    #endregion // CasingAnalyzerBase
}