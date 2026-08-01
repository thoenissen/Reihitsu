#:package Microsoft.CodeAnalysis.CSharp@5.6.0

// Syntax-aware proof that a change carries no compiled behavior.
//
//   dotnet run scripts/proof/verify-text-only.cs -- [--base <rev>] [--head <rev>] [--strict-docs]
//
// The workflow skills use this instead of a line-based grep over the diff,
// because a grep does not recognize every block-comment form and cannot
// distinguish a comment from a directive or from comment-looking text inside a
// string literal.
//
// For every changed C# file the tool proves that
//
//   * the non-trivia token stream is unchanged (this also covers string and
//     character literal contents, which are token text),
//   * no directive or disabled-text structure changed,
//   * every changed syntax element belongs to an allowed comment, documentation,
//     or layout trivia category,
//   * both versions parse without errors, which is also what rules out skipped
//     tokens: they only ever appear alongside a parse error.
//
// Generated XML-documentation impact is reported separately instead of being
// folded into the verdict, because a documentation edit is exactly what a
// comment-only change is allowed to be. Pass `--strict-docs` when the caller
// must additionally exclude public API documentation — the preflight
// `PASS — non-blocking cleanup` result requires that, and plain exit code 0
// does not establish it.
//
// Both sides are decoded identically and a leading byte order mark is stripped
// from each. When the head is the working tree, both sides are additionally
// compared with normalized line endings: git rewrites line endings on checkout
// under `core.autocrlf`, so a raw worktree comparison would report every
// multi-line literal and disabled-text region as changed on Windows.
//
// Exit codes: 0 proven text-only, 1 not proven, 2 tool or setup error.

using System.Diagnostics;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

string? baseRevision = null;
var headRevision = "HEAD";
var repositoryPath = ".";
var strictDocumentation = false;

for (var index = 0; index < args.Length; index++)
{
    switch (args[index])
    {
        case "--base" when index + 1 < args.Length:
            baseRevision = args[++index];
            break;

        case "--head" when index + 1 < args.Length:
            headRevision = args[++index];
            break;

        case "--strict-docs":
            strictDocumentation = true;
            break;

        case "--repository" when index + 1 < args.Length:
            repositoryPath = args[++index];
            break;

        case "--help":
        case "-h":
            Console.WriteLine("usage: dotnet run scripts/proof/verify-text-only.cs -- [--base <rev>] [--head <rev>] [--strict-docs] [--repository <path>]");
            Console.WriteLine("       --head worktree compares the base revision against the working tree");
            Console.WriteLine("       --strict-docs fails the proof when public API documentation changed");
            Console.WriteLine("       --repository names the repository to inspect, defaulting to the working directory");

            return 0;

        default:
            Console.Error.WriteLine($"verify-text-only: unknown argument '{args[index]}'.");

            return 2;
    }
}

string repositoryRoot;

try
{
    repositoryRoot = Proof.Git(repositoryPath, "rev-parse", "--show-toplevel").Trim();
}
catch (Exception exception)
{
    Console.Error.WriteLine($"verify-text-only: '{repositoryPath}' is not inside a git repository ({exception.Message}).");

    return 2;
}

var comparesWorkTree = string.Equals(headRevision, "worktree", StringComparison.OrdinalIgnoreCase);

try
{
    baseRevision ??= Proof.ResolveDefaultBase(repositoryRoot);
}
catch (Exception exception)
{
    Console.Error.WriteLine($"verify-text-only: could not resolve a base revision ({exception.Message}). Pass --base explicitly.");

    return 2;
}

List<ChangedFile> changedFiles;

try
{
    changedFiles = Proof.ReadChangedFiles(repositoryRoot, baseRevision, comparesWorkTree ? null : headRevision);
}
catch (Exception exception)
{
    Console.Error.WriteLine($"verify-text-only: could not read the diff ({exception.Message}).");

    return 2;
}

var baseLabel = Proof.ShortRevision(repositoryRoot, baseRevision);
var headLabel = comparesWorkTree ? "worktree" : Proof.ShortRevision(repositoryRoot, headRevision);

Console.WriteLine($"verify-text-only: base={baseLabel} head={headLabel}");

if (changedFiles.Count == 0)
{
    Console.WriteLine($"TEXT-ONLY PROOF: PASS — base={baseLabel} head={headLabel} — no changed files");

    return 0;
}

var verdicts = new List<FileVerdict>();

