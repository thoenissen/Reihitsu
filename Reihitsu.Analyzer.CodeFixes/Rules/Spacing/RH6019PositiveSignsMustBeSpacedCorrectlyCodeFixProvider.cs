using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Spacing;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Spacing;

/// <summary>
/// Code fix provider for <see cref="RH6019PositiveSignsMustBeSpacedCorrectlyAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH6019PositiveSignsMustBeSpacedCorrectlyCodeFixProvider))]
public class RH6019PositiveSignsMustBeSpacedCorrectlyCodeFixProvider : RemoveWhitespaceRunCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH6019PositiveSignsMustBeSpacedCorrectlyCodeFixProvider()
        : base(RH6019PositiveSignsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, CodeFixResources.RH6019Title)
    {
    }

    #endregion // Constructor

    #region RemoveWhitespaceRunCodeFixProviderBase

    /// <inheritdoc/>
    protected override bool CanOfferFix(SyntaxNode root, TextSpan diagnosticSpan)
    {
        return root.FindNode(diagnosticSpan) is not PrefixUnaryExpressionSyntax node
               || UnaryOperatorSpacingUtilities.WouldGlueIntoDifferentOperator(node) == false;
    }

    #endregion // RemoveWhitespaceRunCodeFixProviderBase
}