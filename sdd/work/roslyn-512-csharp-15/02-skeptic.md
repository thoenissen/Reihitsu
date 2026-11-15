Role: skeptic
Commit: 4cd929a
Verdict: output question

The prompt carried no intended fix, suspected cause, or hoped-for verdict to ignore.

Evidence came from a scratch worktree of 4cd929a (outside the repository, removed afterwards) with every `Microsoft.CodeAnalysis`, `Microsoft.CodeAnalysis.CSharp`, `Microsoft.CodeAnalysis.CSharp.Workspaces` 5.9.0 reference set to 5.12.0 and `MaxSupportedLanguageVersion` set to `CSharp15`. "New CLI" means that worktree's `Reihitsu.Cli` (deps.json: `Microsoft.CodeAnalysis.CSharp/5.12.0`); "old CLI" means the CLI built from 4cd929a. Parser facts come from `dotnet fsi` probes that call `CSharpSyntaxTree.ParseText` / `CSharpCompilation` against the 5.9.0 and 5.12.0 packages in the NuGet cache.

## Premise

The issue is real, and a caller reaches it: the CLI's default parse version is `LanguageVersionResolver.MaxSupportedLanguageVersion` (`src/Reihitsu.Cli/Program.cs:150`), and with Roslyn 5.9 the CLI cannot parse a C# 15 file at any `--lang-version`. `preview` is clamped to 14, and `15` is rejected. On the old CLI, `public union Pet(Cat, Dog) { … }` gives "Skipped 1 file(s) with syntax errors." The analyzer package does not need the change to see C# 15 trees. Its trees come from the host's parser, so a 5.9-built package running in a 5.12 host already sees them.

What the issue gets right or wrong:

- **"Roslyn 5.9.0 cannot parse C# 15": overstated.** On 5.9.0, `public union Pet(Cat, Dog);` parses as `UnionDeclaration` with no errors under `LanguageVersion.Preview`. What 5.9 lacks is the public `LanguageVersion.CSharp15` member (enum ends at `CSharp14`) and `Latest → CSharp15`. The formatter has to name `CSharp15`, so the motivation stands, but the claim is wrong as stated.
- **"MaxSupportedLanguageVersionMatchesRoslynLatest fails until the maximum is raised": not observed.** With only the package bump, I did not run that test separately. With the bump plus the raise, `LanguageVersionResolverTests` shows 8 passed and 1 failed. The failing test is `ExceedsMaximumIsTrueOnlyAboveTheMaximum`, because `(LanguageVersion)1500` is `CSharp15` on 5.12. `ResolveClampsVersionsAboveTheMaximum` still passes, but it no longer tests a clamp: `Resolve((LanguageVersion)1500)` returns `CSharp15`, which is now the maximum (see *Breaking inputs*, FC-017). The issue names only the first of these two tests.
- **"Update the CLI help text": no code change needed.** The help line is built from the constant (`Program.cs:292`). The new CLI prints `latest selects 15.0, the newest supported version; …` with no edit.
- **"The Debug build succeeds with no source changes": confirmed** for the CLI (0 warnings, 0 errors). The other test projects are green on the bumped worktree (*Whole run*).

Each CLI fact in the spec diff was run on the new CLI against a folder holding `U1.cs` (the change doc's union file) and `K1.cs` (the change doc's `class union` file):

| `--lang-version` | Result |
|---|---|
| (none) / `latest` / `15` / `15.0` | `U1.cs` formatted, `K1.cs` skipped with syntax errors |
| `14` | `K1.cs` formatted, `U1.cs` skipped with syntax errors |
| `preview` | `Warning: C# language version 'preview' is newer than the newest supported version 15.0; formatting for 15.0.` then as for `15` |
| `16` | `Invalid value for --lang-version: '16'. …`, exit 2 (unchanged class of behavior) |

The change doc's union before/after block reproduces exactly (`U1.cs`), and so does the one-line union (`U2.cs`).

### Is the spec wrong instead?

FMT-PIPE-008's `Why:` reads "Clamping keeps the formatter's output stable when Roslyn learns a version the formatter has not been reviewed against." The formatter has not been reviewed against C# 15 constructs. Union layout is #1762, and the outputs below show that work is still open. Read literally, the `Why:` supports option B of the spec author's output question 1, which keeps the maximum at 14. Option A is consistent only if "reviewed" means "its version-gated rules were reviewed". That holds: no production gate compares above `CSharp12` (`rg "LanguageVersion\." src` outside tests: the gates are C# 6, 7, 9, 10, 11, 12). This tension belongs with output question 1. If A is chosen, the spec author should reword the `Why:` so it does not read as "the formatter reviewed C# 15 output".

### Conflicts

