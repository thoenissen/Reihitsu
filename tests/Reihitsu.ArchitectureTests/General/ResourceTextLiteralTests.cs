using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.ArchitectureTests.Helpers;

namespace Reihitsu.ArchitectureTests.General;

/// <summary>
/// Guards that no C# file under <c>tests/</c> quotes a diagnostic message, a diagnostic title, or a code-fix
/// title of <c>AnalyzerResources.resx</c> or <c>CodeFixResources.resx</c> as a string literal. A quoted text couples
/// the test to the wording of one rule, so a wording edit fails the test and widens a wording change to a test
/// change; tests read the text through the test base's message format or the resource class instead. A literal is
/// reported when its decoded value, or one text part of an interpolated string, contains a resource value; for a
/// value with placeholders, the longest fixed fragment between them stands for the value. Only literal tokens are
/// read, so text in a comment, a documentation comment, or disabled code is never reported. The scan takes its
/// resource texts as a parameter, so the self-tests below use synthetic texts and are not reported by the
/// repository scan
/// </summary>
[TestClass]
public sealed class ResourceTextLiteralTests
{
    #region Constants

    /// <summary>
    /// Synthetic resource text without placeholders, used by the self-tests
    /// </summary>
    private const string SyntheticText = "Synthetic resource text for the scan.";

    /// <summary>
    /// Synthetic resource value with placeholders, used by the self-tests
    /// </summary>
    private const string SyntheticFormat = "Member has {0} lines, over the limit of {1} for sure.";

    /// <summary>
    /// Longest fixed fragment of <see cref="SyntheticFormat"/>
    /// </summary>
    private const string SyntheticFormatFragment = " lines, over the limit of ";

    /// <summary>
    /// Repository-relative paths of the resource files whose values no test may quote
    /// </summary>
    private static readonly string[] _resourceFilePaths = [
                                                              Path.Combine("src", "Reihitsu.Analyzer", "AnalyzerResources.resx"),
                                                              Path.Combine("src", "Reihitsu.Analyzer.CodeFixes", "CodeFixResources.resx")
                                                          ];

    /// <summary>
    /// Matches a composite-format placeholder such as <c>{0}</c>, <c>{1,-5}</c>, or <c>{0:N0}</c>
    /// </summary>
    private static readonly Regex _placeholderPattern = new(@"\{\d+(?:,-?\d+)?(?::[^}]*)?\}");

    #endregion // Constants

    #region Tests

    /// <summary>
    /// Verifies that no C# file under <c>tests/</c> quotes a value of either resource file as a string literal
    /// </summary>
    [TestMethod]
    public void TestSourcesQuoteNoResourceText()
    {
        // Arrange
        var repositoryRoot = RepositoryRoot.Find();
        var resourceTexts = LoadRepositoryResourceTexts(repositoryRoot);

        // Act
        var findings = ScanForResourceTextLiterals(Path.Combine(repositoryRoot, "tests"), resourceTexts);

        // Assert
        Assert.IsEmpty(findings, DescribeFindings(findings));
    }

    /// <summary>
    /// Verifies that the repository scan reads the values of both resource files, decoded from their XML, so the
    /// scan above cannot pass on an empty or still-encoded set of texts
    /// </summary>
    [TestMethod]
    public void RepositoryResourceTextsAreReadFromBothFilesAndDecoded()
    {
        // Arrange
        var repositoryRoot = RepositoryRoot.Find();

        // Act
        var perFile = _resourceFilePaths.Select(path => LoadResourceTexts([Path.Combine(repositoryRoot, path)]))
                                        .ToList();

        // Assert
        Assert.IsTrue(perFile.TrueForAll(texts => texts.Count > 100), "Each resource file must contribute its values.");
        Assert.Contains(text => text.Contains('<', StringComparison.Ordinal), perFile.SelectMany(texts => texts), "An entity-encoded value must be decoded.");
        Assert.DoesNotContain(text => text.Contains("&lt;", StringComparison.Ordinal), perFile.SelectMany(texts => texts), "No value may stay entity-encoded.");
    }

