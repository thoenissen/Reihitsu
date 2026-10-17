using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.ArchitectureTests.Sdd;

/// <summary>
/// Reads the code owners and requirement IDs of the specs under <c>sdd/specs/</c> and the type declarations of
/// the production projects under <c>src/</c>
/// </summary>
internal static class SpecCatalog
{
    #region Constants

    /// <summary>
    /// File name of the append-only ledger of every requirement ID ever issued
    /// </summary>
    public const string LedgerFileName = "requirement-ids.txt";

    /// <summary>
    /// File name of the spec index, which is not a spec itself
    /// </summary>
    public const string IndexFileName = "README.md";

    /// <summary>
    /// Cell text of a header row that names nothing, such as <c>Uses</c> without uses
    /// </summary>
    public const string NoneCell = "—";

    /// <summary>
    /// Heading of the section that lists what a spec deliberately does not cover
    /// </summary>
    private const string NonGoalsHeading = "## Non-goals";

    /// <summary>
    /// Matches the <c>Code owners</c> row of a spec's header table and captures its cell
    /// </summary>
    private static readonly Regex _codeOwnersRowPattern = new(@"^\|\s*Code owners\s*\|(?<owners>.*)\|\s*$");

    /// <summary>
    /// Matches one backtick-quoted entry of a <c>Code owners</c> cell
    /// </summary>
    private static readonly Regex _quotedEntryPattern = new("`(?<entry>[^`]+)`");

    /// <summary>
    /// Matches a type name with an optional type-parameter list, such as <c>CasingCodeFixProviderBase&lt;T&gt;</c>
    /// </summary>
    private static readonly Regex _typeNamePattern = new(@"^(?<name>[A-Za-z_][A-Za-z0-9_]*)(?:<(?<parameters>[^<>]+)>)?$");

    /// <summary>
    /// Matches a requirement heading and captures its ID
    /// </summary>
    private static readonly Regex _requirementHeadingPattern = new(@"^### (?<id>\S+) — ");

    /// <summary>
    /// Matches the status line that marks a requirement as retired
    /// </summary>
    private static readonly Regex _retiredStatusPattern = new(@"^Status:\s*Retired\b");

    /// <summary>
    /// Matches one row of a spec's header table and captures its label and its cell
    /// </summary>
    private static readonly Regex _headerRowPattern = new(@"^\|\s*(?<label>[^|]*?)\s*\|\s*(?<value>.*?)\s*\|\s*$");

    /// <summary>
    /// Matches a requirement ID anywhere in a line, such as <c>RH3202-001</c> or <c>SHARED-ORDER-002</c>
    /// </summary>
    private static readonly Regex _requirementIdPattern = new(@"\b(?<id>(?:RH[0-9]{4}[A-Z]?|[A-Z]+(?:-[A-Z]+)*)-[0-9]{3})\b");

    /// <summary>
    /// Matches a row of the spec index and captures the linked spec path, the ID prefix, and the code owners cell
    /// </summary>
    private static readonly Regex _indexRowPattern = new(@"^\|\s*\[(?<path>[^\]]+)\]\([^)]*\)\s*\|\s*(?<prefix>[^|]*?)\s*\|\s*(?<owners>.*?)\s*\|\s*$");

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Enumerates every spec file below the specs directory, excluding the spec index
    /// </summary>
    /// <param name="specsDirectory">The <c>sdd/specs</c> directory</param>
    /// <returns>Full paths of the spec files, in ordinal order</returns>
    public static IReadOnlyList<string> EnumerateSpecFiles(string specsDirectory)
    {
        return Directory.EnumerateFiles(specsDirectory, "*.md", SearchOption.AllDirectories)
                        .Where(path => string.Equals(Path.GetRelativePath(specsDirectory, path), IndexFileName, StringComparison.Ordinal) == false)
                        .OrderBy(path => path, StringComparer.Ordinal)
                        .ToList();
    }

