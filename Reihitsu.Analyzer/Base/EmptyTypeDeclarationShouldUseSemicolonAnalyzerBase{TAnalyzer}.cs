using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Base;

/// <summary>
/// Base class for type-specific analyzers that prefer semicolon declarations for empty type bodies
/// </summary>
/// <typeparam name="TAnalyzer">Analyzer type</typeparam>
public abstract class EmptyTypeDeclarationShouldUseSemicolonAnalyzerBase<TAnalyzer> : DiagnosticAnalyzerBase<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    #region Fields

    /// <summary>
    /// The declaration kind analyzed by this rule
    /// </summary>
    private readonly SyntaxKind _declarationKind;

    /// <summary>
    /// The minimum required language version
    /// </summary>
    private readonly LanguageVersion _minimumLanguageVersion;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="diagnosticId">Diagnostic ID</param>
    /// <param name="titleResourceName">Title resource name</param>
    /// <param name="messageFormatResourceName">Message resource name</param>
    /// <param name="declarationKind">Declaration kind</param>
    /// <param name="minimumLanguageVersion">Minimum required language version</param>
    protected EmptyTypeDeclarationShouldUseSemicolonAnalyzerBase(string diagnosticId, string titleResourceName, string messageFormatResourceName, SyntaxKind declarationKind, LanguageVersion minimumLanguageVersion)
        : base(diagnosticId, DiagnosticCategory.Formatting, titleResourceName, messageFormatResourceName)
    {
        _declarationKind = declarationKind;
        _minimumLanguageVersion = minimumLanguageVersion;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzes the configured type declaration kind
    /// </summary>
    /// <param name="context">Context</param>
    private void OnTypeDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is TypeDeclarationSyntax typeDeclaration
            && EmptyTypeDeclarationSemicolonAnalysisUtilities.ShouldReport(typeDeclaration, _declarationKind, _minimumLanguageVersion))
        {
            context.ReportDiagnostic(CreateDiagnostic(typeDeclaration.Identifier.GetLocation()));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnTypeDeclaration, _declarationKind);
    }

    #endregion // DiagnosticAnalyzer
}