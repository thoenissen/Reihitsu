using System;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// Region descriptions should not end with implementation
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0388RegionDescriptionsShouldNotEndWithImplementationAnalyzer : DiagnosticAnalyzerBase<RH0388RegionDescriptionsShouldNotEndWithImplementationAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0388";

    /// <summary>
    /// Forbidden suffix
    /// </summary>
    private const string ForbiddenSuffix = "implementation";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0388RegionDescriptionsShouldNotEndWithImplementationAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0388Title), nameof(AnalyzerResources.RH0388MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether the specified description ends with the forbidden suffix
    /// </summary>
    /// <param name="description">Description to inspect</param>
    /// <returns><see langword="true"/> if the suffix is present</returns>
    private static bool EndsWithForbiddenSuffix(string description)
    {
        var trimmedDescription = description.Trim();

        if (trimmedDescription.EndsWith(ForbiddenSuffix, StringComparison.OrdinalIgnoreCase) == false)
        {
            return false;
        }

        return trimmedDescription.Length == ForbiddenSuffix.Length
               || char.IsWhiteSpace(trimmedDescription[trimmedDescription.Length - ForbiddenSuffix.Length - 1]);
    }

    /// <summary>
    /// Gets the description of an endregion directive
    /// </summary>
    /// <param name="directive">Directive</param>
    /// <returns>Description text</returns>
    private static string GetEndRegionDescription(EndRegionDirectiveTriviaSyntax directive)
    {
        var description = (directive.EndRegionKeyword.TrailingTrivia.ToFullString()
                           + directive.EndOfDirectiveToken.LeadingTrivia.ToFullString()).Trim();

        if (description.StartsWith("//", StringComparison.Ordinal))
        {
            description = description.Substring(2).TrimStart();
        }

        return description;
    }

    /// <summary>
    /// Gets the description of a region directive
    /// </summary>
    /// <param name="directive">Directive</param>
    /// <returns>Description text</returns>
    private static string GetRegionDescription(RegionDirectiveTriviaSyntax directive)
    {
        var messageTrivia = directive.EndOfDirectiveToken.LeadingTrivia.FirstOrDefault(static trivia => trivia.IsKind(SyntaxKind.PreprocessingMessageTrivia));

        return messageTrivia == default
                   ? string.Empty
                   : messageTrivia.ToString().Trim();
    }

    /// <summary>
    /// Analyzes endregion directives
    /// </summary>
    /// <param name="context">Context</param>
    private void OnEndRegion(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is EndRegionDirectiveTriviaSyntax node
            && EndsWithForbiddenSuffix(GetEndRegionDescription(node)))
        {
            context.ReportDiagnostic(CreateDiagnostic(node.GetLocation()));
        }
    }

    /// <summary>
    /// Analyzes region directives
    /// </summary>
    /// <param name="context">Context</param>
    private void OnRegion(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is RegionDirectiveTriviaSyntax node
            && EndsWithForbiddenSuffix(GetRegionDescription(node)))
        {
            context.ReportDiagnostic(CreateDiagnostic(node.GetLocation()));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnRegion, SyntaxKind.RegionDirectiveTrivia);
        context.RegisterSyntaxNodeAction(OnEndRegion, SyntaxKind.EndRegionDirectiveTrivia);
    }

    #endregion // DiagnosticAnalyzer
}