foreach (var changedFile in changedFiles)
{
    try
    {
        verdicts.Add(Proof.Verify(repositoryRoot, baseRevision, comparesWorkTree ? null : headRevision, changedFile, strictDocumentation));
    }
    catch (Exception exception)
    {
        Console.Error.WriteLine($"verify-text-only: could not inspect '{changedFile.Path}' ({exception.Message}).");

        return 2;
    }
}

foreach (var verdict in verdicts.OrderBy(entry => entry.Path, StringComparer.Ordinal))
{
    Console.WriteLine($"  {(verdict.CarriesBehavior ? "BEHAVIORAL " : "text-only  ")}{verdict.Path}  —  {verdict.Detail}");
}

var behavioral = verdicts.Count(verdict => verdict.CarriesBehavior);
var nonCompiled = verdicts.Count(verdict => verdict is { CarriesBehavior: false, IsCSharp: false });
var commentOnly = verdicts.Count(verdict => verdict is { CarriesBehavior: false, IsCSharp: true });
var documentationImpact = verdicts.Where(verdict => verdict.DocumentationImpact != DocumentationImpact.None).ToList();

var documentationSummary = documentationImpact.Count == 0
                               ? "none"
                               : documentationImpact.Any(verdict => verdict.DocumentationImpact == DocumentationImpact.PublicApi)
                                   ? $"{documentationImpact.Count} file(s), including public API documentation"
                                   : $"{documentationImpact.Count} file(s), no public API documentation";

if (Proof.SkippedUntracked.Count > 0)
{
    Console.WriteLine($"  note: {Proof.SkippedUntracked.Count} untracked file(s) ignored — they are not part of the tree that will merge: {string.Join(", ", Proof.SkippedUntracked.Take(5))}");
}

if (behavioral > 0)
{
    Console.WriteLine($"TEXT-ONLY PROOF: FAIL — base={baseLabel} head={headLabel} — {behavioral} of {verdicts.Count} changed file(s) carry compiled behavior; xml-doc impact: {documentationSummary}");

    return 1;
}

Console.WriteLine($"TEXT-ONLY PROOF: PASS — base={baseLabel} head={headLabel} — {verdicts.Count} changed file(s) ({nonCompiled} non-compiled, {commentOnly} C# comment/layout-only); xml-doc impact: {documentationSummary}");

return 0;

/// <summary>
/// One entry of the compared diff
/// </summary>
/// <param name="Status">Git status letter (`A`, `M`, `D`, `R`, …)</param>
/// <param name="Path">Repository-relative path at the head revision</param>
/// <param name="OldPath">Repository-relative path at the base revision, for renames</param>
internal sealed record ChangedFile(string Status, string Path, string? OldPath);

/// <summary>
/// Impact of a change on generated XML documentation
/// </summary>
internal enum DocumentationImpact
{
    /// <summary>
    /// No documentation comment changed
    /// </summary>
    None,

    /// <summary>
    /// A documentation comment changed on a member that is not publicly visible
    /// </summary>
    Internal,

    /// <summary>
    /// A documentation comment changed on a public or protected member
    /// </summary>
    PublicApi
}

/// <summary>
/// Result for a single changed file
/// </summary>
/// <param name="Path">Repository-relative path</param>
/// <param name="CarriesBehavior">Whether the file could not be proven free of compiled behavior</param>
/// <param name="IsCSharp">Whether the file was inspected as C# source</param>
/// <param name="Detail">Human-readable evidence for the verdict</param>
/// <param name="DocumentationImpact">Reported separately from the verdict</param>
internal sealed record FileVerdict(string Path, bool CarriesBehavior, bool IsCSharp, string Detail, DocumentationImpact DocumentationImpact);

/// <summary>
/// Proof implementation
/// </summary>
internal static class Proof
{
    #region Fields

    /// <summary>
    /// Trivia kinds that cannot change compiled behavior
    /// </summary>
    private static readonly HashSet<SyntaxKind> _allowedTriviaKinds =
    [
        SyntaxKind.WhitespaceTrivia,
        SyntaxKind.EndOfLineTrivia,
        SyntaxKind.SingleLineCommentTrivia,
        SyntaxKind.MultiLineCommentTrivia,
        SyntaxKind.SingleLineDocumentationCommentTrivia,
        SyntaxKind.MultiLineDocumentationCommentTrivia
    ];

    /// <summary>
    /// Trivia kinds that carry documentation
    /// </summary>
    private static readonly HashSet<SyntaxKind> _documentationTriviaKinds =
    [
        SyntaxKind.SingleLineDocumentationCommentTrivia,
        SyntaxKind.MultiLineDocumentationCommentTrivia
    ];

