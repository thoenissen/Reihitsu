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
/// Code fix provider for <see cref="RH6020DereferenceAndAccessOfSymbolsMustBeSpacedCorrectlyAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH6020DereferenceAndAccessOfSymbolsMustBeSpacedCorrectlyCodeFixProvider))]
public class RH6020DereferenceAndAccessOfSymbolsMustBeSpacedCorrectlyCodeFixProvider : RemoveWhitespaceRunCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH6020DereferenceAndAccessOfSymbolsMustBeSpacedCorrectlyCodeFixProvider()
        : base(RH6020DereferenceAndAccessOfSymbolsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, CodeFixResources.RH6020Title)
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