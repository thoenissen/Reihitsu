# Roslyn 5.12 and C# 15 as the newest supported language version

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | FMT-PIPE-008, CLI-004 |

## What

Every project of the repository moves from Roslyn (`Microsoft.CodeAnalysis`, `Microsoft.CodeAnalysis.CSharp`, `Microsoft.CodeAnalysis.CSharp.Workspaces`) 5.9.0 to 5.12.0, and the newest language version the formatter supports moves from C# 14 to C# 15 (FMT-PIPE-008). `Default`, `Latest`, and `LatestMajor` resolve to C# 15, `Preview` is clamped to C# 15, and the CLI parses files as C# 15 when `--lang-version` is absent or `latest` (CLI-004).

No formatter rule is gated on a version above C# 12, so input that Roslyn parses the same way under C# 14 and C# 15 is formatted exactly as before. Input that parses differently — C# 14 code that uses a type named `union` or `closed` where C# 15 reads a keyword — is listed below. This change specifies no layout, spacing, or analyzer behavior for C# 15 constructs (union declarations, `with(…)` collection-expression elements, the `closed` and `safe` modifiers); that is the work of the other sub-issues of thoenissen/Reihitsu#1716 (#1762 to #1767). Whatever the phases do to such a construct today, they keep doing; this change only makes the CLI reach it by default.

## Why

Roslyn 5.9 parses a union declaration only under `LanguageVersion.Preview`: its public `LanguageVersion` enum ends at `CSharp14`, and it maps `Latest` to C# 14. The formatter cannot name C# 15 as its maximum on 5.9, and the CLI, which clamps `preview` to the maximum and rejects `15`, cannot parse a C# 15 file at any `--lang-version`. Roslyn 5.12 adds `LanguageVersion.CSharp15` and maps `Latest` to it. Keeping the formatter's maximum at C# 14 on 5.12 would make the CLI's default parse every C# 15 construct as a syntax error and skip the file, and a `--lang-version 15` request would be lowered with a warning.

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

No line-break rule covers a union's braces yet (#1762), so a union written with its braces on one line keeps them there, also when a member inside gets a block body:

```csharp
// input, reihitsu-format with no --lang-version
public class Outer
{
    private union Inner(int,string) { public override string ToString() { return "x"; } }
}
```

```csharp
// before: unchanged; "Skipped 1 file(s) with syntax errors."
```

```csharp
// after
public class Outer
{
    private union Inner(int, string) { public override string ToString()
        {
            return "x";
        } }
}
```

**CLI, a type named `union` or `closed`.** C# 15 reads `union` and `closed` as keywords in some positions where C# 14 read a type name: as the type of a field, property, or indexer, as a method's return type, as a generic type (`union<int> F;`), and further `union` as an interface method's return type and in a top-level `union x = null;`, and `closed` as the name of a constructor of a type named `closed`. A file that uses such a type name there now has syntax errors under the default version and is skipped; `--lang-version 14` formats it as before. Such a lowercase type name already violates the type-naming rules.

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

```csharp
// input, reihitsu-format with no --lang-version
class closed { }
class C { closed F; closed M() => null; }
```

```csharp
// before
class closed;
class C
{
    closed F;
    closed M()
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

**Analyzer package, host compiler.** `Reihitsu.Analyzer`, its code fixes, and the formatter they carry are compiled against Roslyn 5.12, so they load only in a compiler of at least that version: the .NET 11 SDK (11.0.100 ships Roslyn 5.12) or a Visual Studio release with the same compiler. Before, the floor was Roslyn 5.9. In an older host the compiler reports that the analyzer assembly references a newer compiler and does not load the package's analyzers, so no Reihitsu diagnostic and no code fix appears. This belongs in the release notes of 1.3.0.

**Formatter entry points used by code fixes and whole-document formatting.** None. The tree is parsed by the project, not by the formatter, so the Roslyn upgrade does not change which documents have syntax errors, and a 5.9-built package in a 5.12 host already sees C# 15 trees. The resolved version only selects version-gated rules, and none is gated above C# 12.

## Decisions

### The default language version becomes C# 15

Maintainer's answer: raise the maximum to C# 15. `latest` and the absent option select C# 15, the help names 15.0, the `preview` warning names 15.0, and `--lang-version 15` and `15.0` are accepted.

### The interim union layout is accepted until #1762

Maintainer's answer: the CLI formats files with union declarations now, and a union's braces stay where the phases leave them — including the one-line braces around a block-bodied member shown above — until #1762 specifies union layout.

### A type named `union` or `closed` follows C# 15

Maintainer's answer: C# 14 code that uses a type named `union` or `closed` in a position C# 15 reads as a keyword is skipped with syntax errors under the default; `--lang-version 14` restores the old result. An existing test of the repository that uses such a type name and breaks is fixed by renaming the type.

### The analyzer package's host floor rises to Roslyn 5.12

Maintainer's answer: accepted; the raised minimum host compiler is a release-note item of 1.3.0.

### One Roslyn version for every project

The formatter ships inside the analyzer package and the CLI, and both share `Reihitsu.Core`; mixing Roslyn versions across them would compile the formatter against one version and run it against another. All projects move to 5.12.0 together.

### The maximum is a reviewed version

FMT-PIPE-008 keeps the maximum as a fixed version rather than Roslyn's `Latest`, so a later Roslyn upgrade cannot switch on version-gated rules unnoticed. The maximum names the newest version whose gates were reviewed, not a version whose every construct has a reviewed layout; FMT-PIPE-008's `Why:` says so. This change raises it once, to C# 15, together with the Roslyn version that names C# 15.

### `Preview` stays clamped

Roslyn 5.12's `Preview` includes experimental syntax (the memory-safety preview, `UnsafeExpressionSyntax`) and the `safe` keyword, which are out of scope for C# 15 support. `Preview` keeps being clamped to the maximum, now C# 15.

## Alternatives considered

- **Upgrade Roslyn but keep the maximum at C# 14 until the C# 15 layout work lands** — rejected by the maintainer's answer: the CLI would skip every C# 15 file by default and warn on `--lang-version 15`, while code fixes on a C# 15 project already see C# 15 trees through the host's parser.
- **Compile the analyzer package against Roslyn 5.9 and only the CLI against 5.12** — rejected: the formatter is one assembly shipped in both, and would have to name C# 15 as a hidden numeric constant because 5.9's `LanguageVersion` has no `CSharp15`; the C# 15 sub-issues of the same release need the 5.12 API anyway.