    /// <summary>
    /// Paths that hold no compiled source and no build configuration
    /// </summary>
    private static readonly string[] _nonCompiledPrefixes = [".claude/", ".codex/", ".github/ISSUE_TEMPLATE/", "documentation/", "plans/"];

    /// <summary>
    /// Paths whose contents are executable behavior even though nothing there compiles
    /// </summary>
    private static readonly string[] _behavioralPrefixes = ["scripts/", ".github/workflows/"];

    /// <summary>
    /// Untracked files that were left out of the comparison
    /// </summary>
    public static readonly List<string> SkippedUntracked = [];

    #endregion // Fields

    #region Methods

    /// <summary>
    /// Runs a git command and returns its standard output
    /// </summary>
    /// <param name="workingDirectory">Directory to run the command in</param>
    /// <param name="arguments">Command arguments</param>
    /// <returns>Standard output of the command</returns>
    public static string Git(string workingDirectory, params string[] arguments)
    {
        var startInfo = new ProcessStartInfo("git")
                        {
                            WorkingDirectory = workingDirectory,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            StandardOutputEncoding = Encoding.UTF8,
                            StandardErrorEncoding = Encoding.UTF8
                        };

        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("git could not be started");

        // Read both pipes concurrently: git can fill the error pipe while this side blocks on output, and both would then wait forever
        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();

        var output = outputTask.GetAwaiter().GetResult();
        var error = errorTask.GetAwaiter().GetResult();

        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"git {string.Join(' ', arguments)} failed: {error.Trim()}");
        }

        return output;
    }

    /// <summary>
    /// Resolves the default base revision as the merge base with the remote default branch
    /// </summary>
    /// <param name="repositoryRoot">Repository root</param>
    /// <returns>Resolved base revision</returns>
    public static string ResolveDefaultBase(string repositoryRoot)
    {
        try
        {
            return Git(repositoryRoot, "merge-base", "origin/main", "HEAD").Trim();
        }
        catch (InvalidOperationException)
        {
            return Git(repositoryRoot, "rev-parse", "origin/main").Trim();
        }
    }

    /// <summary>
    /// Shortens a revision for reporting
    /// </summary>
    /// <param name="repositoryRoot">Repository root</param>
    /// <param name="revision">Revision to shorten</param>
    /// <returns>Short revision, or the original value when it cannot be resolved</returns>
    public static string ShortRevision(string repositoryRoot, string revision)
    {
        try
        {
            return Git(repositoryRoot, "rev-parse", "--short", revision).Trim();
        }
        catch (InvalidOperationException)
        {
            return revision;
        }
    }

    /// <summary>
    /// Reads the changed files between the base revision and the head revision or working tree
    /// </summary>
    /// <param name="repositoryRoot">Repository root</param>
    /// <param name="baseRevision">Base revision</param>
    /// <param name="headRevision">Head revision, or <c>null</c> to compare against the working tree</param>
    /// <returns>Changed files</returns>
    public static List<ChangedFile> ReadChangedFiles(string repositoryRoot, string baseRevision, string? headRevision)
    {
        string[] arguments = headRevision is null
                                 ? ["diff", "--name-status", "--find-renames", baseRevision]
                                 : ["diff", "--name-status", "--find-renames", baseRevision, headRevision];

        var changedFiles = new List<ChangedFile>();

        foreach (var line in Git(repositoryRoot, arguments).Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = line.TrimEnd('\r').Split('\t');
            var status = parts[0];
            var isRenameOrCopy = status.StartsWith('R') || status.StartsWith('C');

            if (parts.Length < (isRenameOrCopy ? 3 : 2))
            {
                throw new InvalidOperationException($"unexpected `git diff --name-status` line: {line}");
            }

            changedFiles.Add(isRenameOrCopy
                                 ? new ChangedFile(status[..1], parts[2], parts[1])
                                 : new ChangedFile(status[..1], parts[1], null));
        }

        if (headRevision is null)
        {
            // Untracked files are not part of the tree that will merge, so a stray scratch file must not decide the verdict. Report them instead.
            SkippedUntracked.AddRange(Git(repositoryRoot, "ls-files", "--others", "--exclude-standard")
                                          .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                                          .Select(line => line.TrimEnd('\r')));
        }

        return changedFiles;
    }

    /// <summary>
    /// Verifies a single changed file
    /// </summary>
    /// <param name="repositoryRoot">Repository root</param>
    /// <param name="baseRevision">Base revision</param>
    /// <param name="headRevision">Head revision, or <c>null</c> for the working tree</param>
    /// <param name="changedFile">Changed file to verify</param>
    /// <param name="strictDocumentation">Whether changed public API documentation fails the proof</param>
    /// <returns>Verdict for the file</returns>
    public static FileVerdict Verify(string repositoryRoot, string baseRevision, string? headRevision, ChangedFile changedFile, bool strictDocumentation)
    {
        var isCSharp = changedFile.Path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase);

        if (changedFile.OldPath is not null)
        {
            return new FileVerdict(changedFile.Path, true, isCSharp, $"renamed from {changedFile.OldPath}; a path change is not a comment change", DocumentationImpact.None);
        }

        if (isCSharp == false)
        {
            return IsNonCompiledPath(changedFile.Path)
                       ? new FileVerdict(changedFile.Path, false, false, "non-compiled text file", DocumentationImpact.None)
                       : new FileVerdict(changedFile.Path, true, false, "not a C# file and not a known non-compiled text path", DocumentationImpact.None);
        }

        switch (changedFile.Status)
        {
            case "A":
                return new FileVerdict(changedFile.Path, true, true, "added C# file", DocumentationImpact.None);

            case "D":
                return new FileVerdict(changedFile.Path, true, true, "deleted C# file", DocumentationImpact.None);
        }

        // The working tree carries whatever line endings the checkout produced, while a blob carries what was committed, so the worktree comparison normalizes both sides
        var normalizeLineEndings = headRevision is null;

        var before = Decode(ReadBlob(repositoryRoot, baseRevision, changedFile.Path), normalizeLineEndings);
        var after = Decode(headRevision is null
                               ? File.ReadAllText(Path.Combine(repositoryRoot, changedFile.Path))
                               : ReadBlob(repositoryRoot, headRevision, changedFile.Path),
                           normalizeLineEndings);

        return VerifyCSharp(changedFile.Path, before, after, strictDocumentation);
    }

    /// <summary>
    /// Brings both sides of a comparison into the same textual shape
    /// </summary>
    /// <param name="text">Content to decode</param>
    /// <param name="normalizeLineEndings">Whether CRLF is folded to LF</param>
    /// <returns>Content without a byte order mark, optionally with normalized line endings</returns>
    private static string Decode(string text, bool normalizeLineEndings)
    {
        if (text.StartsWith('﻿'))
        {
            text = text[1..];
        }

        return normalizeLineEndings ? text.Replace("\r\n", "\n") : text;
    }

    /// <summary>
    /// Compares the two versions of a C# file
    /// </summary>
    /// <param name="path">Repository-relative path</param>
    /// <param name="before">Content at the base revision</param>
    /// <param name="after">Content at the head revision</param>
    /// <param name="strictDocumentation">Whether changed public API documentation fails the proof</param>
    /// <returns>Verdict for the file</returns>
    private static FileVerdict VerifyCSharp(string path, string before, string after, bool strictDocumentation)
    {
        var beforeTree = CSharpSyntaxTree.ParseText(before);
        var afterTree = CSharpSyntaxTree.ParseText(after);

        if (HasParseErrors(beforeTree))
        {
            return new FileVerdict(path, true, true, "the base version does not parse", DocumentationImpact.None);
        }

        if (HasParseErrors(afterTree))
        {
            return new FileVerdict(path, true, true, "the changed version does not parse", DocumentationImpact.None);
        }

        var beforeRoot = beforeTree.GetRoot();
        var afterRoot = afterTree.GetRoot();

        var beforeTokens = beforeRoot.DescendantTokens().ToList();
        var afterTokens = afterRoot.DescendantTokens().ToList();

        var tokenDifference = FirstDifference(beforeTokens.Select(DescribeToken).ToList(), afterTokens.Select(DescribeToken).ToList());

        if (tokenDifference is not null)
        {
            return new FileVerdict(path, true, true, $"token stream differs ({tokenDifference})", DocumentationImpact.None);
        }

        var structuralDifference = FirstDifference(DescribeStructuralTrivia(beforeRoot), DescribeStructuralTrivia(afterRoot));

        if (structuralDifference is not null)
        {
            return new FileVerdict(path, true, true, $"directive or disabled-text structure differs ({structuralDifference})", DocumentationImpact.None);
        }

        var literalCount = beforeTokens.Count(IsLiteralToken);
        var documentationImpact = DetermineDocumentationImpact(beforeRoot, afterRoot);
        var changedCategories = DescribeChangedTriviaCategories(beforeRoot, afterRoot);

        if (strictDocumentation && documentationImpact == DocumentationImpact.PublicApi)
        {
            return new FileVerdict(path, true, true, "public API documentation changed and --strict-docs was requested", documentationImpact);
        }

        return new FileVerdict(path,
                               false,
                               true,
                               $"{beforeTokens.Count} tokens identical ({literalCount} literal), directives and disabled text identical, changed trivia: {changedCategories}",
                               documentationImpact);
    }

    /// <summary>
    /// Determines whether a syntax tree contains parse errors
    /// </summary>
    /// <param name="tree">Tree to inspect</param>
    /// <returns><c>true</c> when the tree contains an error diagnostic</returns>
    private static bool HasParseErrors(SyntaxTree tree) => tree.GetDiagnostics().Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);

    /// <summary>
    /// Describes a token by kind and text
    /// </summary>
    /// <param name="token">Token to describe</param>
    /// <returns>Comparable description</returns>
    private static string DescribeToken(SyntaxToken token) => $"{token.RawKind}:{token.Text}";

    /// <summary>
    /// Determines whether a token carries literal content
    /// </summary>
    /// <param name="token">Token to inspect</param>
    /// <returns><c>true</c> for string, character, and raw string literal tokens</returns>
    private static bool IsLiteralToken(SyntaxToken token) => token.IsKind(SyntaxKind.StringLiteralToken)
                                                          || token.IsKind(SyntaxKind.CharacterLiteralToken)
                                                          || token.IsKind(SyntaxKind.InterpolatedStringTextToken)
                                                          || token.IsKind(SyntaxKind.SingleLineRawStringLiteralToken)
                                                          || token.IsKind(SyntaxKind.MultiLineRawStringLiteralToken)
                                                          || token.IsKind(SyntaxKind.Utf8StringLiteralToken);

    /// <summary>
    /// Describes every trivia that is not an allowed comment, documentation, or layout category
    /// </summary>
    /// <param name="root">Root node to inspect</param>
    /// <returns>Comparable descriptions, in document order</returns>
    private static List<string> DescribeStructuralTrivia(SyntaxNode root) => root.DescendantTrivia()
                                                                                .Where(trivia => _allowedTriviaKinds.Contains(trivia.Kind()) == false)
                                                                                .Select(trivia => $"{trivia.RawKind}:{trivia.ToFullString()}")
                                                                                .ToList();

    /// <summary>
    /// Names the trivia categories that changed between the two versions
    /// </summary>
    /// <param name="beforeRoot">Root node at the base revision</param>
    /// <param name="afterRoot">Root node at the head revision</param>
    /// <returns>Comma-separated category names, or <c>none</c></returns>
    private static string DescribeChangedTriviaCategories(SyntaxNode beforeRoot, SyntaxNode afterRoot)
    {
        var categories = new List<string>();

        foreach (var (name, kinds) in new (string Name, SyntaxKind[] Kinds)[]
                                      {
                                          ("comments", [SyntaxKind.SingleLineCommentTrivia, SyntaxKind.MultiLineCommentTrivia]),
                                          ("documentation", [.. _documentationTriviaKinds]),
                                          ("layout", [SyntaxKind.WhitespaceTrivia, SyntaxKind.EndOfLineTrivia])
                                      })
        {
            if (DescribeTrivia(beforeRoot, kinds).SequenceEqual(DescribeTrivia(afterRoot, kinds)) == false)
            {
                categories.Add(name);
            }
        }

        return categories.Count == 0 ? "none" : string.Join(", ", categories);
    }

    /// <summary>
    /// Describes every trivia of the given kinds
    /// </summary>
    /// <param name="root">Root node to inspect</param>
    /// <param name="kinds">Trivia kinds to collect</param>
    /// <returns>Comparable descriptions, in document order</returns>
    private static List<string> DescribeTrivia(SyntaxNode root, SyntaxKind[] kinds) => root.DescendantTrivia()
                                                                                           .Where(trivia => kinds.Contains(trivia.Kind()))
                                                                                           .Select(trivia => $"{trivia.RawKind}:{trivia.ToFullString()}")
                                                                                           .ToList();

    /// <summary>
    /// Determines whether changed documentation comments affect publicly visible members
    /// </summary>
    /// <param name="beforeRoot">Root node at the base revision</param>
    /// <param name="afterRoot">Root node at the head revision</param>
    /// <returns>Documentation impact of the change</returns>
    private static DocumentationImpact DetermineDocumentationImpact(SyntaxNode beforeRoot, SyntaxNode afterRoot)
    {
        var before = DescribeDocumentation(beforeRoot);
        var after = DescribeDocumentation(afterRoot);

        if (before.Select(entry => entry.Text).SequenceEqual(after.Select(entry => entry.Text)))
        {
            return DocumentationImpact.None;
        }

        for (var index = 0; index < Math.Min(before.Count, after.Count); index++)
        {
            if (before[index].Text != after[index].Text && (before[index].IsPublic || after[index].IsPublic))
            {
                return DocumentationImpact.PublicApi;
            }
        }

        return DocumentationImpact.Internal;
    }

    /// <summary>
    /// Collects the documentation comment of every member declaration, in document order
    /// </summary>
    /// <param name="root">Root node to inspect</param>
    /// <returns>Documentation text and visibility per member</returns>
    private static List<(string Text, bool IsPublic)> DescribeDocumentation(SyntaxNode root) => root.DescendantNodes()
                                                                                                   .OfType<MemberDeclarationSyntax>()
                                                                                                   .Select(member => (Text: string.Concat(member.GetLeadingTrivia()
                                                                                                                                                .Where(trivia => _documentationTriviaKinds.Contains(trivia.Kind()))
                                                                                                                                                .Select(trivia => trivia.ToFullString())),
                                                                                                                      IsPublic: IsPubliclyVisible(member)))
                                                                                                   .ToList();

    /// <summary>
    /// Determines whether a member reaches the generated XML documentation file
    /// </summary>
    /// <param name="member">Member to inspect</param>
    /// <returns><c>true</c> for public and protected members, interface members, and enum members</returns>
    private static bool IsPubliclyVisible(MemberDeclarationSyntax member)
    {
        // Interface and enum members carry no accessibility modifier of their own, yet both land in the generated documentation
        if (member is EnumMemberDeclarationSyntax || member.Parent is InterfaceDeclarationSyntax)
        {
            return true;
        }

        return member.Modifiers.Any(SyntaxKind.PublicKeyword) || member.Modifiers.Any(SyntaxKind.ProtectedKeyword);
    }

    /// <summary>
    /// Returns the first difference between two comparable sequences
    /// </summary>
    /// <param name="before">Sequence at the base revision</param>
    /// <param name="after">Sequence at the head revision</param>
    /// <returns>Description of the first difference, or <c>null</c> when the sequences are equal</returns>
    private static string? FirstDifference(List<string> before, List<string> after)
    {
        for (var index = 0; index < Math.Min(before.Count, after.Count); index++)
        {
            if (before[index] != after[index])
            {
                return $"at index {index}: '{Shorten(before[index])}' became '{Shorten(after[index])}'";
            }
        }

        if (before.Count != after.Count)
        {
            return $"count changed from {before.Count} to {after.Count}";
        }

        return null;
    }

    /// <summary>
    /// Shortens a value for reporting
    /// </summary>
    /// <param name="value">Value to shorten</param>
    /// <returns>Single-line value of at most 40 characters</returns>
    private static string Shorten(string value)
    {
        var singleLine = value.Replace("\r", "\\r").Replace("\n", "\\n");

        return singleLine.Length <= 40 ? singleLine : singleLine[..40] + "…";
    }

    /// <summary>
    /// Determines whether a path holds no compiled source and no build configuration
    /// </summary>
    /// <param name="path">Repository-relative path</param>
    /// <returns><c>true</c> when the path is known to be non-compiled text</returns>
    private static bool IsNonCompiledPath(string path)
    {
        var normalized = path.Replace('\\', '/');

        // A behavioral prefix wins over the extension: scripts/README.md documents executable behavior and belongs to the surface the workflows audit
        if (_behavioralPrefixes.Any(prefix => normalized.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        return normalized.EndsWith(".md", StringComparison.OrdinalIgnoreCase)
            || _nonCompiledPrefixes.Any(prefix => normalized.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Reads a file from a git revision
    /// </summary>
    /// <param name="repositoryRoot">Repository root</param>
    /// <param name="revision">Revision to read from</param>
    /// <param name="path">Repository-relative path</param>
    /// <returns>File content at that revision</returns>
    private static string ReadBlob(string repositoryRoot, string revision, string path) => Git(repositoryRoot, "show", $"{revision}:{path.Replace('\\', '/')}");

    #endregion // Methods
}
