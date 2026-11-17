using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.ArchitectureTests.General;

/// <summary>
/// Pins the type-declaration syntax types and syntax kinds of the Roslyn version the repository builds against
/// </summary>
[TestClass]
public sealed class TypeDeclarationKindsTests
{
    #region Constants

    /// <summary>
    /// Source holding one declaration of every type-declaration kind <see cref="SyntaxFactory.TypeDeclaration(SyntaxKind, string)"/> does not create
    /// </summary>
    private const string DeclarationsNotCreatedByTheFactory = """
                                                              internal static class Extensions
                                                              {
                                                                  extension(int value)
                                                                  {
                                                                  }
                                                              }

                                                              internal enum Color
                                                              {
                                                                  Red
                                                              }

                                                              internal delegate void Handler();
                                                              """;

    /// <summary>
    /// Text that marks a type-declaration kind no declaration was found for
    /// </summary>
    private const string UnpairedType = "(no syntax type)";

    /// <summary>
    /// Text that marks a type-declaration syntax type no kind was found for
    /// </summary>
    private const string UnpairedKind = "(no syntax kind)";

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Verifies that the type-declaration syntax types and the syntax kinds each of them carries are still the ones
    /// the repository was written against. Analyzer registrations, Core utilities, and formatter rewriters each select
    /// type declarations with their own explicit list of kinds or types, so a type or kind added by a later Roslyn
    /// version would silently fall out of every list. This test fails when that happens, including when an existing
    /// syntax type gains a kind, as <see cref="RecordDeclarationSyntax"/> did with
    /// <see cref="SyntaxKind.RecordStructDeclaration"/>.
    /// </summary>
    [TestMethod]
    public void TypeDeclarationSyntaxTypesAndKindsAreFullyEnumerated()
    {
        // Arrange
        string[] expected = [
                                "ClassDeclarationSyntax: ClassDeclaration",
                                "DelegateDeclarationSyntax: DelegateDeclaration",
                                "EnumDeclarationSyntax: EnumDeclaration",
                                "ExtensionBlockDeclarationSyntax: ExtensionBlockDeclaration",
                                "InterfaceDeclarationSyntax: InterfaceDeclaration",
                                "RecordDeclarationSyntax: RecordDeclaration",
                                "RecordDeclarationSyntax: RecordStructDeclaration",
                                "StructDeclarationSyntax: StructDeclaration",
                                "UnionDeclarationSyntax: UnionDeclaration"
                            ];

        // Act
        var actual = GetTypeDeclarationPairs();

        // Assert
        Assert.AreSequenceEqual(expected,
                                actual,
                                $"""
                                 The type-declaration syntax types or kinds of Roslyn changed: [{string.Join(", ", actual)}].
                                 Every explicit list of type-declaration kinds or types in the analyzers, Reihitsu.Core, and the formatter
                                 has to be reviewed for the change, together with each spec under sdd/specs/ that lists the type kinds a rule
                                 or phase covers; then update the expected pairs of this test.
                                 """);
    }

    /// <summary>
    /// Gets every pair of a type-declaration syntax type and a syntax kind it carries, sorted ordinally
    /// </summary>
    /// <returns>The pairs as <c>Type: Kind</c>, with a marker for a type or kind no partner was found for</returns>
    private static string[] GetTypeDeclarationPairs()
    {
        var pairs = new SortedSet<string>(StringComparer.Ordinal);
        var pairedKinds = new HashSet<SyntaxKind>();
        var pairedTypes = new HashSet<Type>();

        void AddPair(Type type, SyntaxKind kind)
        {
            pairs.Add($"{type.Name}: {kind}");
            pairedKinds.Add(kind);
            pairedTypes.Add(type);
        }

        var allKinds = Enum.GetValues<SyntaxKind>()
                           .Distinct()
                           .ToArray();

        foreach (var kind in allKinds)
        {
            TypeDeclarationSyntax declaration;

            try
            {
                declaration = SyntaxFactory.TypeDeclaration(kind, "Sample");
            }
            catch (InvalidOperationException)
            {
                continue;
            }
            catch (ArgumentException)
            {
                continue;
            }

            AddPair(declaration.GetType(), declaration.Kind());
        }

        var root = CSharpSyntaxTree.ParseText(DeclarationsNotCreatedByTheFactory, new CSharpParseOptions(LanguageVersion.Preview))
                                   .GetRoot();

        Assert.IsFalse(root.ContainsDiagnostics, "The declarations source of this test does not parse.");

        foreach (var node in root.DescendantNodes())
        {
            if (node is BaseTypeDeclarationSyntax
                || SyntaxFacts.IsTypeDeclaration(node.Kind()))
            {
                AddPair(node.GetType(), node.Kind());
            }
        }

        foreach (var kind in allKinds.Where(SyntaxFacts.IsTypeDeclaration)
                                     .Where(kind => pairedKinds.Contains(kind) == false))
        {
            pairs.Add($"{UnpairedType}: {kind}");
        }

        foreach (var type in typeof(BaseTypeDeclarationSyntax).Assembly
                                                              .GetTypes()
                                                              .Where(type => type.IsAbstract == false
                                                                             && typeof(BaseTypeDeclarationSyntax).IsAssignableFrom(type)
                                                                             && pairedTypes.Contains(type) == false))
        {
            pairs.Add($"{type.Name}: {UnpairedKind}");
        }

        return [.. pairs];
    }

    #endregion // Methods
}