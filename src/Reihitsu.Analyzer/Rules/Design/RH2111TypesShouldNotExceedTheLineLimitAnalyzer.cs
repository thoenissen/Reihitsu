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
    private const int DefaultProductionLimit = 1500;

    /// <summary>
    /// Default limit of a type's body in test code
    /// </summary>
    private const int DefaultTestLimit = 3000;

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH2111TypesShouldNotExceedTheLineLimitAnalyzer()
        : base(DiagnosticId,
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

    /// <inheritdoc/>
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