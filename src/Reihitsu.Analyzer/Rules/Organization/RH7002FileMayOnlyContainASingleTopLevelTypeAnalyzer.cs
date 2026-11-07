using System.Collections.Generic;
using System.Globalization;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Organization;

/// <summary>
/// RH7002: File may only contain a single top-level type
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH7002FileMayOnlyContainASingleTopLevelTypeAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH7002";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH7002FileMayOnlyContainASingleTopLevelTypeAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Organization, nameof(AnalyzerResources.RH7002Title), nameof(AnalyzerResources.RH7002MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Gets a key that identifies the type a top-level declaration declares: its namespace, name, and number of type
    /// parameters, each name compared by its value (SHARED-DIAG-002), so that two parts of one partial type get the
    /// same key (RH7002-004)
    /// </summary>
    /// <param name="typeDeclaration">Top-level type declaration</param>
    /// <returns>The key</returns>
    private static string GetTypeKey(BaseTypeDeclarationSyntax typeDeclaration)
    {
        var namespaceNames = typeDeclaration.Ancestors()
                                            .OfType<BaseNamespaceDeclarationSyntax>()
                                            .Reverse()
                                            .SelectMany(static namespaceDeclaration => NamespaceCasingHelper.GetLocations(namespaceDeclaration.Name))
                                            .Select(static part => part.Name);
        var arity = typeDeclaration is TypeDeclarationSyntax { TypeParameterList: not null } genericDeclaration
                        ? genericDeclaration.TypeParameterList.Parameters.Count
                        : 0;

        return $"{string.Join(".", namespaceNames.Append(typeDeclaration.Identifier.ValueText))}`{arity.ToString(CultureInfo.InvariantCulture)}";
    }

    /// <summary>
    /// Analyzing all syntax trees
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        if (context.Tree.GetRoot(context.CancellationToken) is not CompilationUnitSyntax root)
        {
            return;
        }

        var topLevelTypeDeclarations = root.DescendantNodes(static node => node is CompilationUnitSyntax or BaseNamespaceDeclarationSyntax)
                                           .OfType<BaseTypeDeclarationSyntax>()
                                           .ToList();

        if (topLevelTypeDeclarations.Count <= 1)
        {
            return;
        }

        var declaredTypes = new HashSet<string>(StringComparer.Ordinal)
                            {
                                GetTypeKey(topLevelTypeDeclarations[0])
                            };

        foreach (var topLevelTypeDeclaration in topLevelTypeDeclarations.Skip(1))
        {
            var isFirstDeclarationOfType = declaredTypes.Add(GetTypeKey(topLevelTypeDeclaration));

            // A further part of a partial type declared earlier in the file is the same type (RH7002-004)
            if (isFirstDeclarationOfType
                || topLevelTypeDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword) == false)
            {
                context.ReportDiagnostic(CreateDiagnostic(topLevelTypeDeclaration.Identifier.GetLocation()));
            }
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxTreeAction(OnSyntaxTree);
    }

    #endregion // DiagnosticAnalyzer
}