    /// <summary>
    /// Reads the code owners named in the header of every spec
    /// </summary>
    /// <param name="specsDirectory">The <c>sdd/specs</c> directory</param>
    /// <param name="problems">Receives one message per spec whose header has no readable <c>Code owners</c> row</param>
    /// <returns>The code owners of all specs</returns>
    public static IReadOnlyList<SpecCodeOwner> ReadCodeOwners(string specsDirectory, List<string> problems)
    {
        var owners = new List<SpecCodeOwner>();

        foreach (var specFile in EnumerateSpecFiles(specsDirectory))
        {
            var specPath = Path.GetRelativePath(specsDirectory, specFile).Replace('\\', '/');
            var rows = File.ReadLines(specFile)
                           .Select(line => _codeOwnersRowPattern.Match(line))
                           .Where(match => match.Success)
                           .ToList();

            if (rows.Count != 1)
            {
                problems.Add($"{specPath}: expected exactly one 'Code owners' header row, found {rows.Count}");

                continue;
            }

            var cell = rows[0].Groups["owners"].Value.Trim();
            var entries = _quotedEntryPattern.Matches(cell);

            if (entries.Count == 0
                && cell != NoneCell)
            {
                problems.Add($"{specPath}: the 'Code owners' header row names no type");
            }

            foreach (Match entry in entries)
            {
                var text = entry.Groups["entry"].Value;
                var typeName = _typeNamePattern.Match(text);

                if (typeName.Success == false)
                {
                    problems.Add($"{specPath}: code owner `{text}` is not a type name");

                    continue;
                }

                var arity = typeName.Groups["parameters"].Success
                                ? typeName.Groups["parameters"].Value.Split(',').Length
                                : 0;

                owners.Add(new SpecCodeOwner(specPath, text, typeName.Groups["name"].Value, arity));
            }
        }

        return owners;
    }

    /// <summary>
    /// Reads every requirement heading of every spec, skipping fenced code blocks. A requirement is retired when a
    /// <c>Status: Retired</c> line follows its heading before the next heading
    /// </summary>
    /// <param name="specsDirectory">The <c>sdd/specs</c> directory</param>
    /// <returns>The requirement headings of all specs</returns>
    public static IReadOnlyList<SpecRequirement> ReadRequirements(string specsDirectory)
    {
        var requirements = new List<SpecRequirement>();

        foreach (var specFile in EnumerateSpecFiles(specsDirectory))
        {
            var specPath = Path.GetRelativePath(specsDirectory, specFile).Replace('\\', '/');
            var isInFence = false;
            var lineNumber = 0;
            SpecRequirement current = null;

            foreach (var line in File.ReadLines(specFile))
            {
                lineNumber++;

                if (line.TrimStart().StartsWith("```", StringComparison.Ordinal))
                {
                    isInFence = isInFence == false;

                    continue;
                }

                if (isInFence)
                {
                    continue;
                }

                if (line.StartsWith("#", StringComparison.Ordinal))
                {
                    current = null;
                }

                var heading = _requirementHeadingPattern.Match(line);

                if (heading.Success)
                {
                    current = new SpecRequirement(specPath, lineNumber, heading.Groups["id"].Value, false);
                    requirements.Add(current);
                }
                else if (current != null
                         && _retiredStatusPattern.IsMatch(line))
                {
                    current = current with
                                      {
                                          IsRetired = true
                                      };
                    requirements[^1] = current;
                }
            }
        }

        return requirements;
    }

