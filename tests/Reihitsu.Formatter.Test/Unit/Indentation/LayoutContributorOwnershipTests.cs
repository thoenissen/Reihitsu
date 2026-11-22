using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.Indentation.Utilities;

namespace Reihitsu.Formatter.Test.Unit.Indentation;

/// <summary>
/// Tests that at most one alignment contributor places a given line: the alignment sweeps of
/// <see cref="LayoutComputer.Compute"/> give every line its column through one construct, so no line's column is the
/// result of one contributor overriding another
/// </summary>
[TestClass]
public class LayoutContributorOwnershipTests
{
    #region Properties

    /// <summary>
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that at most one alignment contributor places each line of constructs that nest one aligned construct in
    /// another
    /// </summary>
    /// <param name="source">The source text</param>
    [TestMethod]
    [DataRow("using System.Linq;\n\nclass C\n{\n    object M(int[] items)\n    {\n        return Run(items.Where(x => x > 0)\n                        .Select(x => x),\n                   2);\n    }\n\n    object Run(object a, int b) => a;\n}")]
    [DataRow("class C\n{\n    void M()\n    {\n        Run(1,\n            x =>\n            {\n                return x;\n            });\n    }\n\n    void Run(int a, System.Func<int, int> f)\n    {\n    }\n}")]
    [DataRow("class C\n{\n    int A;\n    int B;\n\n    void M()\n    {\n        Run(new C\n            {\n                A = 1,\n                B = 2\n            },\n            3);\n    }\n\n    void Run(C c, int b)\n    {\n    }\n}")]
    [DataRow("class C\n{\n    int M(bool a, int b, int c)\n    {\n        return Run(a\n                       ? b\n                       : c,\n                   b + c\n                   + 1);\n    }\n\n    int Run(int a, int b) => a;\n}")]
    [DataRow("class C\n{\n    System.Func<int, int> M()\n    {\n        return x => x switch\n                    {\n                        1 => 2,\n                        _ => 3\n                    };\n    }\n}")]
    [DataRow("class C\n{\n    int[] M(int a)\n    {\n        return Run([a,\n                    a + 1],\n                   a);\n    }\n\n    int[] Run(int[] a, int b) => a;\n}")]
    [DataRow("class C\n{\n    bool M(object o)\n    {\n        return o is string { Length: > 0 } s\n               && s.Length < 10\n               && s != \"x\";\n    }\n}")]
    [DataRow("class C\n{\n    object M(int a)\n    {\n        return Run(new\n                   {\n                       A = a,\n                       B = a\n                           .ToString()\n                   });\n    }\n\n    object Run(object o) => o;\n}")]
    [DataRow("class C<T, U> : System.IComparable<T>,\n                System.IEquatable<U>\n    where T : class,\n              new()\n    where U : struct\n{\n    public int CompareTo(T other) => 0;\n\n    public bool Equals(U other) => true;\n}")]
    [DataRow("class C\n{\n    [System.Obsolete(\"a\",\n                     true)]\n    void M()\n    {\n    }\n}")]
    [DataRow("using System.Linq;\n\nclass C\n{\n    object M(int[] items)\n    {\n        return items.Select(x => new\n                                 {\n                                     A = x,\n                                     B = Run(x,\n                                             x)\n                                 })\n                    .ToList();\n    }\n\n    int Run(int a, int b) => a;\n}")]
    [DataRow("class C\n{\n    string M(int a)\n    {\n        return \"a\"\n               + Run(a,\n                     a)\n               + \"b\";\n    }\n\n    string Run(int a, int b) => string.Empty;\n}")]
    public void AtMostOneAlignmentContributorPlacesEachLineOfNestedConstructs(string source)
    {
        var violations = FindLinesPlacedByMoreThanOneContributor(source);

        Assert.IsEmpty(violations, string.Join(Environment.NewLine, violations));
    }

    /// <summary>
    /// Verifies that at most one alignment contributor places each line of every source file of the shipped projects
    /// </summary>
    [TestMethod]
    public void AtMostOneAlignmentContributorPlacesEachLineOfShippedSources()
    {
        var root = FindRepositoryRoot();
        var violations = new List<string>();

        foreach (var project in new[] { "Reihitsu.Formatter", "Reihitsu.Core", "Reihitsu.Analyzer", "Reihitsu.Analyzer.CodeFixes", "Reihitsu.Cli" })
        {
            var projectDirectory = Path.Combine(root, "src", project);

            foreach (var file in Directory.EnumerateFiles(projectDirectory, "*.cs", SearchOption.AllDirectories)
                                          .Where(path => IsBuildOutput(Path.GetRelativePath(projectDirectory, path)) == false)
                                          .OrderBy(path => path, StringComparer.Ordinal))
            {
                var relativePath = Path.GetRelativePath(root, file).Replace('\\', '/');

                violations.AddRange(FindLinesPlacedByMoreThanOneContributor(File.ReadAllText(file)).Select(violation => $"{relativePath}: {violation}"));
            }
        }

        Assert.IsEmpty(violations, string.Join(Environment.NewLine, violations));
    }

    /// <summary>
    /// Determines whether a path relative to a project directory lies in its build output
    /// </summary>
    /// <param name="relativePath">The relative path</param>
    /// <returns><see langword="true"/> when the path lies below <c>bin</c> or <c>obj</c>; otherwise, <see langword="false"/></returns>
    private static bool IsBuildOutput(string relativePath)
    {
        var firstSegment = relativePath.Replace('\\', '/').Split('/')[0];

        return firstSegment is "bin" or "obj";
    }

    /// <summary>
    /// Finds the directory that holds the solution file, walking up from the test's base directory
    /// </summary>
    /// <returns>The repository root</returns>
    private static string FindRepositoryRoot()
    {
        var directory = AppContext.BaseDirectory;

        while (directory != null)
        {
            if (File.Exists(Path.Combine(directory, "Reihitsu.sln")))
            {
                return directory;
            }

            directory = Path.GetDirectoryName(directory);
        }

        throw new InvalidOperationException("Could not find the repository root (Reihitsu.sln).");
    }

    /// <summary>
    /// Computes the layout of the source and lists every line that more than one alignment contributor placed
    /// </summary>
    /// <param name="source">The source text</param>
    /// <returns>One entry per such line, naming the 1-based line and the contributors</returns>
    private List<string> FindLinesPlacedByMoreThanOneContributor(string source)
    {
        var tree = CSharpSyntaxTree.ParseText(source, cancellationToken: TestContext.CancellationToken);
        var model = LayoutComputer.Compute(tree.GetRoot(TestContext.CancellationToken), new FormattingContext("\n"), TestContext.CancellationToken);
        var violations = new List<string>();
        var lineCount = tree.GetText(TestContext.CancellationToken).Lines.Count;

        for (var line = 0; line < lineCount; line++)
        {
            var contributors = model.GetAlignmentContributors(line);

            if (contributors.Count > 1)
            {
                violations.Add($"line {line + 1} is placed by {string.Join(", ", contributors.Select(contributor => contributor.Name).OrderBy(name => name, StringComparer.Ordinal))}");
            }
        }

        return violations;
    }

    #endregion // Methods
}