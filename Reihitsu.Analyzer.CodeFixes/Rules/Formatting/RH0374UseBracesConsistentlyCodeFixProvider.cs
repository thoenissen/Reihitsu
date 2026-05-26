using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.Base;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// Code fix provider for <see cref="RH0374UseBracesConsistentlyAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0374UseBracesConsistentlyCodeFixProvider))]
public class RH0374UseBracesConsistentlyCodeFixProvider : StatementBracesCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0374UseBracesConsistentlyCodeFixProvider()
        : base(RH0374UseBracesConsistentlyAnalyzer.DiagnosticId, CodeFixResources.RH0374Title)
    {
    }

    #endregion // Constructor
}