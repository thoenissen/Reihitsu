using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Clarity;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Clarity;

/// <summary>
/// Code fix provider for <see cref="RH3207ExpressionStyleFinalizersShouldNotBeUsedAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH3207ExpressionStyleFinalizersShouldNotBeUsedCodeFixProvider))]
public class RH3207ExpressionStyleFinalizersShouldNotBeUsedCodeFixProvider : ExpressionBodyToBlockCodeFixProviderBase<DestructorDeclarationSyntax>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH3207ExpressionStyleFinalizersShouldNotBeUsedCodeFixProvider()
        : base(RH3207ExpressionStyleFinalizersShouldNotBeUsedAnalyzer.DiagnosticId, CodeFixResources.RH3207Title)
    {
    }

    #endregion // Constructor
}