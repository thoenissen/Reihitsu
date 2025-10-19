using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH0401: The &lt;inheritdoc&gt; Tag should be used if possible.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0401InheritdocShouldBeUsedAnalyzer : DiagnosticAnalyzerBase<RH0401InheritdocShouldBeUsedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0401";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0401InheritdocShouldBeUsedAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Documentation, nameof(AnalyzerResources.RH0401Title), nameof(AnalyzerResources.RH0401MessageFormat))
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

        context.RegisterSyntaxNodeAction(OnSingleLineDocumentationCommentTrivia, SyntaxKind.MethodDeclaration);
        context.RegisterSyntaxNodeAction(OnSingleLineDocumentationCommentTrivia, SyntaxKind.PropertyDeclaration);
        context.RegisterSyntaxNodeAction(OnSingleLineDocumentationCommentTrivia, SyntaxKind.EventDeclaration);
        context.RegisterSyntaxNodeAction(OnSingleLineDocumentationCommentTrivia, SyntaxKind.IndexerDeclaration);
    }

    #endregion // DiagnosticAnalyzer
}