    /// <summary>
    /// Verifies that a real resource value written into a test source is reported, with its file and line, by the
    /// scan over the real resource texts; the literal is built at run time, so this file quotes no value itself
    /// </summary>
    [TestMethod]
    public void ScanReportsRealResourceTextWithFileAndLine()
    {
        // Arrange
        var resourceTexts = LoadRepositoryResourceTexts(RepositoryRoot.Find());
        var quotedText = resourceTexts[0];
        IReadOnlyList<ResourceTextLiteral> findings;

        // Act
        using (var fixture = new FixtureDirectory())
        {
            var literal = SymbolDisplay.FormatLiteral(quotedText, quote: true);

            fixture.WriteFile(Path.Combine("Project", "SampleTests.cs"), $"internal class SampleTests\n{{\n    private const string Message = {literal};\n}}\n");

            findings = ScanForResourceTextLiterals(fixture.Path, resourceTexts);
        }

        // Assert
        Assert.HasCount(1, findings);
        Assert.AreEqual(new ResourceTextLiteral(Path.Combine("Project", "SampleTests.cs"), 3, quotedText), findings[0]);
    }

    /// <summary>
    /// Verifies that the values are read only from the root's <c>data</c> elements: the sample <c>data</c> elements
    /// inside the standard XML-comment header of a resource file are skipped, and XML entities are decoded
    /// </summary>
    [TestMethod]
    public void LoadResourceTextsSkipsHeaderSamplesAndDecodesEntities()
    {
        // Arrange
        IReadOnlyList<string> resourceTexts;

        // Act
        using (var fixture = new FixtureDirectory())
        {
            var path = fixture.WriteFile("Sample.resx",
                                         "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n"
                                         + "<root>\n"
                                         + "  <!--\n"
                                         + "    <data name=\"Name1\"><value>this is my long string</value></data>\n"
                                         + "    <data name=\"Color1\" type=\"System.Drawing.Color, System.Drawing\">Blue</data>\n"
                                         + "  -->\n"
                                         + "  <data name=\"RH9998Title\" xml:space=\"preserve\">\n"
                                         + "    <value>Remove &lt;sample&gt; tag</value>\n"
                                         + "  </data>\n"
                                         + "  <data name=\"RH9999MessageFormat\" xml:space=\"preserve\">\n"
                                         + "    <value>" + SyntheticFormat + "</value>\n"
                                         + "  </data>\n"
                                         + "</root>\n");

            resourceTexts = LoadResourceTexts([path]);
        }

        // Assert
        Assert.AreSequenceEqual(new[] { "Remove <sample> tag", SyntheticFormatFragment }, resourceTexts, SequenceOrder.InAnyOrder);
    }

    /// <summary>
    /// Verifies that a value without placeholders stands for itself
    /// </summary>
    [TestMethod]
    public void ResourceTextOfValueWithoutPlaceholdersIsTheWholeValue()
    {
        // Act
        var text = GetResourceText(SyntheticText);

        // Assert
        Assert.AreEqual(SyntheticText, text);
    }

    /// <summary>
    /// Verifies that a value with placeholders, also with an alignment or a format, is represented by its longest
    /// fixed fragment between the placeholders
    /// </summary>
    [TestMethod]
    public void ResourceTextOfValueWithPlaceholdersIsItsLongestFixedFragment()
    {
        // Act
        var plain = GetResourceText(SyntheticFormat);
        var formatted = GetResourceText("Member has {0,-5} lines, over the limit of {1:N0} for sure.");
        var leading = GetResourceText("{0} is the member that is too long");

        // Assert
        Assert.AreEqual(SyntheticFormatFragment, plain);
        Assert.AreEqual(SyntheticFormatFragment, formatted);
        Assert.AreEqual(" is the member that is too long", leading);
    }

    /// <summary>
    /// Verifies that a regular literal equal to a resource text is reported
    /// </summary>
    [TestMethod]
    public void ScanReportsRegularLiteralEqualToText()
    {
        // Act
        var findings = ScanSource("        var message = \"Synthetic resource text for the scan.\";\n");

        // Assert
        Assert.HasCount(1, findings);
        Assert.AreEqual(new ResourceTextLiteral("Sample.cs", 5, SyntheticText), findings[0]);
    }

