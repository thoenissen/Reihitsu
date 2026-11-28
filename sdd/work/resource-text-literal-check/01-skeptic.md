Role: skeptic
Commit: c1992ef
Verdict: proceed narrower

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Premise

The class is real, and so is the risk that it comes back. The issue's second half, "after the literals are replaced", is stale: no literal is left to replace.

- Recurrence is documented. `squad/notes/2026-11-21-squad-short-lanes.md:63` records that quoted literals widened the RH5201 wording run and points to follow-up #1912. Commit `5db426f` (#1967, the clean-up for #1912) replaced the literals in 24 test files.
- The tree today is clean. I scanned every `.cs` file under `tests/` (1135 files, 26 MB, `bin/` and `obj/` skipped). The scan pulled out each regular, verbatim, interpolated, and raw string literal and compared it with the 943 `<data>` values (551 distinct) of both `.resx` files, which an XML parser read. It reported **0 hits** at `c1992ef`, both when a literal must equal a value and when it only contains the value (or, for a value with placeholders, its longest fixed fragment). Non-`.cs` files under `tests/` also hold **0** resource texts. The scan is a Python text scan written for this check (`/tmp/claude-0/-home-user-Reihitsu/d4cf7f09-29b6-563b-89f1-c0623e627fe4/scratchpad/scan3.py`, `scan4.py`), not a Roslyn parse.
- The scan does find the class. I ran it on the tree before the clean-up (`git archive 5db426f^ tests src/Reihitsu.Analyzer/AnalyzerResources.resx src/Reihitsu.Analyzer.CodeFixes/CodeFixResources.resx`). Equality found 261 literal sites. Containment found 268, in exactly the 24 files #1967 changed, and nothing outside them. So that tree had no false positive under either matching rule.
- The 7 sites that only containment finds, or that equality finds only per interpolated-string text part:
  - `tests/Reihitsu.SelfHosting.Test/Analyzers/LineCountSelfHostingTests.cs` (4 sites). The literal embeds a formatted message inside a longer string, so equality to the fragment misses it. Old line 107:
    `Assert.EndsWith("[RH2008] (RH2008MethodsShouldNotExceedTheLineLimitAnalyzer) This member has 121 lines of code, which exceeds the limit of 120.", violations[0]);`
  - `RH2008…`, `RH2009…`, `RH2111…AnalyzerTests.cs` (1 site each). The fragment equals an interpolated text part. Old `RH2111TypesShouldNotExceedTheLineLimitAnalyzerTests.cs:886`:
    `.WithMessage($"This type has {count} lines of code, which exceeds the limit of {limit}.");`
- No existing test pins this (Coverage first). Searching `tests/` for `resx` and `ResourceManager` finds `AnalyzerResourcesTests`, `CodeFixResourcesTests`, `AnalyzerPackageMetadataTests`, `LaneClassifierTests`, and `LaneCheckCommandTests`. None of them checks that test sources leave resource texts unquoted.
- Caller reachability (`AGENTS.md`, *Supported inputs*) does not apply here. This is a test-tooling guard and changes no shipped surface. The "caller" is the squad's own wording lane, and the evidence above shows it was reached twice.

## Mechanism and candidates

_N/A_: not a bug. These are the facts the check depends on:

- **Values.** `AnalyzerResources` (`src/Reihitsu.Analyzer/AnalyzerResources.cs:9`) and `CodeFixResources` (`src/Reihitsu.Analyzer.CodeFixes/CodeFixResources.cs:8`) are `internal`. Their `InternalsVisibleTo` covers only `Reihitsu.Analyzer.Test` (and CodeFixes for the analyzer). `Reihitsu.ArchitectureTests` cannot use either class directly, so it has to read the `.resx` files or go through a `ResourceManager` by manifest name.
- **XML header trap.** Both `.resx` files open with the standard XML-comment header, which holds sample `<data>` elements (`AnalyzerResources.resx:20-25`: `"this is my long string"`, `Blue`, …). An XML parser (XDocument) skips them. A regex or line-based read would pick them up, and `Blue` is exactly the "common word" false positive the issue worries about. Read only the root's `<data>` children and their `<value>`.
- **Escaping.** 23 lines in the two files hold XML entities (for example `CodeFixResources` `RH8202Title` = `Remove <value> tag`). Compare decoded values. No value contains `"`, `\`, or a line break, so line endings and C# escaping only matter on the literal side: compare the token's decoded value, not its source text.
- **Names.** All 943 names match `^RH\d{4}[A-Z]?(Title|MessageFormat)$` (588 titles, 355 message formats). There is no other kind of entry.
- **Placeholders.** 19 values use `{n}` (RH0001, RH2008, RH2009, RH2111, RH7308, RH7309, RH7311, RH7401–RH7412). Their longest fixed fragments run from 22 characters (RH7308: `' should contain only `) to 135.
- **Scope.** `tests/**/*.cs` except `bin/` and `obj/`. That includes `Reihitsu.SelfHosting.Test` (which held 4 of the old sites) and `Reihitsu.ArchitectureTests` itself. `tests/` holds no generated files (`.g.cs`, `.Designer.cs`, `.g.i.cs`). The lane classifier treats `tests/` as test paths (`src/Reihitsu.Tooling/LaneClassifier.cs:76,255`), so the `tests` lane admits the change.
- **Size.** About 26 MB of source against 551 distinct needles. My Python containment scan takes about 7.5 s. `SelfReferentialTrackerReferenceTests` already parses the whole repository, so whole-tree Roslyn scans are an accepted cost here.

## Breaking inputs

Classes that apply:
- FC-003: test each matching rule on both sides of its boundary.
- FC-033: the self-tests must not normalize away the shape. Use the real literal kinds, not just plain `"…"`.
- FC-015: a ninth private copy of `FindRepositoryRoot` (see *Architecture findings*).

Matrix of literal shape × value kind. "Old tree" says whether the pre-#1967 tree held the shape. Existing coverage is "none found" for every row.

| # | Literal shape | Value kind | Must report | Old tree |
|---|---|---|---|---|
| 1 | regular `"…"`, equal to the value | no placeholder (`"Use lambda syntax."`) | yes | 261 sites, e.g. the RH3002 files |
| 2 | regular `"…"`, value embedded in a longer literal | no placeholder | yes (containment) | none |
| 3 | regular `"…"`, holding the formatted message | placeholder (RH2008 fragment inside `"[RH2008] (…) This member has 121 lines of code, which exceeds the limit of 120."`) | yes (containment) | `LineCountSelfHostingTests` ×4 |
| 4 | interpolated `$"…{x}…"` text part equal to the fragment | placeholder | yes | RH2008/RH2009/RH2111 ×3 |
| 5 | format string literal holding `{0}` placeholders verbatim | placeholder | yes (the fragment is contained) | none |
| 6 | verbatim `@"…"` with `""`, and a regular literal with `\"` around or in the text | either | yes, on the decoded value | none |
| 7 | raw `"""…"""`, single-line and multi-line | either | yes | none |
| 8 | the text inside a comment, an XML doc comment, or `#if false` disabled text | either | no: not a literal token | none |
| 9 | literal that is just a diagnostic ID (`"RH5001"`) next to other strings in an argument array | n/a | no. No value equals or contains an ID; the shortest value is 10 characters | the CLI argument arrays #1967 left alone |
| 10 | literal that shares only a word with a value (`"Fix"`, `"casing"`) | n/a | no | none |
| 11 | a value from the `.resx` XML-comment header (`Blue`) | n/a | no | none |
| 12 | a file under `bin/` or `obj/` | either | no | none |

Assert the reported file and line on at least one positive row, since the issue requires the check to name file and line.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `tests/Reihitsu.ArchitectureTests/` | must change: the new check and its self-tests |
| Other test files under `tests/` | must stay: none quotes a resource text, so nothing needs replacing |
| `AnalyzerResources.resx`, `CodeFixResources.resx` | must stay: read only |
| Analyzer, code fixes, Fix All, formatter, `Reihitsu.Core`, CLI | not involved |
| Rule pages, specs, change doc | not involved: no spec covers architecture tests, and nothing visible changes |

## Refuse option

