using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Layout;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Code fix provider for <see cref="RH5026ChainedStatementBlocksMustNotBePrecededByBlankLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5026ChainedStatementBlocksMustNotBePrecededByBlankLineCodeFixProvider))]
public class RH5026ChainedStatementBlocksMustNotBePrecededByBlankLineCodeFixProvider : BlankLineSpanRemovalCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5026ChainedStatementBlocksMustNotBePrecededByBlankLineCodeFixProvider()
        : base(RH5026ChainedStatementBlocksMustNotBePrecededByBlankLineAnalyzer.DiagnosticId, CodeFixResources.RH5026Title)
    {
    }

    #endregion // Constructor
}