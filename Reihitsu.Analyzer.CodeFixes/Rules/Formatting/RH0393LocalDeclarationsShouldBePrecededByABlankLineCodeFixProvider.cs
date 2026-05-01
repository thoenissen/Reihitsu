using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.Base;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// Providing fixes for <see cref="RH0393LocalDeclarationsShouldBePrecededByABlankLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0393LocalDeclarationsShouldBePrecededByABlankLineCodeFixProvider))]
public class RH0393LocalDeclarationsShouldBePrecededByABlankLineCodeFixProvider : StatementShouldBePrecededByABlankLineCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0393LocalDeclarationsShouldBePrecededByABlankLineCodeFixProvider()
        : base(RH0393LocalDeclarationsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, CodeFixResources.RH0393Title)
    {
    }

    #endregion // Constructor
}