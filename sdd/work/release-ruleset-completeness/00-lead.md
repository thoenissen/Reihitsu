# Lead — release-ruleset-completeness

- Issue: thoenissen/Reihitsu#1890 — `agent-filed` (goals are proposals). Labels: maintenance, agent-filed. Milestone 1.5.0.
- Branch: `claude/squad-1890-jlxazs` (session-designated). Draft PR: #1899.
- Slug: `release-ruleset-completeness`
- Lane: **tooling**. Reason: the change touches `Release.ruleset` (a `*.ruleset` file, tooling in the lane-check table) and a test under a test project; no production path and no change doc.
- Roles run: test implementer (the ruleset has self-hosting tests in `src/Reihitsu.SelfHosting.Test/Repository/RulesetTests.cs`), implementer, cleaner, spec checker, auditor, validator; issue manager before publishing if follow-up candidates exist.
- Roles skipped: spec author and skeptic (the tooling lane does not run them).

## Maintainer clarifications (summarized)

- Triage comment on #1890 (maintainer decision, 2026-11-20): reading `Release.ruleset` showed 14 shipped rules missing, not only RH5417 — RH5113, RH5308, RH5417, RH6023, RH6024, RH7110, RH7308–RH7311, RH7409–RH7412. The run re-checks that list on current `main`, adds every missing rule, and adds the test for the missing direction. It was to start after the run for #892 merged; #892 is closed (PR #1892 merged, which is `origin/main` at the claim, 725ea15), so rules added there (RH5115, RH5116) are part of the re-check.

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Test implementer | Is RH7305A exempt from the completeness test, or listed in `Release.ruleset` as `None`? | Keep it on the commented exemption list | RH7305A is the alternative to RH7305, which Release enables; the exemption list is what the issue asked for, and the exemption test keeps it from going stale | Repository build configuration and tests only; no shipped surface changes | 403719dd |
| 2 | Test implementer | Merge the 14 new error checks into the existing per-rule error test (renamed `ReleaseRulesetReportsRuleAsError`) | Accept | S4144 forbids two identical methods; nothing else uses the old name | Test structure only | 403719dd |

## Follow-up candidates

(none yet)

## State

- Audited SHA: —
- Last step finished: test implementer — reproduced (01-test-implementer.md): the 14 rules of the triage list are missing; RH5115/RH5116 present.
- Next step: implementer.
