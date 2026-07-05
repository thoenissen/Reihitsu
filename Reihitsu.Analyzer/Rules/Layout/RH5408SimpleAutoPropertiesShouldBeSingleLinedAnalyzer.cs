using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5408: Simple auto-properties should be single lined
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5408";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Layout, nameof(AnalyzerResources.RH5408Title), nameof(AnalyzerResources.RH5408MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether the given accessor list belongs to a simple auto-property
    /// (all accessors have neither a body nor an expression body)
    /// </summary>
    /// <param name="accessorList">The accessor list to inspect</param>
    /// <returns><see langword="true"/> if every accessor is body-free; otherwise, <see langword="false"/></returns>
    private static bool IsAutoPropertyAccessorList(AccessorListSyntax accessorList)
    {
        foreach (var accessor in accessorList.Accessors)
        {
            if (accessor.Body != null || accessor.ExpressionBody != null)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Gets the first token of the property signature while skipping property-level attributes
    /// </summary>
    /// <param name="propertyDeclaration">The property declaration to inspect</param>
    /// <returns>The first signature token</returns>
    private static SyntaxToken GetSingleLineSignatureStartToken(PropertyDeclarationSyntax propertyDeclaration)
    {
        if (propertyDeclaration.Modifiers.Count > 0)
        {
            return propertyDeclaration.Modifiers[0];
        }

        return propertyDeclaration.Type.GetFirstToken();
    }

    /// <summary>
    /// Gets the portion of the property declaration that is expected to fit on a single line
    /// </summary>
    /// <param name="propertyDeclaration">The property declaration to inspect</param>
    /// <returns>The relevant property span</returns>
    private static TextSpan GetRelevantPropertySpan(PropertyDeclarationSyntax propertyDeclaration)
    {
        var signatureStartToken = GetSingleLineSignatureStartToken(propertyDeclaration);

        return TextSpan.FromBounds(signatureStartToken.SpanStart, propertyDeclaration.Span.End);
    }

    /// <summary>
    /// Determines whether the given property can be collapsed to a single line without losing readability
    /// </summary>
    /// <param name="propertyDeclaration">The property declaration to inspect</param>
    /// <returns><see langword="true"/> if the declaration can be collapsed to a single line; otherwise, <see langword="false"/></returns>
    private static bool IsEligibleSimpleAutoProperty(PropertyDeclarationSyntax propertyDeclaration)
    {
        if (propertyDeclaration.AccessorList == null)
        {
            return false;
        }

        // The formatter's CanCollapseAutoPropertyToSingleLine bails out on any comment or directive in the
        // accessor list (for example a comment between accessors), so the analyzer must guard the same shape,
        // otherwise it flags a property the formatter never collapses, leaving a permanent diagnostic.
        if (FormattingSafetyUtilities.HasCommentsOrDirectives(propertyDeclaration.AccessorList))
        {
            return false;
        }

        var tokenBeforeOpenBrace = propertyDeclaration.AccessorList.OpenBraceToken.GetPreviousToken();
        var signatureStartToken = GetSingleLineSignatureStartToken(propertyDeclaration);

        if (signatureStartToken == default
            || signatureStartToken.IsKind(SyntaxKind.None)
            || tokenBeforeOpenBrace == default
            || tokenBeforeOpenBrace.IsKind(SyntaxKind.None))
        {
            return false;
        }

        if (FormattingSafetyUtilities.IsSingleLineSpan(propertyDeclaration.SyntaxTree, TextSpan.FromBounds(signatureStartToken.SpanStart, tokenBeforeOpenBrace.Span.End)) == false)
        {
            return false;
        }

        if (propertyDeclaration.Initializer != null)
        {
            if (FormattingSafetyUtilities.HasCommentsOrDirectives(propertyDeclaration.Initializer))
            {
                return false;
            }

            if (FormattingSafetyUtilities.IsSingleLineSpan(propertyDeclaration.SyntaxTree, propertyDeclaration.Initializer.Value.Span) == false)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Analyzing all <see cref="SyntaxKind.PropertyDeclaration"/> occurrences
    /// </summary>
    /// <param name="context">Context</param>
    private void OnPropertyDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not PropertyDeclarationSyntax propertyDeclaration)
        {
            return;
        }

        if (propertyDeclaration.AccessorList is null)
        {
            return;
        }

        if (propertyDeclaration.ExpressionBody is not null)
        {
            return;
        }

        if (IsAutoPropertyAccessorList(propertyDeclaration.AccessorList) == false)
        {
            return;
        }

        if (IsEligibleSimpleAutoProperty(propertyDeclaration) == false)
        {
            return;
        }

        var relevantSpan = GetRelevantPropertySpan(propertyDeclaration);
        var lineSpan = propertyDeclaration.SyntaxTree.GetLineSpan(relevantSpan);

        if (lineSpan.StartLinePosition.Line != lineSpan.EndLinePosition.Line)
        {
            context.ReportDiagnostic(CreateDiagnostic(Location.Create(propertyDeclaration.SyntaxTree, relevantSpan)));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnPropertyDeclaration, SyntaxKind.PropertyDeclaration);
    }

    #endregion // DiagnosticAnalyzer
}