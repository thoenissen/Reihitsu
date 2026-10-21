Role: lead
Commit: 8e0f35d
Verdict: lane claimed — tests

## Issue

thoenissen/Reihitsu#939 — tag the formatter and Playground tests with requirement IDs. Draft claim PR: thoenissen/Reihitsu#1358. Parent thoenissen/Reihitsu#899; question tracker thoenissen/Reihitsu#912 (at its sub-issue limit — new question issues are named in the run report instead of attached).

Slug: `tag-formatter-playground-tests`.

Branch: `claude/stoic-hawking-udrgwu` instead of `claude/issue-939-<slug>` — the session harness allows pushing only to this branch. Based on `origin/main` (8e0f35d).

## Lane

Specs and tests for existing behavior, tests only. Reason: the run adds `[Requirement]` tags, two `RequirementAttribute` declarations, focused tests where none covers a requirement, and extends `RequirementCoverageTests.EnforcedSpecPaths` (all in test projects). No spec text changes and no production code.

## Roles

| Role | Runs | Reason |
|---|---|---|
| Spec author | skipped | The run writes no spec; the maintainer forbade spec text edits under `sdd/specs/`. Open questions found by other roles are filed by the lead. |
| Skeptic | runs, split in three parallel agents by spec group | Checks the existing formatter and Playground specs against the code; 13 specs plus Playground are too large for one agent. |
| Test implementer | runs: one prelude agent for the attributes, then three parallel agents by spec group, then the enforcement switch | Tags existing tests, writes focused tests where no test covers a requirement. |
| Implementer | skipped | Lane has no production change. |
| Cleaner | runs | Formats changed files, Release build warning-free. |
| Spec checker | runs | Verifies coverage and open-question filing. |
| Auditor | skipped | Not in this lane. |
| Validator | runs | Full build and tests. |

Spec groups:

- **A**: `formatter/pipeline.md`, `formatter/indentation.md`, `formatter/raw-string-alignment.md`, `formatter/cleanup.md`, `formatter/line-endings.md`, `playground/playground.md`.
- **B**: `formatter/line-breaks.md`, `formatter/switch-case-braces.md`, `formatter/horizontal-spacing.md`.
- **C**: `formatter/structural-transforms.md`, `formatter/blank-lines.md`, `formatter/documentation-comments.md`, `formatter/region-formatting.md`, `formatter/using-directives.md`.

## Maintainer clarifications (summarized)

- Every `shared/` spec and `cli/` are already covered; all 13 `formatter/` specs and `playground/playground.md` are uncovered. `Reihitsu.Formatter.Test` and `Reihitsu.Playground.Test` get their own `RequirementAttribute`, modeled on the Analyzer, CLI, and Core test projects.
- Add `formatter/` and `playground/` to `RequirementCoverageTests.EnforcedSpecPaths` once covered.
- Tag existing tests that prove a requirement; write a focused test only where none covers it. Tests that pass through a requirement incidentally are not tagged.
- Do not implement thoenissen/Reihitsu#998 (opt-out switch, every-test-tagged rule).
- thoenissen/Reihitsu#1138 edits FMT-PIPE-010's `Why:` in parallel; no spec text is edited by this run.
- thoenissen/Reihitsu#1148 (FMT-BLANK-011 scope) and thoenissen/Reihitsu#1149 (FMT-BREAK-016 enum members) are decided but not implemented: their current behavior gets characterization tests, no new requirements.
- A requirement that is wrong about current behavior is filed as an open question in `SDD open questions: <spec path>`, never edited. A new instance of a decided principle (P1–P31, see thoenissen/Reihitsu#912) is filed marked "Decided by principle Pn"; a further instance of an existing follow-up issue gets a one-line comment on that issue instead.
- No new requirement IDs. No retiring, moving, or renumbering IDs without escalating. No production code change, no bug fix. No issues besides open-question issues without approval of the exact text.
- Other squads run in parallel on `shared/` specs and `sdd/README.md`; their files are not touched.
