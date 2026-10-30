Role: lead
Commit: 3da05b7
Verdict: lane claimed — tooling

## Run

- Issue: thoenissen/Reihitsu#1484 (package A4 of thoenissen/Reihitsu#1482).
- Slug: `phase-docs-match-code`.
- Branch: `claude/squad-1484-2rgo8u` — the branch this session was assigned; it stands in for `claude/issue-1484-<slug>`.
- Draft PR: thoenissen/Reihitsu#1506.

## Lane

**Tooling.** The issue changes the formatter-phase-ownership table in `AGENTS.md` (a tooling path), clarifies three formatter specs, and adds covering tests in `src/Reihitsu.Formatter.Test/`. No production file changes. The narrowest lane admitting `AGENTS.md` together with specs and tests is tooling.

## Roles

| Role | Runs | Reason |
|---|---|---|
| Spec author | yes (added) | The run rewords spec requirements (FMT-SWITCH-003, FMT-PIPE-005/006/008), corrects `line-breaks.md`, and corrects the guard names in the `AGENTS.md` ownership table — all text. The tooling lane does not list it, but the text has to be written by its owner. |
| Skeptic | yes (added) | The issue was written against an older `main` and asks the run to check every statement against current `main`; the skeptic checks the spec diff and the table against the code. |
| Test implementer | yes | Covering tests for the clarified requirements. |
| Implementer | skipped | No production or tooling code changes; the covering tests describe existing behavior. A covering test that fails is escalated, not implemented. |
| Cleaner | yes | |
| Spec checker | yes | |
| Auditor | yes | |
| Validator | yes | |

## Maintainer clarifications

None so far.

## Escalations

1. Escalation 4 (test implementer, `03-test-implementer.md`): FMT-PIPE-008's detached-node clause is wrong for a node parsed with explicit parse options. The maintainer chose to narrow the clause to the code: a node parsed with parse options keeps them, a node parsed or built without them gets the default parse options; the covering test expects the C# 11 result for a node parsed with C# 11 options. Recorded in `squad/decisions/2026-10-30-phase-docs-match-code-detached-node-version.md`.
2. Escalation 7 (auditor, `08-auditor.md`, finding #1 part b): the stale public remarks of `ReihitsuFormatter.FormatNode` may be corrected in this run as a comment-only edit; the lane stays tooling, and a lane-check rejection caused only by that edit is accepted by the maintainer and stated in the run report. Recorded in `squad/decisions/2026-10-30-phase-docs-match-code-formatnode-remarks.md`.

## Repair round 1

Worklist from `08-auditor.md`, finding #1 (same requirement / PR-introduced):

- (a) FMT-PIPE-008 `FormatNode` wording — spec author.
- (c) test summary in `ReihitsuFormatterLanguageVersionTests.cs`, and any covering test the reworded clause needs — test implementer.
- (b) public remarks of `ReihitsuFormatter.FormatNode`, comment only, proven with `scripts/verify-text-only.sh` on that file — cleaner.
