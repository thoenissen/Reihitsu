using System.Collections.Generic;

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
/// RH4105: Parameter and query range variable names should be in camelCase
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH4105MethodParameterCasingAnalyzer : CasingAnalyzerBase
{
    #region Fields

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH4105";

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH4105MethodParameterCasingAnalyzer()
        : base(DiagnosticId,
               DiagnosticCategory.Naming,
               [SyntaxKind.Parameter, .. RangeVariableDeclarationUtilities.DeclarationKinds],
               CasingUtilities.IsCamelCase)
    {
    }

    #endregion // Constructor

    #region CasingAnalyzerBase

    /// <inheritdoc/>
    /// <remarks>
    /// A parameter without a name, such as the receiver parameter of <c>extension(long)</c>, has nothing to check and is
    /// skipped, as is a parameter of a record's primary constructor. A range variable is checked at the query clause that
    /// declares it, never at a later use. A parameter or range variable named <c>_</c> is not checked
    /// </remarks>
    protected override IEnumerable<(string Name, Location Location)> GetLocations(SyntaxNode node)
    {
        var identifier = node switch
                         {
                             ParameterSyntax { Parent: not ParameterListSyntax { Parent: RecordDeclarationSyntax } } parameter => parameter.Identifier,
                             ParameterSyntax => default,
                             _ => RangeVariableDeclarationUtilities.GetIdentifier(node)
                         };

        if (identifier.Span.IsEmpty == false
            && identifier.ValueText != "_")
        {
            yield return (identifier.ValueText, identifier.GetLocation());
        }
    }

    #endregion // CasingAnalyzerBase
}