- **Change doc, *What*:** "input that is valid C# 14 is formatted exactly as before under C# 15" is false. The doc's own `class union` example contradicts it, and so does the wider class under *Breaking inputs*. It should say "input that Roslyn parses the same way under C# 14 and C# 15". The *Formatter entry points* paragraph has the same wording issue. Its point (the project parses the tree, not the formatter) is correct.
- **No conflict with SHARED-DIAG-010 / P11.** No analyzer behavior changes, and the analyzers already see C# 15 trees in a 5.12 host.
- **Visible change missing from the change doc:** the `closed` keyword (*Breaking inputs*, member class 1).

## Mechanism and candidates

_N/A_ (not a bug). For a behavior change, the candidate set is every syntax shape whose parse differs between C# 14 and C# 15 on Roslyn 5.12, and every C# 15 construct that the default CLI now reaches. I took both from the 5.12 parser by running probes, not from the issue text.

## Breaking inputs

### Member class 1: C# 14-valid code that has syntax errors under C# 15 (formatted before, skipped after)

Axes: the keyword (`union`, `closed`; `safe` breaks only under `Preview`, which stays clamped, so no caller reaches it) and the position of the type name. Probe results on 5.12, C# 14 vs C# 15:

| Position | `union` | `closed` |
|---|---|---|
| method return type in a type body: `union M(int a) { … }` | **errors** (CS1519) | **errors** |
| field type: `union F;` / `static union F;` | **errors** | **errors** |
| property type: `union P { get; }` / `public union P => null;` | **errors** | **errors** |
| indexer type: `public union this[int i] => null;` | **errors** | **errors** (CS1031) |
| generic: `union<int> F;` | **errors** | **errors** |
| interface method return: `union M();` | **errors** (CS1031) | holds |
| constructor of the type itself: `class closed { public closed() { } }` | holds | **errors** (CS8124) |
| top-level statement `union x = null;` | **errors** | holds |
| record field: `closed R;` (type is a record) | (covered by field) | **errors** |
| local, parameter, qualified (`N.union F;`), `event union E;`, delegate return, namespace name, member named `union`/`closed` | holds | holds |

Old CLI vs new CLI, confirmed for three files: `class union { } class C { union M(int a) { return null; } }`, `class closed { } class C { closed F; closed M() => null; }`, and `class union { } class C { union F; union P { get; } }`. Each was formatted before and is skipped with "Skipped 1 file(s) with syntax errors." after. This is non-destructive: the file is left unchanged. It is rare. A lowercase type named `union` or `closed` violates RH4002, RH4003, RH4010, or RH4011 (type-name PascalCase). A C# 15 project cannot compile this code anyway, and `--lang-version 14` restores the old result. The change doc lists only the `union` method-return member. The `closed` members are missing.

### Member class 2: C# 15 constructs the default CLI now formats (skipped before)

Checks on the new CLI's output: parsed under C# 15 (no errors in any output), comment and directive counts compared, and a second pass with `--check` (0 files need formatting, LF only).

- `union` at namespace level (block and file-scoped), nested in a class, generic with `where`, with an attribute, with a base list, as an empty body, with a `;` body, or with a multi-line case list holding comments. Every output compiles at the parse level, keeps every comment, `#region`, and `#if`, and is stable on the second pass.
- **The layout the spec author's question 1 does not show.** A union whose braces stand on one line but whose member gets a block body comes out as follows (input `private union Inner(int,string) { public override string ToString() { return "x"; } }` inside a class):

  ```csharp
          private union Inner(int, string) { public override string ToString()
              {
                  return "x";
              } }
  ```

  The same holds for `public union Shape(Circle,\n    Square) : System.IEquatable<Shape> { public bool Equals(Shape other) => true; }`: the expression body becomes a block inside one-line union braces. The output compiles and keeps its comments, but it is the worst visible result of option A. The maintainer should see it next to the best case. The same input as a `class` gets its braces on their own lines.
- `closed class` / `closed record` and `[with(…)]` collection elements: **no change.** Both Roslyn versions parse them version-independently. `closed class B { }` is a `ClassDeclaration`, and `[with(1)]` is a `WithElement` under C# 14 too. The old and new CLI output were byte-identical on my fixtures. The phases already see these today.
- A comment as the first content of a union's case list gets a blank line after `(`. This is inherited: a `record` parameter list gives the same output on the old CLI. It is not union-specific (see *Follow-up candidates*).

### Failure classes that reach the change