    /// <summary>
    /// Reads the header table of every spec: the rows between the title and the first section heading
    /// </summary>
    /// <param name="specsDirectory">The <c>sdd/specs</c> directory</param>
    /// <returns>The headers of all specs</returns>
    public static IReadOnlyList<SpecHeader> ReadHeaders(string specsDirectory)
    {
        var headers = new List<SpecHeader>();

        foreach (var specFile in EnumerateSpecFiles(specsDirectory))
        {
            var rows = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (var line in File.ReadLines(specFile))
            {
                if (line.StartsWith("## ", StringComparison.Ordinal))
                {
                    break;
                }

                var row = _headerRowPattern.Match(line);

                if (row.Success
                    && row.Groups["label"].Value.Length > 0
                    && row.Groups["label"].Value.Trim('-').Length > 0)
                {
                    rows.TryAdd(row.Groups["label"].Value, row.Groups["value"].Value);
                }
            }

            headers.Add(new SpecHeader(GetSpecPath(specsDirectory, specFile), rows));
        }

        return headers;
    }

    /// <summary>
    /// Reads every requirement ID a spec mentions outside its header table and its <c>Non-goals</c> section,
    /// including the IDs of its own requirements
    /// </summary>
    /// <param name="specsDirectory">The <c>sdd/specs</c> directory</param>
    /// <returns>The mentioned IDs per spec path</returns>
    public static IReadOnlyDictionary<string, HashSet<string>> ReadMentionedRequirementIds(string specsDirectory)
    {
        var mentions = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);

        foreach (var specFile in EnumerateSpecFiles(specsDirectory))
        {
            var ids = new HashSet<string>(StringComparer.Ordinal);
            var isInHeaderTable = false;
            var hasPassedHeaderTable = false;
            var isInNonGoals = false;

            foreach (var line in File.ReadLines(specFile))
            {
                if (hasPassedHeaderTable == false)
                {
                    if (line.StartsWith('|'))
                    {
                        isInHeaderTable = true;

                        continue;
                    }

                    if (isInHeaderTable)
                    {
                        hasPassedHeaderTable = true;
                    }
                }

                if (line.StartsWith("## ", StringComparison.Ordinal))
                {
                    hasPassedHeaderTable = true;
                    isInNonGoals = line.TrimEnd() == NonGoalsHeading;
                }

                if (isInNonGoals)
                {
                    continue;
                }

                foreach (Match match in _requirementIdPattern.Matches(line))
                {
                    ids.Add(match.Groups["id"].Value);
                }
            }

            mentions.Add(GetSpecPath(specsDirectory, specFile), ids);
        }