There is no fix or formatter output to withhold, and there is no shape to declare unsupported: no formatter phase or code fix is involved.

The refuse option here is **no change**. The lane check already catches a quoted literal in a wording run, only late: it names the wider lane, and the lead widens to `tests`. A recurrence costs one lane widening, plus the test implementer's edits. The requested change costs one architecture test file (plus, optionally, a fixture-directory type) and no production lines.

The class is not rare. 268 sites accumulated before #1967, and it showed up in two wording runs. The guard is cheap and worth it.

A narrower option is equality only, which is the issue's literal wording. It misses row 3, which really occurred (4 sites), so I do not recommend it.

## Output questions

_None._ The run changes test code only.

## Internal choices

1. **Matching rule.**
   - **A:** a literal's decoded value, or an interpolated text part, *contains* the needle. The needle is the whole value, or the longest fixed fragment when the value has placeholders.
   - **B:** equality.

   **Recommend A.** It is the only rule that catches row 3. It had no false positive on either tree, and its cost is a containment test per literal.
2. **Minimum value length (issue design point 1).** No value is a single word: the shortest is 10 characters (`Fix casing` ×28 code-fix titles, `Add braces` ×2, `Move comma`), and the shortest fragment is 22. **Recommend no minimum and no stop-list.** A future false positive is fixed by rewording the test literal. A length threshold would reopen the hole for exactly the short code-fix titles.
3. **Exemption for resource-infrastructure tests (issue design point 2).** `AnalyzerResourcesTests`, `CodeFixResourcesTests`, and `AnalyzerPackageMetadataTests` quote no resource value: they read through `ResourceManager`, the rule pages, and the README. **Recommend no exemption.** Nothing needs one, and an exemption list is a second owner of the policy.
4. **Self-trip of the check's own tests.** Under containment, a self-test that writes a *real* resource text into fixture source would be reported by the repository-wide scan, because the check's own file lies under `tests/`. **Recommend** that the scan take its needle set as a parameter, and that the self-tests use synthetic needles with fixtures in a temp directory, as `TrackerReferenceFixtureDirectory` does. Do not exempt the project.
5. **Reading values.** Parse the `.resx` with XDocument from the repository root, or load a `ResourceManager` by manifest name through reflection. **Recommend parsing the `.resx`.** It matches the other repository scans, needs no assembly internals, and the issue names the files. Either way, mind the header trap above.

## Issue goals

- "passes on the repository after the literals are replaced": **change** to "passes on the repository as it stands". There are no literals left (0 hits at `c1992ef`), so the test implementer replaces none. Nothing visible changes.
- "a string literal (or the fixed fragment of an interpolated string) equal to a value": **change** "equal" to "containing" (internal choice 1). Equality misses the self-hosting assert shape that really occurred. Nothing visible changes.
- "Argument arrays that merely contain a diagnostic ID … must not be reported": **keep it as a test row (9), but no special handling is needed.** It held under the old `rg` pattern. Under value matching an ID can never match, so the goal is met without its own code. Nothing visible changes.
- Both open design points are settled in *Internal choices* 2 and 3: no threshold, no exemption.
- Label `analyzer`: the run touches no analyzer. This is the issue manager's concern, and it changes no output.

## Architecture findings

_None_ within the run's scope.

## Follow-up candidates

- `FindRepositoryRoot` has eight private copies across `tests/Reihitsu.ArchitectureTests` (`TestScriptProjectSelectionTests.cs:268`, `RoslynPackageVersionTests.cs:943`, `SquadNoteReferenceTests.cs:368`, `DocumentationCommentNormalizationDeterminismTests.cs:69`, `SelfReferentialTrackerReferenceTests.cs:671`, `ShellScriptExecutableModeTests.cs:128`, `PhaseOwnershipDocumentationTests.cs:891`, `SpecConsistencyTests.cs:402`). There are also four near-identical `*FixtureDirectory` types and the `bin`/`obj` skip in `SelfReferentialTrackerReferenceTests.cs:598-618`. This run will likely add a ninth copy (FC-015). A clean-up that gives these one owner in the project is a candidate, outside this issue.