    /// <summary>
    /// Verifies that a regular literal holding a resource text inside a longer string is reported
    /// </summary>
    [TestMethod]
    public void ScanReportsRegularLiteralContainingText()
    {
        // Act
        var findings = ScanSource("        var message = \"[RH9999] (SampleAnalyzer) Synthetic resource text for the scan. More text\";\n");

        // Assert
        Assert.HasCount(1, findings);
        Assert.AreEqual(5, findings[0].Line);
    }

    /// <summary>
    /// Verifies that a literal holding only part of a resource text is not reported — the other side of the
    /// containment boundary
    /// </summary>
    [TestMethod]
    public void ScanIgnoresLiteralHoldingOnlyPartOfText()
    {
        // Act
        var findings = ScanSource("        var message = \"Synthetic resource text for the scan\";\n"
                                  + "        var other = \"resource text for the scan.\";\n");

        // Assert
        Assert.IsEmpty(findings);
    }

    /// <summary>
    /// Verifies that a literal holding a formatted message — the fixed fragment of a value with placeholders, with
    /// the placeholders filled in — is reported
    /// </summary>
    [TestMethod]
    public void ScanReportsLiteralHoldingFormattedMessage()
    {
        // Act
        var findings = ScanSource("        var message = \"[RH9999] (SampleAnalyzer) Member has 121 lines, over the limit of 120 for sure.\";\n");

        // Assert
        Assert.HasCount(1, findings);
        Assert.AreEqual(new ResourceTextLiteral("Sample.cs", 5, SyntheticFormatFragment), findings[0]);
    }

    /// <summary>
    /// Verifies that a literal holding a value's format string with its placeholders verbatim is reported
    /// </summary>
    [TestMethod]
    public void ScanReportsLiteralHoldingFormatString()
    {
        // Act
        var findings = ScanSource("        var format = \"Member has {0} lines, over the limit of {1} for sure.\";\n");

        // Assert
        Assert.HasCount(1, findings);
        Assert.AreEqual(5, findings[0].Line);
    }

    /// <summary>
    /// Verifies that an interpolated string whose text part holds the fixed fragment of a value is reported
    /// </summary>
    [TestMethod]
    public void ScanReportsInterpolatedStringTextPart()
    {
        // Act
        var findings = ScanSource("        var count = 121;\n"
                                  + "        var limit = 120;\n"
                                  + "        var message = $\"Member has {count} lines, over the limit of {limit} for sure.\";\n");

        // Assert
        Assert.HasCount(1, findings);
        Assert.AreEqual(new ResourceTextLiteral("Sample.cs", 7, SyntheticFormatFragment), findings[0]);
    }

    /// <summary>
    /// Verifies that the text part of an interpolated raw string is reported
    /// </summary>
    [TestMethod]
    public void ScanReportsInterpolatedRawStringTextPart()
    {
        // Act
        var findings = ScanSource("        var count = 121;\n"
                                  + "        var message = $$\"\"\"Member has {{count}} lines, over the limit of {{count}} for sure.\"\"\";\n");

        // Assert
        Assert.HasCount(1, findings);
        Assert.AreEqual(6, findings[0].Line);
    }

    /// <summary>
    /// Verifies that the decoded value of a regular literal with escape sequences is compared, not its source text
    /// </summary>
    [TestMethod]
    public void ScanComparesDecodedValueOfEscapedRegularLiteral()
    {
        // Act
        var findings = ScanSource("        var message = \"Say \\\"hello\\\" now\";\n", ["Say \"hello\" now"]);

        // Assert
        Assert.HasCount(1, findings);
        Assert.AreEqual(5, findings[0].Line);
    }

    /// <summary>
    /// Verifies that the decoded value of a verbatim literal with doubled quotes is compared, not its source text
    /// </summary>
    [TestMethod]
    public void ScanComparesDecodedValueOfVerbatimLiteral()
    {
        // Act
        var findings = ScanSource("        var message = @\"Say \"\"hello\"\" now\";\n", ["Say \"hello\" now"]);

        // Assert
        Assert.HasCount(1, findings);
        Assert.AreEqual(5, findings[0].Line);
    }