        return mentions;
    }

    /// <summary>
    /// Reads the rows of the spec index <c>sdd/specs/README.md</c>
    /// </summary>
    /// <param name="specsDirectory">The <c>sdd/specs</c> directory</param>
    /// <returns>Spec path, ID prefix, and code owners cell of every row; empty when the index does not exist</returns>
    public static IReadOnlyList<(string SpecPath, string Prefix, string CodeOwners)> ReadIndex(string specsDirectory)
    {
        var indexPath = Path.Combine(specsDirectory, IndexFileName);

        if (File.Exists(indexPath) == false)
        {
            return [];
        }

        return File.ReadLines(indexPath)
                   .Select(line => _indexRowPattern.Match(line))
                   .Where(match => match.Success)
                   .Select(match => (match.Groups["path"].Value, match.Groups["prefix"].Value, match.Groups["owners"].Value))
                   .ToList();
    }

    /// <summary>
    /// Splits a comma-separated header cell into its entries; <see cref="NoneCell"/> yields none
    /// </summary>
    /// <param name="cell">Cell text</param>
    /// <returns>The trimmed entries</returns>
    public static IReadOnlyList<string> SplitCell(string cell)
    {
        if (cell == null
            || cell.Trim() == NoneCell)
        {
            return [];
        }

        return cell.Split(',')
                   .Select(entry => entry.Trim())
                   .Where(entry => entry.Length > 0)
                   .ToList();
    }

    /// <summary>
    /// Reads the requirement-ID ledger: one ID per line, blank lines ignored
    /// </summary>
    /// <param name="specsDirectory">The <c>sdd/specs</c> directory</param>
    /// <returns>The ledger entries with their one-based line numbers; empty when the ledger does not exist</returns>
    public static IReadOnlyList<(int Line, string Id)> ReadLedger(string specsDirectory)
    {
        var ledgerPath = Path.Combine(specsDirectory, LedgerFileName);

        if (File.Exists(ledgerPath) == false)
        {
            return [];
        }

        return File.ReadLines(ledgerPath)
                   .Select((line, index) => (Line: index + 1, Id: line.Trim()))
                   .Where(entry => entry.Id.Length > 0)
                   .ToList();
    }

    /// <summary>
    /// Collects the types declared by the production projects below the source directory. Test projects are
    /// excluded, because a spec describes shipped behavior and its code owners are production types. A Razor
    /// component is declared by its <c>.razor</c> file and counts under the file name
    /// </summary>
    /// <param name="sourceDirectory">The <c>src</c> directory</param>
    /// <returns>Declared types, each keyed by <see cref="GetTypeKey"/></returns>
    public static HashSet<string> CollectDeclaredTypes(string sourceDirectory)
    {
        var declaredTypes = new HashSet<string>(StringComparer.Ordinal);

        foreach (var file in EnumerateProductionFiles(sourceDirectory, "*.cs"))
        {
            var root = CSharpSyntaxTree.ParseText(File.ReadAllText(file)).GetRoot();

            foreach (var node in root.DescendantNodes())
            {
                switch (node)
                {
                    case TypeDeclarationSyntax typeDeclaration:
                        declaredTypes.Add(GetTypeKey(typeDeclaration.Identifier.ValueText, typeDeclaration.TypeParameterList?.Parameters.Count ?? 0));
                        break;

                    case EnumDeclarationSyntax enumDeclaration:
                        declaredTypes.Add(GetTypeKey(enumDeclaration.Identifier.ValueText, 0));
                        break;

                    case DelegateDeclarationSyntax delegateDeclaration:
                        declaredTypes.Add(GetTypeKey(delegateDeclaration.Identifier.ValueText, delegateDeclaration.TypeParameterList?.Parameters.Count ?? 0));
                        break;
                }
            }
        }

        foreach (var file in EnumerateProductionFiles(sourceDirectory, "*.razor"))
        {
            declaredTypes.Add(GetTypeKey(Path.GetFileNameWithoutExtension(file), 0));
        }

        return declaredTypes;
    }

    /// <summary>
    /// Builds the lookup key of a type from its simple name and number of type parameters
    /// </summary>
    /// <param name="name">Simple type name</param>
    /// <param name="arity">Number of type parameters</param>
    /// <returns>The lookup key</returns>
    public static string GetTypeKey(string name, int arity)
    {
        return arity == 0 ? name : $"{name}`{arity}";
    }

    /// <summary>
    /// Enumerates the files of the production projects, skipping test projects and build output
    /// </summary>
    /// <param name="sourceDirectory">The <c>src</c> directory</param>
    /// <param name="searchPattern">File search pattern</param>
    /// <returns>Full paths of the matching files</returns>
    private static IEnumerable<string> EnumerateProductionFiles(string sourceDirectory, string searchPattern)
    {
        foreach (var projectDirectory in Directory.EnumerateDirectories(sourceDirectory))
        {
            var projectName = Path.GetFileName(projectDirectory);

            if (projectName.EndsWith(".Test", StringComparison.Ordinal)
                || projectName.EndsWith("Tests", StringComparison.Ordinal))
            {
                continue;
            }

            foreach (var file in Directory.EnumerateFiles(projectDirectory, searchPattern, SearchOption.AllDirectories))
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

    /// <summary>
    /// Builds the spec path relative to <c>sdd/specs</c>, with forward slashes
    /// </summary>
    /// <param name="specsDirectory">The <c>sdd/specs</c> directory</param>
    /// <param name="specFile">Full path of the spec file</param>
    /// <returns>The relative spec path</returns>
    private static string GetSpecPath(string specsDirectory, string specFile)
    {
        return Path.GetRelativePath(specsDirectory, specFile).Replace('\\', '/');
    }

    #endregion // Methods
}