using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5049: Comments must not be placed after a binary operator
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5049CommentsMustNotBePlacedAfterBinaryOperatorAnalyzer : CommentPositionAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5049";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5049CommentsMustNotBePlacedAfterBinaryOperatorAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH5049Title), nameof(AnalyzerResources.RH5049MessageFormat))
    {
    }

    #endregion // Constructor

    #region CommentPositionAnalyzerBase

    /// <inheritdoc/>
    protected override bool IsMisplaced(SyntaxTrivia comment, SyntaxToken previousToken, SyntaxToken nextToken)
    {
        return CommentPositionUtilities.IsBinaryOperator(previousToken);
    }

    #endregion // CommentPositionAnalyzerBase
}