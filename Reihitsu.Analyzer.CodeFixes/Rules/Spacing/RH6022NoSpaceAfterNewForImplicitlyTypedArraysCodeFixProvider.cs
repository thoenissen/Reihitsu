using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Spacing;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Spacing;

/// <summary>
/// Code fix provider for <see cref="RH6022NoSpaceAfterNewForImplicitlyTypedArraysAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH6022NoSpaceAfterNewForImplicitlyTypedArraysCodeFixProvider))]
public class RH6022NoSpaceAfterNewForImplicitlyTypedArraysCodeFixProvider : RemoveWhitespaceRunCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH6022NoSpaceAfterNewForImplicitlyTypedArraysCodeFixProvider()
        : base(RH6022NoSpaceAfterNewForImplicitlyTypedArraysAnalyzer.DiagnosticId, CodeFixResources.RH6022Title)
    {
    }

    #endregion // Constructor
}