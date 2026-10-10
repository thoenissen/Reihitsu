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
/// Code fix provider for <see cref="RH6018NegativeSignsMustBeSpacedCorrectlyAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH6018NegativeSignsMustBeSpacedCorrectlyCodeFixProvider))]
public class RH6018NegativeSignsMustBeSpacedCorrectlyCodeFixProvider : RemoveWhitespaceRunCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH6018NegativeSignsMustBeSpacedCorrectlyCodeFixProvider()
        : base(RH6018NegativeSignsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, CodeFixResources.RH6018Title)
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