- **FC-017** (assertion weaker than the guarantee): `ResolveClampsVersionsAboveTheMaximum` and `ExceedsMaximumIsTrueOnlyAboveTheMaximum` use `(LanguageVersion)1500` as "above the maximum". On 5.12 that value is the maximum, so both tests need a value above `CSharp15` (for example `(LanguageVersion)1600`). Otherwise the clamp test passes without testing a clamp.
- **FC-031** (stale comment): the summaries in `LanguageVersionResolverTests` and the `FormattingContext` docs mention the maximum only symbolically. Any text that says "C# 14" has to move. `rg "C# 14|14\.0"` over `src` found only test names that pin `CSharp14` on purpose (`GetOnlyMemberExpressionBodyTests`, RH7309), and those stay.
- **FC-010** (idempotency): held on every member-class-2 fixture (LF only).
- No other catalog class reaches a constant and package change.

Candidate inputs for the test implementer: the FMT-PIPE-008 and CLI-004 examples of the spec author, plus `ExceedsMaximum(CSharp15) == false`, `Resolve(CSharp15) == CSharp15`, a version above 15 clamped, and, depending on the answer to the output question, a CLI test that a `closed`-typed member file is skipped under the default and formatted under `--lang-version 14`.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `Reihitsu.Formatter` (`LanguageVersionResolver`) | must change: the constant |
| `Reihitsu.Formatter` phases | must stay: no gate above C# 12; the union layout belongs to #1762 |
| `Reihitsu.Cli` (`Program`) | must stay: the help, the warning, and `TryParse("15")` all follow from the constant and the package |
| CLI package | package reference changes (Roslyn 5.12 bundled); runtime already requires .NET 11 |
| `Reihitsu.Analyzer` / `.CodeFixes` / package | package reference changes, which raises the host floor from Roslyn 5.9 to 5.12 (visible; output question 2); no rule behavior changes |
| `Reihitsu.Core` | package reference only |
| `Reihitsu.Tooling` (`FixtureRunner`, `LanguageVersion.Latest`) | package reference only; fixtures now parse at C# 15, and none uses `union` or `closed` as a type name |
| Code fixes / Fix All | not involved: the project's parse options decide the tree; `Resolve` only selects gates ≤ C# 12 |
| Rule pages | not involved |
| Tests | `LanguageVersionResolverTests` (two tests, FC-017), new CLI-004 tests |
| Analyzer test infrastructure | not involved in this issue (see *Issue goals*) |
| User docs | no page states the host-compiler floor (`README.md`, `src/Reihitsu.Analyzer.Package/README.MD` checked); the change doc routes it to the 1.3.0 release notes |

## Refuse option

- **Refuse: keep the maximum at C# 14 on Roslyn 5.12.** The spec author's question 1, option B. Cost: zero production lines. The CLI skips every C# 15 file by default and lowers `--lang-version 15` with a warning until #1762 lands. Member class 1 is then not affected at all. This is the option the literal FMT-PIPE-008 `Why:` supports. It is not "rare": everyone who writes a union is refused.
- **Narrower: keep the analyzer package on Roslyn 5.9 and bundle 5.12 only in the CLI.** The formatter would have to name C# 15 as `(LanguageVersion)1500`, because 5.9's public enum stops at `CSharp14`. That puts a hidden version constant into the single owner of the maximum. The siblings in milestone 1.3.0 need the 5.12 API anyway, so the floor rises in the same release either way. Not recommended. I list it because it is the only way to keep the package's host floor in 1.3.0 if the siblings slip.
- **Declare a shape unsupported:** not an option here. No phase gains or loses a special case from this change, and a union is a syntax-valid construct the precheck would have no reason to refuse.
- **Member class 1** needs no refusal: the file is already skipped, not damaged.

## Output questions

These are additions to the spec author's two questions and do not replace them.

