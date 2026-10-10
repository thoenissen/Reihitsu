using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Layout;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Providing fixes for <see cref="RH5029LocalDeclarationsShouldBePrecededByABlankLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5029LocalDeclarationsShouldBePrecededByABlankLineCodeFixProvider))]
public class RH5029LocalDeclarationsShouldBePrecededByABlankLineCodeFixProvider : StatementShouldBePrecededByABlankLineCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5029LocalDeclarationsShouldBePrecededByABlankLineCodeFixProvider()
        : base(RH5029LocalDeclarationsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, CodeFixResources.RH5029Title)
    {
    }

    #endregion // Constructor
}