    /// <summary>
    /// Verifies that a single-line raw string literal holding a resource text is reported
    /// </summary>
    [TestMethod]
    public void ScanReportsSingleLineRawStringLiteral()
    {
        // Act
        var findings = ScanSource("        var message = \"\"\"Synthetic resource text for the scan.\"\"\";\n");

        // Assert
        Assert.HasCount(1, findings);
        Assert.AreEqual(5, findings[0].Line);
    }

    /// <summary>
    /// Verifies that a multi-line raw string literal holding a resource text is reported at the line of the text,
    /// not at the line of its opening quotes
    /// </summary>
    [TestMethod]
    public void ScanReportsMultiLineRawStringLiteralAtTheTextLine()
    {
        // Act
        var findings = ScanSource("        var message = \"\"\"\n"
                                  + "            First line\n"
                                  + "            Synthetic resource text for the scan.\n"
                                  + "            \"\"\";\n");

        // Assert
        Assert.HasCount(1, findings);
        Assert.AreEqual(7, findings[0].Line);
    }

    /// <summary>
    /// Verifies that the line of a text inside a multi-line raw string literal is counted the same in a file with
    /// CRLF line endings
    /// </summary>
    [TestMethod]
    public void ScanReportsMultiLineRawStringLiteralAtTheTextLineWithCrLf()
    {
        // Act
        var findings = ScanSource(("        var message = \"\"\"\n"
                                   + "            First line\n"
                                   + "            Synthetic resource text for the scan.\n"
                                   + "            \"\"\";\n").Replace("\n", "\r\n", StringComparison.Ordinal),
                                  [SyntheticText],
                                  "\r\n");

        // Assert
        Assert.HasCount(1, findings);
        Assert.AreEqual(7, findings[0].Line);
    }

    /// <summary>
    /// Verifies that a UTF-8 string literal holding a resource text is reported
    /// </summary>
    [TestMethod]
    public void ScanReportsUtf8StringLiteral()
    {
        // Act
        var findings = ScanSource("        var message = \"Synthetic resource text for the scan.\"u8;\n");

        // Assert
        Assert.HasCount(1, findings);
        Assert.AreEqual(5, findings[0].Line);
    }

    /// <summary>
    /// Verifies that every literal holding a resource text is reported at its own line
    /// </summary>
    [TestMethod]
    public void ScanReportsEveryLiteralAtItsOwnLine()
    {
        // Act
        var findings = ScanSource("        var first = \"Synthetic resource text for the scan.\";\n"
                                  + "        var clean = \"Unrelated text\";\n"
                                  + "        var second = \"Synthetic resource text for the scan.\";\n");

        // Assert
        Assert.AreSequenceEqual(new[] { 5, 7 }, findings.Select(finding => finding.Line));
    }

    /// <summary>
    /// Verifies that a resource text in a comment, a documentation comment, or disabled code is not reported,
    /// because none of them is a literal token
    /// </summary>
    [TestMethod]
    public void ScanIgnoresCommentsDocumentationCommentsAndDisabledText()
    {
        // Arrange
        const string source = "internal class Sample\n"
                              + "{\n"
                              + "    /// <summary>\n"
                              + "    /// Synthetic resource text for the scan.\n"
                              + "    /// </summary>\n"
                              + "    private void Run()\n"
                              + "    {\n"
                              + "        // Synthetic resource text for the scan.\n"
                              + "        /* Synthetic resource text for the scan. */\n"
                              + "#if false\n"
                              + "        var message = \"Synthetic resource text for the scan.\";\n"
                              + "#endif\n"
                              + "    }\n"
                              + "}\n";
        IReadOnlyList<ResourceTextLiteral> findings;

        // Act
        using (var fixture = new FixtureDirectory())
        {
            fixture.WriteFile("Sample.cs", source);

            findings = ScanForResourceTextLiterals(fixture.Path, [SyntheticText]);
        }

        // Assert
        Assert.IsEmpty(findings);
    }

