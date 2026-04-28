using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Design;

/// <summary>
/// RH0113: Nested types should not be used
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0113NestedTypesShouldNotBeUsedAnalyzer : DiagnosticAnalyzerBase<RH0113NestedTypesShouldNotBeUsedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0113";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0113NestedTypesShouldNotBeUsedAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Design, nameof(AnalyzerResources.RH0113Title), nameof(AnalyzerResources.RH0113MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Getting the identifier location of the declaration
    /// </summary>
    /// <param name="memberDeclaration">Member declaration</param>
    /// <returns>Identifier location</returns>
    private static Location GetIdentifierLocation(MemberDeclarationSyntax memberDeclaration)
    {
        return memberDeclaration switch
               {
                   BaseTypeDeclarationSyntax baseTypeDeclaration => baseTypeDeclaration.Identifier.GetLocation(),
                   DelegateDeclarationSyntax delegateDeclaration => delegateDeclaration.Identifier.GetLocation(),
                   _ => memberDeclaration.GetLocation(),
               };
    }

    /// <summary>
    /// Determining whether the declaration is a nested type
    /// </summary>
    /// <param name="memberDeclaration">Member declaration</param>
    /// <returns>True when the declaration is a nested type</returns>
    private static bool IsNestedTypeDeclaration(MemberDeclarationSyntax memberDeclaration)
    {
        return memberDeclaration is BaseTypeDeclarationSyntax or DelegateDeclarationSyntax;
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

        var nestedTypeDeclarations = root.DescendantNodes(static node => node is CompilationUnitSyntax or BaseNamespaceDeclarationSyntax or TypeDeclarationSyntax)
                                         .OfType<MemberDeclarationSyntax>()
                                         .Where(static memberDeclaration => memberDeclaration.Parent is TypeDeclarationSyntax && IsNestedTypeDeclaration(memberDeclaration));

        foreach (var nestedTypeDeclaration in nestedTypeDeclarations)
        {
            context.ReportDiagnostic(CreateDiagnostic(GetIdentifierLocation(nestedTypeDeclaration)));
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