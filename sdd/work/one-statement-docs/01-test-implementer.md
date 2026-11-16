Role: test implementer
Commit: f1d0c43
Verdict: #1470: no tests (text only); #1471: tests written; #1479: no tests (text only); #1544: tests written

## thoenissen/Reihitsu#1470

Text only; no tool test or architecture test should cover it. `TestScriptProjectSelectionTests` (architecture) already guards the `all)` branch of `scripts/test.sh` itself; which documents restate its coverage is prose the spec checker compares, with no mechanical anchor worth a test.

## thoenissen/Reihitsu#1471

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `Reihitsu.Tooling.Test.Unit.LaneClassifierTests.ClassifyPathTreatsTheRootCommunityFilesAsText` (`CODE_OF_CONDUCT.md`, `SECURITY.md` × proven text-only false/true) | — (tool) | `LaneClassifier.ClassifyPath` direct | fails, 4 rows: expected `Text`, actual `Unknown` |
| `Reihitsu.Tooling.Test.Unit.LaneClassifierTests.ClassifyPathClassifiesEveryRootMarkdownFile` | — (tool) | `ClassifyPath` over every `*.md` file found at the repository root (located by `Reihitsu.sln`) | fails: "Root Markdown files no lane rule classifies: SECURITY.md, CODE_OF_CONDUCT.md" |
| `Reihitsu.Tooling.Test.Unit.LaneCheckCommandTests.ExecuteConfirmsTheDocsLaneForARootCommunityFile` (`CODE_OF_CONDUCT.md`, `SECURITY.md`) | — (tool) | `LaneCheckCommand.Execute` with a one-line name-status diff, `--lane docs --text-only fail` | fails: exit code not `Success` |

Existing rows that bound the change and must stay green (both sides of the root-file condition): `ClassifyPathAssignsTheCategoryOfEachKindOfPath` rows `README.MD` → Text, `AGENTS.md` → Tooling, `CLAUDE.md` → Tooling, `scripts/README.md` → Tooling, `squad/lead.md` → Tooling, `.claude/agents/auditor.md` → Tooling, `notes.txt` → Unknown; `ClassifyPathTreatsTheRootContributingGuideAsText`; `ExecuteReportsNoLaneForAnUnknownPath`. No existing test asserts an output this issue changes; none was edited.

The expected category `Text` for the two files comes from the issue's "Done when" (a diff of only either file is confirmed for the docs lane) and from `squad/README.md`, Lanes, which lists Markdown under docs. The classification of Markdown under `scripts/`, `squad/`, `.claude/` as tooling is left as it is: existing tests assert it, and the issue leaves open which statement wins.

### Not covered, and why

- No test compares `scripts/README.md` or `squad/README.md` prose with the classifier: prose, for the spec checker.
- A hypothetical future root Markdown file is not tested by name; `ClassifyPathClassifiesEveryRootMarkdownFile` covers whatever exists at the root, without fixing how the classifier generalizes.
- Failure classes: the catalog targets formatter, analyzer and code-fix behavior; none reaches `LaneClassifier` (no syntax, trivia, line endings, or layout involved).

## thoenissen/Reihitsu#1479

Text only (`squad/failure-classes.md`); no tool test or architecture test should cover it.

## thoenissen/Reihitsu#1544

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `Reihitsu.ArchitectureTests.Formatter.PhaseOwnershipDocumentationTests.OwnershipMapGuardNamesResolveToDeclarations` | — (tool) | Reflection over the formatter and core assemblies, plus `SyntaxKind` | passes on this head |

It extracts every identifier-shaped backticked name (`Name` or `Type.Member`) from the guard column of the `formatter-phase-ownership` block in `AGENTS.md` and requires each to resolve: `Type.Member` to a declared member of a type of that name, a single name to a type, a member of any type, or a `SyntaxKind` member. Non-identifier entries (`?`, `:`, the spec path) are skipped. This covers the issue's "every backticked guard name resolves to a declaration under `src/`", which no test checked before (the existing `OwnershipMapMatchesRuntimePhaseOrder` checks the phase column only).

Checked that it fails for the right reason: a temporary edit of the `BlankLinePhase` row adding `` `BraceBlankLinePolicy` ``, `` `NoSuchPolicy.Decide` ``, and `` `BlankLineEditor.NoSuchMember` `` made it fail with "Guard names without a declaration: NoSuchPolicy.Decide, BlankLineEditor.NoSuchMember" — `BraceBlankLinePolicy` resolved. `AGENTS.md` was restored; the run commits no change to it.

### Not covered, and why

- No test asserts that the `BlankLinePhase` row names `BraceBlankLinePolicy`: a test pinning one name in one row restates the table instead of checking it; the spec checker verifies the row text.
- The `LineBreakPhase` keep-rule item is already met on this head (the row says "it keeps every blank line"; issue comment of 2026-11-16), so nothing to test.
- Failure classes: none reaches a documentation table.

## Whole run

### Commands

```shell
scripts/test.sh --project tooling --filter "FullyQualifiedName~LaneClassifierTests|FullyQualifiedName~LaneCheckCommandTests"
scripts/test.sh --project architecture --filter "FullyQualifiedName~PhaseOwnershipDocumentationTests"
```

Results on this head: tooling 76 passed, 7 failed (exactly the new #1471 tests and rows above); architecture 2 passed.

### Follow-up candidates

_None._
