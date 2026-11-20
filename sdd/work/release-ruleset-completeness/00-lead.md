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
| 3 | Auditor | Premise overstated: without an entry the 14 rules were already errors in Release via `TreatWarningsAsErrors`; Closes or Refs? | `Closes #1890`; the PR's Review notes state that the change makes each rule's Release action explicit rather than turning the rules on | The issue's goals (every shipped rule listed, a test for the missing direction) are met | Build configuration; the 14 rules already fail the Release build on main, so nothing a user sees changes | — |
| 4 | Auditor | Finding #1, test doc-comment text repeats the wrong premise | Route to the cleaner as comment-only; prove with verify-text-only against b7c14cbb | Comment-only finding after a passing audit (lead.md section 7) | Comment in a test | — |

## Follow-up candidates

(none yet)

## State

- Audited SHA: 0374545c (audit pass, text cleanup; handover b7c14cbb; origin/main 725ea151)
- Last step finished: cleaner clean (03), spec checker consistent (04), Release build green.
- Next step: cleaner fixes test comments (finding #1), verify-text-only vs b7c14cbb, then validator.
