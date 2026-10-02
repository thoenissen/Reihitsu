using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Clarity;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Clarity;

/// <summary>
/// Code fix provider for <see cref="RH3205ExpressionStyleOperatorsShouldNotBeUsedAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH3205ExpressionStyleOperatorsShouldNotBeUsedCodeFixProvider))]
public class RH3205ExpressionStyleOperatorsShouldNotBeUsedCodeFixProvider : ExpressionBodyToBlockCodeFixProviderBase<OperatorDeclarationSyntax>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH3205ExpressionStyleOperatorsShouldNotBeUsedCodeFixProvider()
        : base(RH3205ExpressionStyleOperatorsShouldNotBeUsedAnalyzer.DiagnosticId, CodeFixResources.RH3205Title)
    {
    }

    #endregion // Constructor
}