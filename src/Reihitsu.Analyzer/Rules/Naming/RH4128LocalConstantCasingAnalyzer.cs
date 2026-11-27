using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Naming;

/// <summary>
/// RH4128: Local constant names should be in camelCase
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH4128LocalConstantCasingAnalyzer : CasingAnalyzerBase
{
    #region Fields

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH4128";

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH4128LocalConstantCasingAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Naming, SyntaxKind.LocalDeclarationStatement, CasingUtilities.IsCamelCase)
    {
    }

    #endregion // Constructor

    #region CasingAnalyzerBase

    /// <inheritdoc/>
    /// <remarks>Only a declaration with the <c>const</c> modifier is checked, wherever it stands; a constant named <c>_</c> is not checked</remarks>
    protected override IEnumerable<(string Name, Location Location)> GetLocations(SyntaxNode node)
    {
        return node is LocalDeclarationStatementSyntax { IsConst: true } localDeclaration
                   ? localDeclaration.Declaration
                                     .Variables
                                     .Select(static variable => variable.Identifier)
                                     .Where(static identifier => identifier.ValueText != "_")
                                     .Select(static identifier => (identifier.ValueText, identifier.GetLocation()))
                   : [];
    }

    #endregion // CasingAnalyzerBase
}