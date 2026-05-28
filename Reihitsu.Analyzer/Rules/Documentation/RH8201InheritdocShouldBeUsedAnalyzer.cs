using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH8201: The &lt;inheritdoc&gt; Tag should be used if possible
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH8201InheritdocShouldBeUsedAnalyzer : DiagnosticAnalyzerBase<RH8201InheritdocShouldBeUsedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH8201";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH8201InheritdocShouldBeUsedAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Documentation, nameof(AnalyzerResources.RH8201Title), nameof(AnalyzerResources.RH8201MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzing all <see cref="SyntaxKind.LogicalNotExpression"/> occurrences
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSingleLineDocumentationCommentTrivia(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is MemberDeclarationSyntax node
            && node.Modifiers.Any(SyntaxKind.OverrideKeyword))
        {
            var documentation = node.GetLeadingTrivia()
                                    .FirstOrDefault(obj => obj.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia));

            if (documentation != default
                && documentation.HasStructure)
            {
                static bool ContainsInheritDoc(SyntaxNode checkNode)
                {
                    return checkNode switch
                           {
                               XmlElementStartTagSyntax element => element.Name.LocalName.ValueText.Equals("inheritdoc", StringComparison.InvariantCultureIgnoreCase),
                               XmlEmptyElementSyntax element => element.Name.LocalName.ValueText.Equals("inheritdoc", StringComparison.InvariantCultureIgnoreCase),
                               _ => checkNode.ChildNodes().Any(ContainsInheritDoc)
                           };
                }

                if (ContainsInheritDoc(documentation.GetStructure()) == false)
                {
                    context.ReportDiagnostic(CreateDiagnostic(documentation.GetLocation()));
                }
            }
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeActionWithDocumentationModeCheck(OnSingleLineDocumentationCommentTrivia, SyntaxKind.MethodDeclaration);
        context.RegisterSyntaxNodeActionWithDocumentationModeCheck(OnSingleLineDocumentationCommentTrivia, SyntaxKind.PropertyDeclaration);
        context.RegisterSyntaxNodeActionWithDocumentationModeCheck(OnSingleLineDocumentationCommentTrivia, SyntaxKind.EventDeclaration);
        context.RegisterSyntaxNodeActionWithDocumentationModeCheck(OnSingleLineDocumentationCommentTrivia, SyntaxKind.IndexerDeclaration);
    }

    #endregion // DiagnosticAnalyzer
}