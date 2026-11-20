using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5409: Final enum member must not have trailing comma
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5409FinalEnumMemberMustNotHaveTrailingCommaAnalyzer : TrailingCommaAnalyzerBase<EnumDeclarationSyntax>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5409";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5409FinalEnumMemberMustNotHaveTrailingCommaAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Layout, SyntaxKind.EnumDeclaration)
    {
    }

    #endregion // Constructor

    #region TrailingCommaAnalyzerBase

    /// <inheritdoc/>
    protected override SyntaxNodeOrTokenList GetElementsWithSeparators(EnumDeclarationSyntax node)
    {
        return node.Members.GetWithSeparators();
    }

    #endregion // TrailingCommaAnalyzerBase
}