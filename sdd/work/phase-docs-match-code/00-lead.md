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
