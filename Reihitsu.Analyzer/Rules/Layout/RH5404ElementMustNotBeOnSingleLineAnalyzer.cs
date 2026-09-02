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

        // Every concrete BaseTypeDeclarationSyntax kind has to be listed here, because the previous syntax-tree
        // action matched the abstract type and so covered all of them. RH5404's test class carries a canary over
        // those declaration types, so a kind added by a future Roslyn version fails loudly instead of silently
        // dropping out of scope.
        // SyntaxKind.UnionDeclaration is still an experimental Roslyn API. It is registered anyway because the
        // previous syntax-tree action matched BaseTypeDeclarationSyntax and therefore already covered union
        // declarations for consumers compiling with a preview language version; omitting the kind here would
        // silently drop those diagnostics.
#pragma warning disable RSEXPERIMENTAL006
        context.RegisterSyntaxNodeAction(OnDeclaration,
                                         SyntaxKind.ClassDeclaration,
                                         SyntaxKind.StructDeclaration,
                                         SyntaxKind.InterfaceDeclaration,
                                         SyntaxKind.RecordDeclaration,
                                         SyntaxKind.RecordStructDeclaration,
                                         SyntaxKind.EnumDeclaration,
                                         SyntaxKind.ExtensionBlockDeclaration,
                                         SyntaxKind.UnionDeclaration);
#pragma warning restore RSEXPERIMENTAL006
    }

    #endregion // DiagnosticAnalyzer
}