using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Spacing;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Spacing;

/// <summary>
/// Code fix provider for <see cref="RH6023AssignmentOperatorsMustBeSpacedCorrectlyAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH6023AssignmentOperatorsMustBeSpacedCorrectlyCodeFixProvider))]
public class RH6023AssignmentOperatorsMustBeSpacedCorrectlyCodeFixProvider : OperatorSpacingCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH6023AssignmentOperatorsMustBeSpacedCorrectlyCodeFixProvider()
        : base(RH6023AssignmentOperatorsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, CodeFixResources.RH6023Title)
    {
    }

    #endregion // Constructor
}