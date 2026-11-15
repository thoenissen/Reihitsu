# Roslyn 5.12 and C# 15 as the newest supported language version

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | FMT-PIPE-008, CLI-004 |

## What

Every project of the repository moves from Roslyn (`Microsoft.CodeAnalysis`, `Microsoft.CodeAnalysis.CSharp`, `Microsoft.CodeAnalysis.CSharp.Workspaces`) 5.9.0 to 5.12.0, and the newest language version the formatter supports moves from C# 14 to C# 15 (FMT-PIPE-008). `Default`, `Latest`, and `LatestMajor` resolve to C# 15, `Preview` is clamped to C# 15, and the CLI parses files as C# 15 when `--lang-version` is absent or `latest` (CLI-004).

No formatter rule depends on a version above C# 14, so input that is valid C# 14 is formatted exactly as before under C# 15. This change specifies no layout, spacing, or analyzer behavior for C# 15 constructs (union declarations, `with(…)` collection-expression elements, the `closed` and `safe` modifiers); that is the work of the other sub-issues of thoenissen/Reihitsu#1716 (#1762 to #1767). Whatever the phases do to such a construct today, they keep doing; this change only makes the CLI reach it by default.

## Why

Roslyn 5.9 cannot parse C# 15, so no C# 15 support can be built on it. Roslyn 5.12 reports `LanguageVersion.Latest` as C# 15; keeping the formatter's maximum at C# 14 on 5.12 would make the CLI's default parse every C# 15 construct as a syntax error and skip the file, and a `--lang-version 15` request would be lowered with a warning.

## Visible output changes

**CLI, default language version.** A file that uses C# 15 syntax was skipped as a file with syntax errors; it is now formatted. C# 15 constructs are left as the phases treat them today; the code around them is formatted as usual.

```csharp
// input, reihitsu-format with no --lang-version
public union Pet(Cat, Dog)
{
    public int Legs() => 4;
}

public class Cat { }

public class Dog { }
```

```csharp
// before: unchanged; "Skipped 1 file(s) with syntax errors."
```

```csharp
// after
public union Pet(Cat, Dog)
{
    public int Legs()
    {
        return 4;
    }
}

public class Cat;

public class Dog;
```

A union written on one line, `public union Pet(Cat, Dog) { public int X => 1; }`, stays on one line, because no line-break rule covers a union's braces yet (#1762); the class declarations in the same file are formatted.

**CLI, a type named `union`.** C# 15 reads `union M(…) { … }` inside a type as a union declaration, so a file that uses a type named `union` as a member's return type now has syntax errors under the default version and is skipped; `--lang-version 14` formats it as before.

```csharp
// input, reihitsu-format with no --lang-version
class union { }
class C { union M(int a) { return null; } }
```

```csharp
// before
class union;
class C
{
    union M(int a)
    {
        return null;
    }
}
```

```csharp
// after: unchanged; "Skipped 1 file(s) with syntax errors."
```

**CLI, `--lang-version 15` and `15.0`.** Before: `Invalid value for --lang-version: '15'. Expected a C# language version such as 12, 12.0, latest, or preview.`, exit code 2. After: accepted, the files are parsed as C# 15, no warning.

**CLI, `--lang-version preview`.** The warning names the new maximum:

```text
before: Warning: C# language version 'preview' is newer than the newest supported version 14.0; formatting for 14.0.
after:  Warning: C# language version 'preview' is newer than the newest supported version 15.0; formatting for 15.0.
```

**CLI, `--help`.** The line under `--lang-version` reads `latest selects 15.0, the newest supported version; …` instead of `latest selects 14.0, …`.

**Analyzer package, host compiler.** `Reihitsu.Analyzer`, its code fixes, and the formatter they carry are compiled against Roslyn 5.12, so they load only in a compiler of at least that version: the .NET 11 SDK (11.0.100 ships Roslyn 5.12) or a Visual Studio release with the same compiler. Before, the floor was Roslyn 5.9. In an older host the compiler reports that the analyzer assembly references a newer compiler and loads none of the package's analyzers, so no Reihitsu diagnostic and no code fix appears. This belongs in the release notes of 1.3.0.

**Formatter entry points used by code fixes and whole-document formatting.** None for input that is valid C# 14: the resolved version only selects version-dependent rules, and none is gated above C# 14. The tree is parsed by the project, not by the formatter, so the Roslyn upgrade does not change which documents have syntax errors.

## Decisions

### One Roslyn version for every project

The formatter ships inside the analyzer package and the CLI, and both share `Reihitsu.Core`; mixing Roslyn versions across them would compile the formatter against one version and run it against another. All projects move to 5.12.0 together.

### The maximum follows Roslyn's `Latest`

FMT-PIPE-008 keeps the maximum as a reviewed, fixed version rather than Roslyn's `Latest`, so a later Roslyn upgrade cannot raise it unnoticed. This change raises it once, to C# 15, together with the Roslyn version that introduces C# 15.

### `Preview` stays clamped

Roslyn 5.12's `Preview` includes experimental syntax (the memory-safety preview, `UnsafeExpressionSyntax`) that is out of scope for C# 15 support. `Preview` keeps being clamped to the maximum, now C# 15.

## Alternatives considered

- **Upgrade Roslyn but keep the maximum at C# 14 until the C# 15 layout work lands** — rejected: the CLI would skip every C# 15 file by default and warn on `--lang-version 15`, while code fixes on a C# 15 project already see C# 15 trees through the host's parser; the two surfaces would disagree, and the C# 15 sub-issues could not be tested through the CLI.
- **Compile the analyzer package against Roslyn 5.9 and only the CLI against 5.12** — rejected: the formatter is one assembly shipped in both, and `LanguageVersion.CSharp15` does not exist in 5.9.
