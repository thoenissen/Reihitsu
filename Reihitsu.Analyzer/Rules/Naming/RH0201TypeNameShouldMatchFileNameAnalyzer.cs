using System.IO;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Naming;

/// <summary>
/// RH0201: The name of the type should match the filename
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0201TypeNameShouldMatchFileNameAnalyzer : DiagnosticAnalyzerBase<RH0201TypeNameShouldMatchFileNameAnalyzer>
{
    #region Fields

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0201";

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0201TypeNameShouldMatchFileNameAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Naming, nameof(AnalyzerResources.RH0201Title), nameof(AnalyzerResources.RH0201MessageFormat))
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
                return FindFirstTypeDeclaration(child);
            }
        }

        return null;
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
    /// Gets the expected filename for the given type declaration, formatting generic type parameters with curly braces
    /// </summary>
    /// <param name="typeDeclaration">Type declaration</param>
    /// <returns>Expected filename without extension</returns>
    private static string GetExpectedFileName(MemberDeclarationSyntax typeDeclaration)
    {
        var typeName = GetIdentifier(typeDeclaration).Text;

        var typeParameterList = typeDeclaration switch
                                {
                                    TypeDeclarationSyntax typeSyntax => typeSyntax.TypeParameterList,
                                    DelegateDeclarationSyntax delegateSyntax => delegateSyntax.TypeParameterList,
                                    _ => null
                                };

        if (typeParameterList is { Parameters.Count: > 0 })
        {
            typeName += "{" + string.Join(",", typeParameterList.Parameters.Select(p => p.Identifier.Text)) + "}";
        }

        return typeName;
    }

    /// <summary>
    /// Analyzing syntax trees for type-filename mismatch
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSyntaxTreeAction(SyntaxTreeAnalysisContext context)
    {
        var filePath = context.Tree.FilePath;

        if (string.IsNullOrEmpty(filePath))
        {
            return;
        }

        var fileName = Path.GetFileNameWithoutExtension(filePath);

        // Handle .razor.cs files (Razor code-behind) - only if a corresponding .razor file exists in AdditionalFiles
        if (filePath.EndsWith(".razor.cs", StringComparison.OrdinalIgnoreCase))
        {
            var razorFilePath = filePath.Substring(0, filePath.Length - 3);

            if (context.Options.AdditionalFiles.Any(f => string.Equals(f.Path, razorFilePath, StringComparison.OrdinalIgnoreCase)))
            {
                fileName = Path.GetFileNameWithoutExtension(fileName);
            }
        }

        var root = context.Tree.GetRoot(context.CancellationToken);
        var firstTypeDeclaration = FindFirstTypeDeclaration(root);

        if (firstTypeDeclaration == null)
        {
            return;
        }

        var expectedFileName = GetExpectedFileName(firstTypeDeclaration);

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