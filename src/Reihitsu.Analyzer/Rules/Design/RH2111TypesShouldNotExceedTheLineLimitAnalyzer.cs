using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;

namespace Reihitsu.Analyzer.Rules.Design;

/// <summary>
/// RH2111: Types should not exceed the line limit
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH2111TypesShouldNotExceedTheLineLimitAnalyzer : LineCountAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH2111";

    /// <summary>
    /// Default limit of a type's body in production code
    /// </summary>
    private const int DefaultProductionLimit = 500;

    /// <summary>
    /// Default limit of a type's body in test code
    /// </summary>
    private const int DefaultTestLimit = 1000;

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH2111TypesShouldNotExceedTheLineLimitAnalyzer()
        : base(DiagnosticId,
               nameof(AnalyzerResources.RH2111Title),
               nameof(AnalyzerResources.RH2111MessageFormat),
               design => design.MaximumTypeLines,
               DefaultProductionLimit,
               design => design.MaximumTestTypeLines,
               DefaultTestLimit,
               SyntaxKind.ClassDeclaration,
               SyntaxKind.StructDeclaration,
               SyntaxKind.InterfaceDeclaration,
               SyntaxKind.RecordDeclaration,
               SyntaxKind.RecordStructDeclaration,
               SyntaxKind.UnionDeclaration)
    {
    }

    #endregion // Constructor

    #region LineCountAnalyzerBase

    /// <summary>
    /// Gets the span between the braces of a type declaration with a brace body, and the type's identifier. A
    /// declaration that ends with <c>;</c> instead of a brace body is not judged
    /// </summary>
    /// <param name="node">Declaration of one of the registered syntax kinds</param>
    /// <param name="span">Counted span</param>
    /// <param name="location">Location of the diagnostic</param>
    /// <returns><see langword="true"/> when the type has a brace body; otherwise <see langword="false"/></returns>
    private protected override bool TryGetCountedSpan(SyntaxNode node, out TextSpan span, out Location location)
    {
        if (node is TypeDeclarationSyntax typeDeclaration
            && typeDeclaration.OpenBraceToken.IsKind(SyntaxKind.OpenBraceToken))
        {
            span = GetSpanBetweenBraces(typeDeclaration.OpenBraceToken, typeDeclaration.CloseBraceToken);
            location = typeDeclaration.Identifier.GetLocation();

            return true;
        }

        span = default;
        location = null;

        return false;
    }

    #endregion // LineCountAnalyzerBase
}