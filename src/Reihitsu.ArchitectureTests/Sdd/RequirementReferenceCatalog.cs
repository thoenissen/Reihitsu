using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.ArchitectureTests.Sdd;

/// <summary>
/// Reads the <c>[Requirement]</c> attributes from the sources of the test projects under <c>src/</c>. The sources
/// are parsed rather than the assemblies loaded, so every test project counts whether or not this project
/// references it
/// </summary>
internal static class RequirementReferenceCatalog
{
    #region Constants

    /// <summary>
    /// Simple name of the requirement attribute
    /// </summary>
    private const string AttributeName = "Requirement";

    /// <summary>
    /// Simple name of the MSTest attribute that marks a test method
    /// </summary>
    private const string TestMethodAttributeName = "TestMethod";

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Reads every requirement attribute declared in a test project
    /// </summary>
    /// <param name="repositoryRoot">Repository root containing <c>src</c></param>
    /// <returns>The requirement references, in file and line order</returns>
    public static IReadOnlyList<RequirementReference> ReadReferences(string repositoryRoot)
    {
        var references = new List<RequirementReference>();

        foreach (var file in EnumerateTestFiles(Path.Combine(repositoryRoot, "src")))
        {
            var filePath = Path.GetRelativePath(repositoryRoot, file).Replace('\\', '/');
            var root = CSharpSyntaxTree.ParseText(File.ReadAllText(file)).GetRoot();

            foreach (var attribute in root.DescendantNodes().OfType<AttributeSyntax>())
            {
                if (IsNamed(attribute, AttributeName) == false)
                {
                    continue;
                }

                var method = attribute.FirstAncestorOrSelf<MethodDeclarationSyntax>();
                var isOnTestMethod = method?.AttributeLists
                                           .SelectMany(list => list.Attributes)
                                           .Any(candidate => IsNamed(candidate, TestMethodAttributeName)) == true;
                var line = attribute.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

                references.Add(new RequirementReference(filePath, line, GetLiteralId(attribute), isOnTestMethod));
            }
        }

        return references;
    }

    /// <summary>
    /// Determines whether an attribute has the given simple name, with or without the <c>Attribute</c> suffix and
    /// with or without a qualifier
    /// </summary>
    /// <param name="attribute">Attribute</param>
    /// <param name="simpleName">Simple name without the <c>Attribute</c> suffix</param>
    /// <returns><see langword="true"/> when the names match</returns>
    private static bool IsNamed(AttributeSyntax attribute, string simpleName)
    {
        var name = attribute.Name switch
                   {
                       QualifiedNameSyntax qualified => qualified.Right.Identifier.ValueText,
                       AliasQualifiedNameSyntax aliasQualified => aliasQualified.Name.Identifier.ValueText,
                       SimpleNameSyntax simple => simple.Identifier.ValueText,
                       _ => string.Empty
                   };

        return string.Equals(name, simpleName, StringComparison.Ordinal)
               || string.Equals(name, simpleName + "Attribute", StringComparison.Ordinal);
    }

    /// <summary>
    /// Reads the requirement ID of an attribute whose only argument is a string literal
    /// </summary>
    /// <param name="attribute">Requirement attribute</param>
    /// <returns>The ID, or <see langword="null"/> when the argument list is not a single string literal</returns>
    private static string GetLiteralId(AttributeSyntax attribute)
    {
        if (attribute.ArgumentList?.Arguments.Count == 1
            && attribute.ArgumentList.Arguments[0].Expression is LiteralExpressionSyntax literal
            && literal.IsKind(SyntaxKind.StringLiteralExpression))
        {
            return literal.Token.ValueText;
        }

        return null;
    }

    /// <summary>
    /// Enumerates the C# files of the test projects, skipping build output
    /// </summary>
    /// <param name="sourceDirectory">The <c>src</c> directory</param>
    /// <returns>Full paths of the test sources, in ordinal order</returns>
    private static IEnumerable<string> EnumerateTestFiles(string sourceDirectory)
    {
        if (Directory.Exists(sourceDirectory) == false)
        {
            yield break;
        }

        foreach (var projectDirectory in Directory.EnumerateDirectories(sourceDirectory).OrderBy(path => path, StringComparer.Ordinal))
        {
            var projectName = Path.GetFileName(projectDirectory);

            if (projectName.EndsWith(".Test", StringComparison.Ordinal) == false
                && projectName.EndsWith("Tests", StringComparison.Ordinal) == false)
            {
                continue;
            }

            foreach (var file in Directory.EnumerateFiles(projectDirectory, "*.cs", SearchOption.AllDirectories).OrderBy(path => path, StringComparer.Ordinal))
            {
                var segments = Path.GetRelativePath(projectDirectory, file).Replace('\\', '/').Split('/');

                if (segments.Contains("bin", StringComparer.Ordinal) == false
                    && segments.Contains("obj", StringComparer.Ordinal) == false)
                {
                    yield return file;
                }
            }
        }
    }

    #endregion // Methods
}