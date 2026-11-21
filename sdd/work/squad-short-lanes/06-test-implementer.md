Role: test implementer
Commit: 5543a62 (base); my commit follows
Verdict: tests written

Tooling change, no requirement: no test carries a `[Requirement]` tag. Expected values come from `scripts/README.md`, *The lane check* (the script asks the classifier for the wording paths; the table's *Wording* row) and *The text-only proof* (`--exclude` matches exactly and case-insensitively), and from decision 20. The option's own contract (one path per line, no other argument) is the implementer's (05, `LaneCheckCommand` usage), since the README does not name the flag yet. My prompt carried no intended fix or hoped-for verdict to ignore.

The behavior exists on this head, so every new test passes. To see them discriminate, I mutated `LaneCheckCommand` temporarily (query filter `== Wording` → `!= Unknown`; the conflict check disabled) and ran the class: 5 of the 9 new cases failed on assertions (the path list, the no-wording diff, and the three conflict rows); reverted with `git checkout`, not committed.

## Tests

All in `src/Reihitsu.Tooling.Test/Unit/LaneCheckCommandTests.cs`.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `ExecuteWritesOnlyTheWordingPathsOfADiff`: the wording diff with change doc, spec, notes, a C# comment edit, `Cli/CliResources.resx`, `AnalyzerResources.de.resx` → exactly the two resource paths, in diff order, one per line; no category column, no verdict; empty stderr | — (tool) | `Run` | passes |
| `ExecuteWritesNoWordingPathForADiffWithoutWording` (rule page + other resx + C# file; empty diff) → exit 0, empty output (no `no changed files`), empty stderr | — | `Run` | passes |
| `ExecuteWritesAWordingPathInItsOwnSpellingWithForwardSlashes`: `SRC\reihitsu.analyzer\ANALYZERRESOURCES.resx` → written as `SRC/reihitsu.analyzer/ANALYZERRESOURCES.resx` | — | `Run` | passes |
| `ExecuteRejectsTheWordingPathsQueryWithOtherArguments` (`--lane docs`; `--text-only pass`; flag last after a full verdict call) → exit 2, empty stdout, `--wording-paths takes no other argument.` and usage on stderr | — | `Run` | passes |
| `ExecuteRejectsTheWordingPathsQueryWithAnUnknownArgument` → exit 2, `unexpected argument '--extra'.` | — | `Run` | passes |
| `ExecuteRejectsAMalformedChangeLineInTheWordingPathsQuery`: a wording line, then an `R100` line → exit 2, no path written | — | `Run` | passes |

Both sides of each condition the option adds: query versus verdict mode; path is wording versus another resx/text/C# path (C# included because it would be text under a passing proof, which the query must ignore); query alone versus with `--lane` or `--text-only`.

## Coverage first

`rg -n "wording-paths" src scripts` before writing: only the production command and `scripts/lane-check.sh`; no test. Which path is wording (exact paths, case-insensitive, other `.resx` are production) is already pinned by `LaneClassifierTests.ClassifyPathAssignsTheCategoryOfEachKindOfPath` and `ClassifyPathTreatsTheDiagnosticAndCodeFixResourcesAsWording` (04); the query tests use one mixed-case path only to pin that the spelling is written back as read, not to re-test the classifier. Malformed lines in verdict mode stay pinned by `ExecuteRejectsAMalformedChangeLine`; the new test pins the query mode, which returns before the verdict path.

## Not covered, and why

- **`--help` together with `--wording-paths`.** Help wins in both orders; unspecified convenience, no caller depends on it.
- **Deleted resource file (`D` line).** Listed like any other status; the README states no rule for deletions, and `--no-renames` already splits renames. Left to the spec author.
- **`lane-check.sh` passing the list to `verify-text-only.sh --exclude`, and `--exclude` itself.** Shell and file-based proof outside the solution; no harness (04, *Not covered*); the implementer demonstrated them (05, rows 1–8).
- **Failure classes.** None of the formatter, analyzer, code-fix, line-ending, or unsupported-shape classes reaches a command over name-status lines. FC-003 is applied above; FC-017: stdout is asserted in full, not by `Contains`.

## Commands

```shell
scripts/test.sh --project tooling --filter "FullyQualifiedName~Reihitsu.Tooling.Test.Unit.LaneCheckCommandTests"
```

Full tooling run on the changed tree: `Passed: 204` (195 + 9 new cases), no warning.

## Follow-up candidates

_None._