    /// <summary>
    /// Verifies that the arguments of a command line that name a diagnostic ID next to other strings, and literals
    /// that share only a word with a resource text, are not reported by the scan over the real resource texts
    /// </summary>
    [TestMethod]
    public void ScanIgnoresDiagnosticIdArgumentsAndSharedWordsWithRealTexts()
    {
        // Arrange
        var resourceTexts = LoadRepositoryResourceTexts(RepositoryRoot.Find());

        // Act
        var findings = ScanSource("        var arguments = new[] { \"--rule\", \"RH5001\", \"--check\", \"src\" };\n"
                                  + "        var words = new[] { \"Fix\", \"casing\", \"braces\", \"Blue\" };\n",
                                  resourceTexts);

        // Assert
        Assert.IsEmpty(findings);
    }

    /// <summary>
    /// Verifies that files under build-output directories are not scanned
    /// </summary>
    [TestMethod]
    public void ScanExcludesBuildOutputDirectories()
    {
        // Arrange
        const string source = "internal class Sample\n{\n    private const string Message = \"Synthetic resource text for the scan.\";\n}\n";
        IReadOnlyList<ResourceTextLiteral> findings;

        // Act
        using (var fixture = new FixtureDirectory())
        {
            fixture.WriteFile(Path.Combine("Project", "bin", "Debug", "Sample.cs"), source);
            fixture.WriteFile(Path.Combine("Project", "obj", "Release", "Sample.cs"), source);
            fixture.WriteFile(Path.Combine("Project", "Sample.cs"), source);

            findings = ScanForResourceTextLiterals(fixture.Path, [SyntheticText]);
        }

        // Assert
        Assert.HasCount(1, findings);
        Assert.AreEqual(Path.Combine("Project", "Sample.cs"), findings[0].FilePath);
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Reads the resource texts of both resource files of the repository
    /// </summary>
    /// <param name="repositoryRoot">Repository root</param>
    /// <returns>Every distinct resource text</returns>
    internal static IReadOnlyList<string> LoadRepositoryResourceTexts(string repositoryRoot)
    {
        return LoadResourceTexts(_resourceFilePaths.Select(path => Path.Combine(repositoryRoot, path)));
    }

    /// <summary>
    /// Reads the value of every <c>data</c> element directly under the root of each resource file, parsed as XML so
    /// the sample elements inside the file's XML-comment header are skipped and entities are decoded, and turns
    /// each value into the text the scan looks for (<see cref="GetResourceText"/>)
    /// </summary>
    /// <param name="resourceFilePaths">Paths of the resource files</param>
    /// <returns>Every distinct resource text</returns>
    internal static IReadOnlyList<string> LoadResourceTexts(IEnumerable<string> resourceFilePaths)
    {
        return resourceFilePaths.SelectMany(path => XDocument.Load(path).Root?.Elements("data") ?? [])
                                .Select(data => data.Element("value")?.Value)
                                .Where(value => string.IsNullOrEmpty(value) == false)
                                .Select(value => GetResourceText(value))
                                .Where(text => text.Length > 0)
                                .Distinct(StringComparer.Ordinal)
                                .ToList();
    }

    /// <summary>
    /// Gets the text that stands for a resource value in the scan: the value itself, or, when it holds
    /// placeholders, its longest fixed fragment between them, because a test fills the placeholders in
    /// </summary>
    /// <param name="value">Resource value</param>
    /// <returns>The text the scan looks for</returns>
    internal static string GetResourceText(string value)
    {
        return _placeholderPattern.Split(value)
                                  .OrderByDescending(fragment => fragment.Length)
                                  .First();
    }

    /// <summary>
    /// Scans every C# file under a root directory, outside <c>bin</c> and <c>obj</c>, for a literal token whose
    /// decoded value contains one of the resource texts: a regular, verbatim, raw, or UTF-8 string literal, or the
    /// text part of an interpolated string. Each literal is reported once, at the line where the text starts
    /// </summary>
    /// <param name="rootDirectory">Directory to scan</param>
    /// <param name="resourceTexts">Texts no literal may contain</param>
    /// <returns>Every literal found</returns>
    internal static IReadOnlyList<ResourceTextLiteral> ScanForResourceTextLiterals(string rootDirectory, IReadOnlyCollection<string> resourceTexts)
    {
        var findings = new List<ResourceTextLiteral>();
        var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview);

        foreach (var filePath in Directory.EnumerateFiles(rootDirectory, "*.cs", SearchOption.AllDirectories))
        {
            if (BuildOutputDirectories.Contains(rootDirectory, filePath))
            {
                continue;
            }

            var relativePath = Path.GetRelativePath(rootDirectory, filePath);
            var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(filePath), parseOptions);

            foreach (var token in tree.GetRoot().DescendantTokens())
            {
                if (IsLiteralToken(token) == false)
                {
                    continue;
                }

                var resourceText = resourceTexts.FirstOrDefault(text => token.ValueText.Contains(text, StringComparison.Ordinal));

                if (resourceText == null)
                {
                    continue;
                }

                var offset = token.Text.IndexOf(resourceText, StringComparison.Ordinal);
                var position = offset >= 0 ? token.SpanStart + offset : token.SpanStart;
                var line = tree.GetLineSpan(new TextSpan(position, 0)).StartLinePosition.Line + 1;

                findings.Add(new ResourceTextLiteral(relativePath, line, resourceText));
            }
        }