**A · CLI default language version — a type named `closed`** (extends the spec author's question 2, which shows only `union`)

Input:
```csharp
class closed { }
class C { closed F; closed M() => null; }
```

Before (`main`):
```csharp
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
(Shape from the old CLI's run; the exact text is for the test implementer to pin.)

After, A (recommended): file skipped, "Skipped 1 file(s) with syntax errors."; `--lang-version 14` formats it as before.

After, B: same as A. No other option exists without keeping the maximum at 14 (spec author's question 1, B).

Why A: it is how the C# 15 compiler reads the code, and a lowercase type name is already reported by RH4002.

**B · CLI default language version — union with one-line braces around a member** (the worst case of the spec author's question 1, A)

Input:
```csharp
public class Outer
{
    private union Inner(int,string) { public override string ToString() { return "x"; } }
}
```

Before (`main`): file skipped, "Skipped 1 file(s) with syntax errors."

After, A (recommended, as with question 1 A):
```csharp
public class Outer
{
    private union Inner(int, string) { public override string ToString()
        {
            return "x";
        } }
}
```

After, B: file skipped as before (question 1, B).

Why A: the output compiles, keeps every comment, and is stable; #1762 owns the brace layout, and the same milestone ships it.

**C · Analyzer package — host compiler floor** (the change doc lists it; the lead already plans to ask every listed change, so this is a reminder that it needs its own answer)

Before: the package loads in hosts with Roslyn ≥ 5.9. After: only in hosts with Roslyn ≥ 5.12 (.NET 11 SDK 11.0.100 or a matching Visual Studio). In an older host no Reihitsu diagnostic and no code fix appears. I did not run an older host. The "loads none of the analyzers" behavior is the compiler's documented reaction to an analyzer built against a newer compiler and is not verified here.

## Internal choices

- **Change-doc wording** (*What* and *Formatter entry points*): replace "valid C# 14 input is formatted exactly as before" with "input that parses the same under C# 14 and C# 15". Add the `closed` members to the *CLI, a type named `union`* section, renamed to cover both keywords. Recommend: amend. No output changes beyond what question A asks.
- **FMT-PIPE-008 `Why:`** if question 1 is answered A: reword so it says the clamp keeps version-gated rules from following an unreviewed Roslyn `Latest`, not that the output for all C# 15 syntax was reviewed. Recommend: amend.
- **Above-the-maximum value in tests:** `(LanguageVersion)1600` versus deriving "max + 100". Recommend a literal with a comment that it is above every version Roslyn 5.12 knows, matching the existing style.

## Issue goals

- **"Update `ResolveKeepsConcreteVersionsUpToTheMaximum` and `ExceedsMaximumIsTrueOnlyAboveTheMaximum`":** change. `ResolveClampsVersionsAboveTheMaximum` must change too (FC-017), and the `(LanguageVersion)1500` in `ExceedsMaximumIsTrueOnlyAboveTheMaximum` is the failing assert, not only the C# 14 line. Not visible.
- **"Update the CLI help text":** drop as a code task. The help is derived from the constant, and only tests and specs follow. Not visible beyond what the constant already changes.
- **"The test infrastructure can compile a union declaration, or a documented alternative exists":** drop from this run and move the facts to the first union sub-issue. Facts, all run: a union compiled with Roslyn 5.12 against .NET 10 reference assemblies fails with `CS0518: Predefined type 'System.Runtime.CompilerServices.IUnion' is not defined or imported` and `CS0656: Missing compiler required member 'System.Runtime.CompilerServices.UnionAttribute..ctor'`. Against the SDK's `Microsoft.NETCore.App.Ref/11.0.0/ref/net11.0` it compiles with no error. `Microsoft.CodeAnalysis.Analyzer.Testing` 1.1.4 knows reference-assembly sets only up to `net10.0`, so `ReferenceAssemblies.Net.Net90` (and any predefined set) cannot compile a union. A .NET 11 set has to be constructed by hand, or the types polyfilled. Nothing in this issue compiles a union, so building the infrastructure here would be scope with no consumer. Not visible.
- **"No formatter or analyzer behavior for C# 15 constructs is changed here":** keep, but it is only true for code fixes. For the CLI the run does change what users see on C# 15 constructs (skipped → formatted), which is the spec author's question 1 plus question B above.
- **"Findings so far: Roslyn 5.9.0 cannot parse C# 15":** wrong as stated (see *Premise*). It does not change the plan, and the PR should not repeat it.

## Architecture findings

_None._ The maximum has one owner (`LanguageVersionResolver`). The CLI's help, warning, and default all read it.

## Follow-up candidates

- **Blank line after `(` in front of a first comment in a parameter list:** `public record Pet2(\n    // the first case\n    int Cat,\n    int Dog);` gets an empty line between `(` and the comment, on `main` and on the new CLI alike. FMT-BLANK-011 exempts only the first content of a block, a type body, or a switch section, so the output follows the spec as written. A union's case list inherits it. It is outside this run's issues, likely rare (comments in parameter lists), and not checked against existing issues. If filed, it is an open question in output form for the maintainer: should the first content of a parameter list (and of a union's case list) be exempt like a type body's?
- **Test infrastructure facts for unions** (*Issue goals*): attach them to the first union sub-issue of thoenissen/Reihitsu#1716 that compiles a union in analyzer tests.

## Whole run

Every test project except the self-hosting scans, run on the bumped worktree (package bump plus the maximum raised to `CSharp15`; no test changed), Release, with `scripts/test.sh` per project:

| Project | Result |
|---|---|
| analyzer | 10091 passed, 0 failed |
| formatter | 5195 passed, 1 failed: `LanguageVersionResolverTests.ExceedsMaximumIsTrueOnlyAboveTheMaximum` (`(LanguageVersion)1500` is no longer above the maximum) |
| core | 682 passed |
| cli | 281 passed |
| tooling | 126 passed |
| architecture | 118 passed |
| playground | 23 passed |

So the bump compiles and the suite is green apart from the FC-017 pair (one failing test, one that still passes but no longer tests a clamp). The self-hosting scans were not run; CI runs them.
