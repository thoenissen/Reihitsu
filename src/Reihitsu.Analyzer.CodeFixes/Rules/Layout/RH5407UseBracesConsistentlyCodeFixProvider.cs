using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Layout;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Code fix provider for <see cref="RH5407UseBracesConsistentlyAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5407UseBracesConsistentlyCodeFixProvider))]
public class RH5407UseBracesConsistentlyCodeFixProvider : StatementBracesCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5407UseBracesConsistentlyCodeFixProvider()
        : base(RH5407UseBracesConsistentlyAnalyzer.DiagnosticId, CodeFixResources.RH5407Title)
    {
    }

    #endregion // Constructor
}