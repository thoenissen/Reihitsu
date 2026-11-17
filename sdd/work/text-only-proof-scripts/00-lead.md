# Lead — text-only-proof-scripts

- Issues: thoenissen/Reihitsu#1808 (`agent-filed`), thoenissen/Reihitsu#1817 (`agent-filed`) — both goals are proposals.
- Slug: `text-only-proof-scripts`
- Branch: `claude/gifted-davinci-w1ond4` (session-designated); draft PR #1833
- Base at claim: `a971d89`

## Lane

`tooling` for the whole run.

- #1808: changes the classification inside the text-only proof (`scripts/proof/verify-text-only.cs`) and its description in `scripts/README.md` — repository tooling.
- #1817: changes the Roslyn pin of `scripts/proof/verify-text-only.cs` and the Roslyn-version guard in `src/Reihitsu.ArchitectureTests` (test path) — tooling plus tests.
- Shared mechanism: both edit `scripts/proof/verify-text-only.cs` and `scripts/README.md` (maintainer's planning comment on #1817 pairs them).

Roles: test implementer (for #1817 the Roslyn-version guard is a test; for #1808 the proof has no tests, so nothing), implementer, cleaner, spec checker, auditor, validator. Skipped: spec author and skeptic (tooling lane has none; no spec, rule page, or visible output is touched). `scripts/README.md` text is written by the implementer, as the tooling lane has no spec author.

## Maintainer clarifications

- #1817 comment: planned for 1.4.0, runs together with #1808 because both edit the same two files.

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Lane | Which option of #1808 | Markdown (`.md`) under `scripts/` counts as text for the proof, as everywhere else; every other file under `scripts/` and `.github/workflows/` stays behavioral; lane-check classification unchanged; `scripts/README.md`, *The text-only proof*, states it | Nothing in a Markdown file executes; the proof's question is "can this change affect compiled/executed behavior", and prose cannot. The repository already treats specs and rule pages, which also describe behavior, as text | Internal tooling only; no formatter, diagnostic, code-fix, rule page, or CLI output | — |
| 2 | Lane | Which option of #1817 | Bring the pin to the repository's Roslyn version (`RoslynVersion` in `Directory.Build.props`) and make the Roslyn-version guard fail when a `#:package Microsoft.CodeAnalysis*` directive of a file-based program drifts from it; check the proof's verdicts on a text-only and a non-text-only change before and after the bump | One Roslyn version is the repository principle; an exemption would leave the proof parsing newer syntax with an older parser | Internal tooling and tests only | — |
| 3 | Lane | Roles skipped | Spec author and skeptic skipped (tooling lane); test implementer runs for #1817 only | Lane table | — | — |
| 4 | Test implementer | A Roslyn `#:package` directive without a version (open in 01) | Report it as a violation like any other version that is not exactly `RoslynVersion`; the implementer adds the case to the guard and the test implementer's contract applies unchanged otherwise | A version-less directive floats, which is drift by the same rule that rejects `5.*` | Internal guard only | — |
| 5 | Implementer | README sentence on added/deleted files was already inaccurate | Accept the corrected sentence (describes what the code does; no code change) | Adjacent text fix in the paragraph the run rewrites anyway | Internal tooling doc | 7d2ecd5 |
| 6 | Implementer | Markdown under `.github/workflows/` also text | Accept | Same reason as decision 1: Markdown executes nowhere; no such file exists today | Internal tooling | 7d2ecd5 |
| 7 | Implementer | Version-less pin (decision 4) untested; stale summary of `RoslynPackageVersionTests` | Route both to a fresh test implementer | Every guard condition needs a test; the summary is the test implementer's file | Internal tests | — |
| 8 | Implementer | Follow-up candidate: a directive-less C# file under `scripts/` would get the comment-only check instead of the `scripts/` rule | Drop | No such file exists; every C# file under `scripts/` is a file-based program with `#:` directives, so no caller reaches the case | Internal tooling | — |
| 9 | Implementer | Follow-up candidate: `RoslynVersion` comment in `Directory.Build.props` does not mention `#:package` pins | Drop | The guard and `scripts/README.md` state the rule; the comment is not wrong, only incomplete | Internal | — |

## Follow-up candidates

(none yet)

## Worklist / next step

- Done: claim, lane, prepare.
- Done: test implementer, `01-test-implementer.md` at `816e7e4` (8 failing guard tests for #1817; nothing for #1808).
- Done: implementer, `02-implementer.md` at `7d2ecd5`.
- Done: test implementer amendment, `03-test-implementer.md` at `6931294`.
- Next: cleaner (`04-cleaner.md`).
