using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Spacing;

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
}