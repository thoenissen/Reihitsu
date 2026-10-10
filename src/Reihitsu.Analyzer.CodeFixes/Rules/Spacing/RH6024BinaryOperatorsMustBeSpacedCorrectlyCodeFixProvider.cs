using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Spacing;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Spacing;

/// <summary>
/// Code fix provider for <see cref="RH6024BinaryOperatorsMustBeSpacedCorrectlyAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH6024BinaryOperatorsMustBeSpacedCorrectlyCodeFixProvider))]
public class RH6024BinaryOperatorsMustBeSpacedCorrectlyCodeFixProvider : OperatorSpacingCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH6024BinaryOperatorsMustBeSpacedCorrectlyCodeFixProvider()
        : base(RH6024BinaryOperatorsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, CodeFixResources.RH6024Title)
    {
    }

    #endregion // Constructor
}