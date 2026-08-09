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

        // The formatter's CanCollapseAutoPropertyToSingleLine bails out on a comment or directive inside the
        // accessor list (for example a comment between accessors), so the analyzer must guard the same shape,
        // otherwise it flags a property the formatter never collapses, leaving a permanent diagnostic. The
        // guard is interior-scoped to match the formatter exactly: a comment trailing the closing brace sits
        // outside the accessor list, is never crossed by the collapse, and must not suppress the diagnostic.
        if (SyntaxNodeUtilities.InteriorContainsCommentOrDirective(propertyDeclaration.AccessorList))
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

        // A comment or directive in the gap between the signature and the accessor brace (for example
        // "public int X // note\n{ get; set; }") lives in the signature's trailing trivia, not in the accessor
        // list, so the check above misses it. The formatter refuses to join the brace across such trivia, so the
        // analyzer must guard the same gap to avoid a permanent diagnostic.
        if (SyntaxTriviaUtilities.WouldJoinAcrossUnjoinableTrivia(tokenBeforeOpenBrace, propertyDeclaration.AccessorList.OpenBraceToken))
        {
            return false;
        }

        if (SyntaxNodeUtilities.IsSingleLineSpan(propertyDeclaration.SyntaxTree, TextSpan.FromBounds(signatureStartToken.SpanStart, tokenBeforeOpenBrace.Span.End)) == false)
        {
            return false;
        }

        if (propertyDeclaration.Initializer != null)
        {
            // Only the initializer's own span is inspected. Its full span additionally covers the leading trivia of
            // the equals token and the trailing trivia of the value's last token, and neither region is crossed in
            // every case: the two gap guards below own them wherever the collapse actually has to join across them,
            // and where they do not fire nothing is joined at all. Both of those guards carry a line-span
            // precondition and are, after this narrowing, the only owners of their region — weakening either one
            // silently uncovers it.
            if (SyntaxNodeUtilities.InteriorContainsCommentOrDirective(propertyDeclaration.Initializer))
            {
                return false;
            }

            // The reported span runs to the end of the declaration, so it covers the initializer as well. A comment
            // in the gap between the accessor list and the initializer lives in the closing brace's trailing trivia,
            // which the check above does not inspect; a directive cannot occupy trailing trivia, so it always lands
            // in the initializer's leading trivia and is already covered. The formatter only has to join that gap
            // when the initializer starts on a later line, and it refuses to join across such trivia, so the
            // analyzer must guard the same gap to avoid a permanent diagnostic. Trivia in a gap that already sits
            // on one line is never crossed and must not suppress the diagnostic.
            var initializerGapSpan = TextSpan.FromBounds(propertyDeclaration.AccessorList.CloseBraceToken.Span.End, propertyDeclaration.Initializer.EqualsToken.SpanStart);

            if (SyntaxNodeUtilities.IsSingleLineSpan(propertyDeclaration.SyntaxTree, initializerGapSpan) == false
                && SyntaxTriviaUtilities.WouldJoinAcrossUnjoinableTrivia(propertyDeclaration.AccessorList.CloseBraceToken, propertyDeclaration.Initializer.EqualsToken))
            {
                return false;
            }

            if (SyntaxNodeUtilities.IsSingleLineSpan(propertyDeclaration.SyntaxTree, propertyDeclaration.Initializer.Value.Span) == false)
            {
                return false;
            }

            // The reported span ends at the terminating semicolon, so a semicolon broken onto its own line makes the
            // declaration multi-line. DeclarationSemicolonLineBreakRewriter joins that gap, but it refuses to join
            // across a comment or directive, so the analyzer must guard the same gap to avoid a permanent diagnostic.
            // The line check keeps the guard aligned with the rewriter, which only acts on a semicolon that carries a
            // leading line break: trivia in a gap that already sits on one line is never crossed. Such a comment is
            // trailing trivia of the initializer value, which the interior guard above deliberately no longer owns,
            // so this guard is its only owner once the gap spans lines.
            var tokenBeforeSemicolon = propertyDeclaration.SemicolonToken.GetPreviousToken();

            if (propertyDeclaration.SemicolonToken.IsMissing == false
                && tokenBeforeSemicolon.IsKind(SyntaxKind.None) == false
                && SyntaxNodeUtilities.IsSingleLineSpan(propertyDeclaration.SyntaxTree, TextSpan.FromBounds(tokenBeforeSemicolon.Span.End, propertyDeclaration.SemicolonToken.SpanStart)) == false
                && SyntaxTriviaUtilities.WouldJoinAcrossUnjoinableTrivia(tokenBeforeSemicolon, propertyDeclaration.SemicolonToken))
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

        if (SyntaxNodeUtilities.IsSingleLineSpan(propertyDeclaration.SyntaxTree, relevantSpan) == false)
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