Role: spec author
Commit: d3ccab1
Verdict: change doc written

Change doc: `sdd/changes/2026-11/roslyn-512-csharp-15.md` (Status: Draft). The prompt carried no intended fix or verdict to ignore.

## Changed requirements

- FMT-PIPE-008 — Version-dependent rules follow the language version — changed: the newest supported version is C# 15 (was C# 14).
- CLI-004 — The language version comes from `--lang-version` — changed: the example warning names 15.0; a second example states that `--lang-version 15`/`15.0` parses as C# 15 without a warning, and that a union declaration is formatted under C# 15 and skipped as a syntax error under `--lang-version 14`.

No requirement added or retired; `requirement-ids.txt` is unchanged. No rule page changes. No other spec names C# 14 as the maximum (checked with `rg "C# 14|14\.0"` over `sdd/specs` and `documentation`).

## Examples for the tests

- FMT-PIPE-008 — `Latest`, `Default`, `LatestMajor` resolve to C# 15; `Preview` is clamped to C# 15; C# 15 resolves to itself; `ExceedsMaximum` is false for C# 15 and true for `Preview` (the existing `LanguageVersionResolverTests` cases named in the issue).
- CLI-004 — `--lang-version preview` warning text with 15.0; `--lang-version 15` and `15.0` accepted without a warning; a file with `public union Pet(Cat, Dog);` is formatted under the default and skipped with syntax errors under `--lang-version 14`.

How the visible changes were established: a scratch worktree of d3ccab1 with every `Microsoft.CodeAnalysis*` 5.9.0 reference set to 5.12.0 and the maximum set to `CSharp15` built the CLI with 0 warnings; the same samples were run through it and through the unchanged CLI (worktrees removed afterwards). Results are the before/after blocks in the change doc. Further facts from a Roslyn probe: on 5.9, `LanguageVersionFacts.TryParse("15")` fails and `Latest` maps to C# 14; on 5.12, `Latest` maps to C# 15, and `union M(int a) { return null; }` inside a class parses as a `UnionDeclaration` with CS1519 under C# 15 but as a method under C# 14. The installed .NET SDK 11.0.100 ships Roslyn 5.12.0. No shipped source compares against a `LanguageVersion` above `CSharp12` except the maximum itself, which backs the statement that valid C# 14 input formats identically under C# 15.

## Output questions

**1 · CLI default language version — files with C# 15 syntax**

Raising the maximum makes `reihitsu-format` format C# 15 files by default, while union layout is not done yet (#1762). Input:

```csharp
public union Pet(Cat, Dog) { public int X => 1; }

public class Cat { }
```

Before (`main`): file skipped, "Skipped 1 file(s) with syntax errors."

After, A (recommended — the issue's plan):

```csharp
public union Pet(Cat, Dog) { public int X => 1; }

public class Cat;
```

After, B: file skipped as before (keep the maximum at C# 14 on Roslyn 5.12; `--lang-version 15` keeps being lowered to 14.0 with a warning).

Why A: the sub-issues #1762–#1767 that finish C# 15 support share milestone 1.3.0, the interim output is non-destructive, and code fixes on a C# 15 project already see the same trees.

**2 · CLI default language version — a type named `union` used as a return type**

```csharp
class union { }
class C { union M(int a) { return null; } }
```

Before (`main`): formatted (`class union;`, `C` and `M` broken onto lines).

After, A (recommended): file skipped with syntax errors under the default; `--lang-version 14` formats it as before.

After, B: same as A — no other option exists without keeping the default at C# 14 (question 1, B). Listed so the maintainer sees the case; it follows from question 1.

Why A: it is how the C# 15 compiler reads the code; a lowercase type named `union` is rare and the option restores the old result.

The other visible changes (help line, `preview` warning text, `--lang-version 15` accepted, the analyzer package's raised host-compiler floor) are listed in the change doc's `Visible output changes`; they follow directly from the issue's goals and have no competing reading, but the lead turns every listed change into an output question.

## Internal choices

- The issue's test-infrastructure goal (can analyzer tests compile a union declaration against `ReferenceAssemblies.Net.Net90`, or do they need .NET 11 reference assemblies or a polyfill) is tooling with no visible output; no requirement is written for it. Alternative: a requirement in an analyzer test spec — none exists for test infrastructure.
- The C# 15 construct layouts are not written as non-goals of `formatter/pipeline.md`: they are undecided output questions of #1762 and #1763, and `sdd/README.md` forbids recording an undecided case as a non-goal. The change doc's *What* states they are out of this change's scope instead.
- The `Preview` clamp is kept unchanged (FMT-PIPE-008 already demands it); recorded as a decision in the change doc.

## Follow-up candidates

_None._ The C# 15 construct gaps seen while probing (one-line union bodies left as written) are already filed as #1762–#1767.
