using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5404: Element must not be on a single line
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5404ElementMustNotBeOnSingleLineAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5404";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5404ElementMustNotBeOnSingleLineAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Layout, nameof(AnalyzerResources.RH5404Title), nameof(AnalyzerResources.RH5404MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzes a type declaration
    /// </summary>
    /// <param name="context">Context</param>
    private void OnDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not BaseTypeDeclarationSyntax declaration)
        {
            return;
        }

        if (declaration is TypeDeclarationSyntax typeDeclaration
            && typeDeclaration.SemicolonToken.IsKind(SyntaxKind.SemicolonToken))
        {
            return;
        }

        if (declaration.OpenBraceToken.IsMissing
            || declaration.CloseBraceToken.IsMissing)
        {
            return;
        }

        var openBraceLine = declaration.OpenBraceToken.GetLocation().GetLineSpan().StartLinePosition.Line;
        var closeBraceLine = declaration.CloseBraceToken.GetLocation().GetLineSpan().EndLinePosition.Line;

        if (openBraceLine == closeBraceLine)
        {
            context.ReportDiagnostic(CreateDiagnostic(declaration.Identifier.GetLocation()));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        // The previous syntax-tree action matched the abstract BaseTypeDeclarationSyntax, so every concrete kind
        // has to be listed here to keep the same coverage. Each registered kind has a test that fails if it is
        // dropped, and RH5404's test class carries a canary over the declaration types themselves.
        //
        // SyntaxKind.UnionDeclaration is deliberately absent. Union declarations parse only under a preview
        // language version and need runtime support types .NET 10 does not ship, and registering the kind would
        // pull an experimental Roslyn API into the analyzer. They are left out until the feature is released.
        context.RegisterSyntaxNodeAction(OnDeclaration,
                                         SyntaxKind.ClassDeclaration,
                                         SyntaxKind.StructDeclaration,
                                         SyntaxKind.InterfaceDeclaration,
                                         SyntaxKind.RecordDeclaration,
                                         SyntaxKind.RecordStructDeclaration,
                                         SyntaxKind.EnumDeclaration,
                                         SyntaxKind.ExtensionBlockDeclaration);
    }

    #endregion // DiagnosticAnalyzer
}