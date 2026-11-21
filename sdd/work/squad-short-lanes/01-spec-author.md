Role: spec author
Commit: 374cf43
Verdict: change doc written

Process change: no spec under `sdd/specs/` changes, no requirement ID is added, and no rule page changes. The prompt carried no intended fix or verdict to ignore.

## Changed requirements

None — process change. Change doc: `sdd/changes/2026-11/squad-wording-lane-and-ci-only-watch.md` (`Status: Draft`, `Affected requirements: none — process change`), stating the five rules the maintainer accepted at triage of thoenissen/Reihitsu#1906.

Process text written:

| File | What it states |
|---|---|
| `scripts/README.md`, *The lane check* | The one statement of the classification and the lane rules: new category *Wording* (the two resource files, by exact path), ordered rules 1–5 that turn categories into a lane, the widening orders with `wording`, and the proof run with one `--exclude` per wording path |
| `scripts/README.md`, *The text-only proof* | New `--exclude <path>` option and the `; excluded: <n> path(s)` suffix of the proof line |
| `scripts/README.md`, *The line-count rows* (new, under *The default test run*) | Which self-hosting rows the validator runs and why the build and the default run miss them; example line added |
| `squad/README.md` | Roles table, lanes table row *Wording*, lane summaries, widening orders, batching point for lanes without test implementer and auditor, new section *Coverage first* (the one statement of that rule), push pointer in *Rules for every role* |
| `squad/lead.md` | §2 *Pushes* (stage boundaries), §3 wording lane pick, §4 two verdict-table rows, §5 batching pointer, §9 rewritten (subscription only until CI is decided) |
| `squad/roles/spec-author.md`, `skeptic.md`, `test-implementer.md`, `spec-checker.md`, `validator.md` | Wording-lane duty, coverage-first pointers, docs-and-wording wording, line-count rows step |
| `CLAUDE.md`, `.claude/agents/spec-author.md`, `.claude/agents/validator.md` | Matching sentence and descriptions |

### What the classifier must do (for the test implementer and implementer)

Stated in `scripts/README.md`, *The lane check*; summary only, the README governs:

- `ClassifyPath`: `src/Reihitsu.Analyzer/AnalyzerResources.resx` and `src/Reihitsu.Analyzer.CodeFixes/CodeFixResources.resx` (case-insensitive like the other rules) → new category `Wording`; every other `.resx` keeps its current category.
- Required lane, first rule that applies: unknown → none; production → as today (tooling without change doc → none; change doc → behavior-change; else bug-fix); tooling → with wording: change doc ? behavior-change : none, without wording: tooling; wording → with test: change doc ? behavior-change : tests, without test: wording; otherwise as today (change doc → behavior-change, test → tests, else docs).
- `Covers`: `wording` is covered by wording, tests, bug-fix, behavior-change, not by docs or tooling; `wording` covers only docs and itself. The `behavior-change` change-doc condition stays; a `wording` claim has no extra condition.
- Lane name `wording` in `TryParseLane`/`GetName`, the usage strings of `lane-check.cs`/`lane-check.sh`; exit 3 reason covers wording next to tooling without a change doc.
- `lane-check.sh` passes `--exclude <path>` to `verify-text-only.sh` for each changed wording path; `verify-text-only` gains the repeatable `--exclude` (exact, case-insensitive, forward-slash path; a path not in the diff excludes nothing) and the line suffix.

## Examples for the tests

None — no requirement. The classifier cases above, and the RH5201 run's diff (commit 3caebda: both resource files, rule page, spec, change doc, package README, a C# doc-comment edit, one test literal) as a realistic input: with its test file it needs `behavior-change`; without it, `wording`.

## Output questions

_None._ Nothing an end user sees changes.

## Internal choices

1. Wording category by the two exact file paths, not every `*.resx` under `src/`. Alternative: any non-test `.resx`. Recommended: exact paths, so a later resource file is decided when it is added.
2. The wording lane admits no test path; a test that quotes a changed text widens the run (`tests` without a change doc, `behavior-change` with one). Alternative: admit test files whose change is only a string literal, which needs a content check in the lane check. Recommended: no test path; the cause is a follow-up (below).
3. The text-only proof excludes the wording paths through a new `--exclude` option, so a C# comment repeating a changed title stays text. Alternatives: treat the resource files as text in the proof (wrong: they compile), or no exclusion (a wording diff with a comment edit becomes production).
4. A wording diff may carry a change doc; tooling next to wording needs a change doc (then `behavior-change`) and otherwise admits no lane, mirroring production next to tooling.
5. "Stage boundaries" for pushes defined as: claim, before every stop that waits for the maintainer, before ending a run that cannot go on, trigger commit, each CI repair. Alternative: also after each audit. Recommended as written.
6. The line-count rows trigger is kept to test files, as the maintainer stated it, though production files are judged by the same rows; the filter is `Name~RH2008|Name~RH2111` (18 tests, 41 s after a Release build here; the rows of every analyzer had not finished after nine minutes).
7. In the docs and wording lanes the one batch of output questions comes after the spec author, before the spec checker; in the wording lane the new texts are an output question unless a maintainer answer covers them.
8. After unsubscribing, a change the maintainer requests in chat is handled like a repair round with a fresh `sdd/work/<slug>/` and the run's note as the record, since `00-lead.md` is deleted at publish.

## Follow-up candidates

1. About twenty test files under `src/*.Test/` quote a diagnostic message as a literal (`rg -l 'DiagnosticId, *"' src --glob '*.cs'`, 21 files, 260 lines) instead of reading the analyzer's `MessageFormat`, which `AnalyzerResourcesTests` already ties to the resource. Each such literal forces a wording change out of the wording lane. Candidate: replace the literals with `MessageFormat` where the message has no placeholders. Not visible output.
2. The line-count rows also judge production files, which the build's analyzers do not judge with the repository's limits either; the validator trigger covers only test files as the maintainer stated. Candidate: widen the trigger to any C# file under `src/` whose diff adds lines. Process question for the maintainer, not visible output.
