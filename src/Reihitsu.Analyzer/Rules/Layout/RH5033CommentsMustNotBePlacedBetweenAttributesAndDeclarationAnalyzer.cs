using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5033: Comments must not be placed between attributes and the declaration
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5033CommentsMustNotBePlacedBetweenAttributesAndDeclarationAnalyzer : CommentPositionAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5033";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5033CommentsMustNotBePlacedBetweenAttributesAndDeclarationAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH5033Title), nameof(AnalyzerResources.RH5033MessageFormat))
    {
    }

    #endregion // Constructor

    #region CommentPositionAnalyzerBase

    /// <inheritdoc/>
    protected override bool IsMisplaced(SyntaxTrivia comment, SyntaxToken previousToken, SyntaxToken nextToken)
    {
        return previousToken.Parent is AttributeListSyntax attributeList
               && attributeList.CloseBracketToken == previousToken
               && attributeList.Parent is not CompilationUnitSyntax;
    }

    #endregion // CommentPositionAnalyzerBase
}