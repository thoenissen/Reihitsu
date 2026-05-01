using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.Base;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// Providing fixes for <see cref="RH0323LocalDeclarationsShouldBeFollowedByABlankLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0323LocalDeclarationsShouldBeFollowedByABlankLineCodeFixProvider))]
public class RH0323LocalDeclarationsShouldBeFollowedByABlankLineCodeFixProvider : StatementShouldBePrecededByABlankLineCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0323LocalDeclarationsShouldBeFollowedByABlankLineCodeFixProvider()
        : base(RH0323LocalDeclarationsShouldBeFollowedByABlankLineAnalyzer.DiagnosticId, CodeFixResources.RH0323Title)
    {
    }

    #endregion // Constructor
}