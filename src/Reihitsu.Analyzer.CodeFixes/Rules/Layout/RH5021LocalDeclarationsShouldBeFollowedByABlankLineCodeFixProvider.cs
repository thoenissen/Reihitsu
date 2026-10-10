using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Layout;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Providing fixes for <see cref="RH5021LocalDeclarationsShouldBeFollowedByABlankLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5021LocalDeclarationsShouldBeFollowedByABlankLineCodeFixProvider))]
public class RH5021LocalDeclarationsShouldBeFollowedByABlankLineCodeFixProvider : StatementShouldBePrecededByABlankLineCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5021LocalDeclarationsShouldBeFollowedByABlankLineCodeFixProvider()
        : base(RH5021LocalDeclarationsShouldBeFollowedByABlankLineAnalyzer.DiagnosticId, CodeFixResources.RH5021Title)
    {
    }

    #endregion // Constructor
}