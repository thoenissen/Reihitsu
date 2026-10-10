using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5041: Comments must not be placed inside a declarator list
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5041CommentsMustNotBePlacedInsideDeclaratorListAnalyzer : CommentPositionAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5041";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5041CommentsMustNotBePlacedInsideDeclaratorListAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH5041Title), nameof(AnalyzerResources.RH5041MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether the token is a comma separating two declarators
    /// </summary>
    /// <param name="token">Token</param>
    /// <returns><see langword="true"/> if the token separates declarators</returns>
    private static bool IsDeclaratorSeparator(SyntaxToken token)
    {
        return token.IsKind(SyntaxKind.CommaToken)
               && token.Parent is VariableDeclarationSyntax;
    }

    /// <summary>
    /// Determines whether the token terminates a field, event field, or local declaration
    /// </summary>
    /// <param name="token">Token</param>
    /// <returns><see langword="true"/> if the token terminates a declaration</returns>
    private static bool IsDeclarationTerminator(SyntaxToken token)
    {
        return token.IsKind(SyntaxKind.SemicolonToken)
               && token.Parent is BaseFieldDeclarationSyntax or LocalDeclarationStatementSyntax;
    }

    #endregion // Methods

    #region CommentPositionAnalyzerBase

    /// <inheritdoc/>
    protected override bool IsMisplaced(SyntaxTrivia comment, SyntaxToken previousToken, SyntaxToken nextToken)
    {
        return IsDeclaratorSeparator(previousToken)
               || IsDeclaratorSeparator(nextToken)
               || IsDeclarationTerminator(nextToken);
    }

    #endregion // CommentPositionAnalyzerBase
}