        return findings;
    }

    /// <summary>
    /// Writes a method body into a fixture file and scans it
    /// </summary>
    /// <param name="body">Statements of the method body, starting on line 5 of the file</param>
    /// <param name="resourceTexts">Texts no literal may contain; the synthetic texts when omitted</param>
    /// <param name="endOfLine">Line ending of the lines around the body</param>
    /// <returns>Every literal found</returns>
    private static IReadOnlyList<ResourceTextLiteral> ScanSource(string body, IReadOnlyCollection<string> resourceTexts = null, string endOfLine = "\n")
    {
        var header = string.Join(endOfLine, "internal class Sample", "{", "    private void Run()", "    {", string.Empty);
        var footer = string.Join(endOfLine, "    }", "}", string.Empty);
        var source = $"{header}{body}{footer}";

        using (var fixture = new FixtureDirectory())
        {
            fixture.WriteFile("Sample.cs", source);

            return ScanForResourceTextLiterals(fixture.Path, resourceTexts ?? [SyntheticText, SyntheticFormatFragment]);
        }
    }

    /// <summary>
    /// Determines whether a token is a string literal or the text part of an interpolated string
    /// </summary>
    /// <param name="token">Token to check</param>
    /// <returns><see langword="true"/> if the token carries literal text; otherwise, <see langword="false"/></returns>
    private static bool IsLiteralToken(SyntaxToken token)
    {
        return token.Kind() is SyntaxKind.StringLiteralToken
                            or SyntaxKind.Utf8StringLiteralToken
                            or SyntaxKind.SingleLineRawStringLiteralToken
                            or SyntaxKind.Utf8SingleLineRawStringLiteralToken
                            or SyntaxKind.MultiLineRawStringLiteralToken
                            or SyntaxKind.Utf8MultiLineRawStringLiteralToken
                            or SyntaxKind.InterpolatedStringTextToken;
    }

    /// <summary>
    /// Formats every finding into a single assertion message
    /// </summary>
    /// <param name="findings">Findings to describe</param>
    /// <returns>One line per finding, or an empty string when there are none</returns>
    private static string DescribeFindings(IReadOnlyList<ResourceTextLiteral> findings)
    {
        if (findings.Count == 0)
        {
            return string.Empty;
        }

        var lines = findings.Select(finding => $"{finding.FilePath}({finding.Line}): the literal quotes the resource text '{finding.ResourceText}'. "
                                               + "Read it through the test base's MessageFormat property or the resource class instead.");

        return $"Found {findings.Count} literal(s) that quote a resource text:\n{string.Join("\n", lines)}";
    }

    #endregion // Methods
}