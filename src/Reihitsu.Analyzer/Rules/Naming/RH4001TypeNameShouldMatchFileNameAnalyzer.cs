using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Naming;

/// <summary>
/// RH4001: The name of the type should match the filename
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH4001TypeNameShouldMatchFileNameAnalyzer : DiagnosticAnalyzerBase
{
    #region Fields

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH4001";

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH4001TypeNameShouldMatchFileNameAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Naming)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Finds the first type declaration in the syntax tree
    /// </summary>
    /// <param name="node">Root node</param>
    /// <returns>The first type declaration or <c>null</c></returns>
    private static MemberDeclarationSyntax FindFirstTypeDeclaration(SyntaxNode node)
    {
        foreach (var child in node.ChildNodes())
        {
            if (child is BaseTypeDeclarationSyntax or DelegateDeclarationSyntax)
            {
                return (MemberDeclarationSyntax)child;
            }

            if (child is BaseNamespaceDeclarationSyntax)
            {
                var declaration = FindFirstTypeDeclaration(child);

                if (declaration != null)
                {
                    return declaration;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Determines whether the compilation unit holds a global statement among its members, which makes the file one with top-level statements
    /// </summary>
    /// <param name="root">Root node</param>
    /// <returns><see langword="true"/> if the file holds top-level statements</returns>
    private static bool HasTopLevelStatements(SyntaxNode root)
    {
        return root is CompilationUnitSyntax compilationUnit
               && compilationUnit.Members.Any(member => member is GlobalStatementSyntax);
    }

    /// <summary>
    /// Gets the identifier token from a type declaration
    /// </summary>
    /// <param name="typeDeclaration">Type declaration</param>
    /// <returns>The identifier token</returns>
    private static SyntaxToken GetIdentifier(MemberDeclarationSyntax typeDeclaration)
    {
        return typeDeclaration switch
               {
                   BaseTypeDeclarationSyntax baseType => baseType.Identifier,
                   DelegateDeclarationSyntax delegateType => delegateType.Identifier,
                   _ => default
               };
    }

    /// <summary>
    /// Analyzing syntax trees for type-filename mismatch; a file with top-level statements is not checked
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSyntaxTreeAction(SyntaxTreeAnalysisContext context)
    {
        var filePath = context.Tree.FilePath;

        if (string.IsNullOrEmpty(filePath))
        {
            return;
        }

        var fileName = RH4001TypeNameShouldMatchFileNameHelper.GetComparableFileName(filePath);

        var root = context.Tree.GetRoot(context.CancellationToken);

        // A file with top-level statements is the program's entry point, not the home of the types declared next to it
        if (HasTopLevelStatements(root))
        {
            return;
        }

        var firstTypeDeclaration = FindFirstTypeDeclaration(root);

        if (firstTypeDeclaration == null)
        {
            return;
        }

        var expectedFileName = RH4001TypeNameShouldMatchFileNameHelper.GetExpectedFileNameStem(firstTypeDeclaration);

        if (fileName != expectedFileName)
        {
            context.ReportDiagnostic(CreateDiagnostic(GetIdentifier(firstTypeDeclaration).GetLocation()));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxTreeAction(OnSyntaxTreeAction);
    }

    #endregion // DiagnosticAnalyzer
}