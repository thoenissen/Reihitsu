using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Clarity;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Clarity;

/// <summary>
/// Code fix provider for <see cref="RH3206ExpressionStyleConversionOperatorsShouldNotBeUsedAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH3206ExpressionStyleConversionOperatorsShouldNotBeUsedCodeFixProvider))]
public class RH3206ExpressionStyleConversionOperatorsShouldNotBeUsedCodeFixProvider : ExpressionBodyToBlockCodeFixProviderBase<ConversionOperatorDeclarationSyntax>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH3206ExpressionStyleConversionOperatorsShouldNotBeUsedCodeFixProvider()
        : base(RH3206ExpressionStyleConversionOperatorsShouldNotBeUsedAnalyzer.DiagnosticId, CodeFixResources.RH3206Title)
    {
    }

    #endregion // Constructor
}